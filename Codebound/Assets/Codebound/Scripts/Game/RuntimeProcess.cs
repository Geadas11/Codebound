using System;
using System.Collections.Concurrent;
using System.Diagnostics;
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

        public static RuntimeProcess Start(string exe, string args, string workingDir, ConcurrentQueue<(RuntimeProcess, string)> inbox, ConcurrentQueue<string> errors)
        {
            var psi = new ProcessStartInfo
            {
                FileName = exe, Arguments = args, WorkingDirectory = workingDir,
                UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8,
            };
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
