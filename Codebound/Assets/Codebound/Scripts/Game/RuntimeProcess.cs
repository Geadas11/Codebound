using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Codebound.Game
{
    /// <summary>One out-of-process language runtime speaking the line-based JSON protocol on stdin/stdout.</summary>
    public sealed class RuntimeProcess
    {
        readonly Process process;
        public volatile bool Eof;   // stdout closed: every message the process wrote has been queued

        RuntimeProcess(Process p) { process = p; }

        public static RuntimeProcess Start(string exe, string args, string workingDir, ConcurrentQueue<(RuntimeProcess, string)> inbox, ConcurrentQueue<string> errors,
                                           System.Collections.Generic.IDictionary<string, string> env = null)
        {
            var psi = new ProcessStartInfo
            {
                FileName = exe, Arguments = args, WorkingDirectory = workingDir,
                UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8,
            };
            if (env != null) foreach (var kv in env) psi.Environment[kv.Key] = kv.Value;
            var rp = new RuntimeProcess(Process.Start(psi));
            var p = rp.process;
            new Thread(() =>
            {
                try { string l; while ((l = p.StandardOutput.ReadLine()) != null) inbox.Enqueue((rp, l)); } catch { }
                rp.Eof = true;
            }) { IsBackground = true, Name = "cb-runtime-out" }.Start();
            new Thread(() => { try { string l; while ((l = p.StandardError.ReadLine()) != null) errors.Enqueue(l); } catch { } })
                { IsBackground = true, Name = "cb-runtime-err" }.Start();
            return rp;
        }

        // ---------------------------------------------------------------- Windows Job Object
        [StructLayout(LayoutKind.Sequential)]
        struct BasicLimits
        {
            public long PerProcessUserTimeLimit, PerJobUserTimeLimit;
            public uint LimitFlags;
            public UIntPtr MinimumWorkingSetSize, MaximumWorkingSetSize;
            public uint ActiveProcessLimit;
            public UIntPtr Affinity;
            public uint PriorityClass, SchedulingClass;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct ExtendedLimits
        {
            public BasicLimits Basic;
            public ulong ReadOps, WriteOps, OtherOps, ReadBytes, WriteBytes, OtherBytes;   // IO_COUNTERS
            public UIntPtr ProcessMemoryLimit, JobMemoryLimit, PeakProcessMemoryUsed, PeakJobMemoryUsed;
        }

        [DllImport("kernel32.dll", SetLastError = true)] static extern IntPtr CreateJobObject(IntPtr attributes, string name);
        [DllImport("kernel32.dll", SetLastError = true)] static extern bool SetInformationJobObject(IntPtr job, int infoClass, ref ExtendedLimits info, int size);
        [DllImport("kernel32.dll", SetLastError = true)] static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

        IntPtr job;   // kept open for the life of the game: KILL_ON_JOB_CLOSE ends the runtime if the game dies

        /// <summary>Windows: puts the process in a Job Object with a memory cap, no child processes, and death with the game.
        /// Returns false on other platforms or on failure (the caller logs a warning).</summary>
        public bool Confine(ulong memoryBytes, uint maxProcesses = 1) => (job = ConfineProcess(process, memoryBytes, maxProcesses)) != IntPtr.Zero;

        static IntPtr ConfineProcess(Process process, ulong memoryBytes, uint maxProcesses)
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT) return IntPtr.Zero;
            try
            {
                var job = CreateJobObject(IntPtr.Zero, null);
                if (job == IntPtr.Zero) return IntPtr.Zero;
                var info = new ExtendedLimits
                {
                    Basic = new BasicLimits { LimitFlags = 0x100 | 0x8 | 0x2000 | 0x400, ActiveProcessLimit = maxProcesses },   // process memory, active processes, kill on close, die on unhandled exception
                    ProcessMemoryLimit = new UIntPtr(memoryBytes),
                };
                return SetInformationJobObject(job, 9, ref info, Marshal.SizeOf(typeof(ExtendedLimits))) && AssignProcessToJobObject(job, process.Handle) ? job : IntPtr.Zero;
            }
            catch (Exception) { return IntPtr.Zero; }
        }

        [DllImport("kernel32.dll", SetLastError = true)] static extern bool TerminateJobObject(IntPtr job, uint exitCode);
        [DllImport("kernel32.dll", SetLastError = true)] static extern bool CloseHandle(IntPtr h);

        /// <summary>Runs a tool (e.g. the C compiler) to completion inside a Job Object with a time limit.
        /// Returns (exit code, stderr); exit code -1 means it was killed for taking too long. Thread-safe, no Unity API.</summary>
        public static (int code, string stderr) RunConfined(string exe, string args, string workingDir, int timeoutMs, ulong memoryBytes, uint maxProcesses)
        {
            var psi = new ProcessStartInfo
            {
                FileName = exe, Arguments = args, WorkingDirectory = workingDir, UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardOutput = true, RedirectStandardError = true, StandardErrorEncoding = Encoding.UTF8, StandardOutputEncoding = Encoding.UTF8,
            };
            using (var p = Process.Start(psi))
            {
                var job = ConfineProcess(p, memoryBytes, maxProcesses);
                var err = p.StandardError.ReadToEndAsync();
                var outp = p.StandardOutput.ReadToEndAsync();
                bool done = p.WaitForExit(timeoutMs);
                if (!done) { if (job != IntPtr.Zero) TerminateJobObject(job, 1); try { p.Kill(); } catch { } p.WaitForExit(2000); }
                if (job != IntPtr.Zero) CloseHandle(job);   // kills anything left in the job
                string text = (err.Wait(2000) ? err.Result : "") + (outp.Wait(500) ? outp.Result : "");
                return (done ? p.ExitCode : -1, text);
            }
        }

        public bool HasExited { get { try { return process.HasExited; } catch { return true; } } }

        /// <summary>Private (committed) memory of the runtime process in MB, measured from outside.</summary>
        public double PrivateMb { get { try { process.Refresh(); return process.PrivateMemorySize64 / 1048576.0; } catch { return 0; } } }
        public int ExitCode { get { try { return process.ExitCode; } catch { return -1; } } }

        /// <summary>Writes one protocol line. Returns false if the process is gone.</summary>
        public bool Send(string json)
        {
            try
            {
                var bytes = Encoding.UTF8.GetBytes(json + "\n");
                process.StandardInput.BaseStream.Write(bytes, 0, bytes.Length);
                process.StandardInput.BaseStream.Flush();
                return true;
            }
            catch (Exception) { return false; }
        }

        /// <summary>Closes stdin (the runtime exits by itself) and kills it if it does not within the grace period.</summary>
        public void Shutdown(int graceMs)
        {
            try { process.StandardInput.Close(); } catch { }
            try { if (graceMs <= 0 || !process.WaitForExit(graceMs)) process.Kill(); } catch { }
        }
    }
}
