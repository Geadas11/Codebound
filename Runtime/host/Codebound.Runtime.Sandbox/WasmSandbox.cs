using System.Diagnostics;
using System.Text;
using Wasmtime;

namespace Codebound.Runtime.Sandbox;

/// <summary>
/// Runs WASI command modules (e.g. CPython compiled to wasm32-wasi) in an isolated Wasmtime store.
/// Isolation: no host directories are pre-opened, no network, no host environment variables.
/// Limits: fuel (CPU), memory (store limiter) and wall clock (epoch interruption).
/// One <see cref="WasmSandbox"/> per runtime module; concurrent <see cref="Run"/> calls use separate stores.
/// </summary>
public sealed class WasmSandbox : IDisposable
{
    private const int EpochMs = 10;

    private static readonly Lazy<Engine> SharedEngine = new(() => new Engine(new Config()
        .WithFuelConsumption(true)
        .WithEpochInterruption(true)));

    private readonly Module _module;
    private readonly Timer _epochTicker;

    public WasmSandbox(string wasmPath)
    {
        if (!File.Exists(wasmPath)) throw new FileNotFoundException("Runtime module not found", wasmPath);
        Engine = SharedEngine.Value;
        _module = LoadModuleCached(Engine, wasmPath);
        // Advance the engine epoch periodically; each store sets a deadline in epochs for its wall-clock limit.
        _epochTicker = new Timer(_ => Engine.IncrementEpoch(), null, EpochMs, EpochMs);
    }

    public Engine Engine { get; }

    /// <param name="args">argv including argv[0].</param>
    /// <param name="stdin">Text given to the program on standard input.</param>
    public SandboxResult Run(IReadOnlyList<string> args, string stdin, SandboxLimits limits)
    {
        var sw = Stopwatch.StartNew();
        string dir = Path.Combine(Path.GetTempPath(), "codebound-sbx-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        string inPath = Path.Combine(dir, "stdin"), outPath = Path.Combine(dir, "stdout"), errPath = Path.Combine(dir, "stderr");
        File.WriteAllText(inPath, stdin ?? string.Empty, new UTF8Encoding(false));
        File.WriteAllText(outPath, string.Empty);
        File.WriteAllText(errPath, string.Empty);

        SandboxStatus status = SandboxStatus.Completed;
        int exitCode = 0;
        string? detail = null;
        ulong fuelUsed = 0;
        try
        {
            using var store = new Store(Engine);
            store.SetWasiConfiguration(new WasiConfiguration()
                .WithArgs(args.ToArray())
                .WithStandardInput(inPath)
                .WithStandardOutput(outPath)
                .WithStandardError(errPath));
            store.SetLimits(memorySize: limits.MemoryBytes, tableElements: 100_000, instances: 4, tables: 4, memories: 2);
            store.Fuel = limits.Fuel;
            store.SetEpochDeadline((ulong)Math.Max(1, limits.WallClock.TotalMilliseconds / EpochMs));

            using var linker = new Linker(Engine);
            linker.DefineWasi();
            try
            {
                var instance = linker.Instantiate(store, _module);
                var start = instance.GetAction("_start") ?? throw new InvalidOperationException("module has no _start");
                start();
            }
            catch (WasmtimeException e)
            {
                (status, exitCode, detail) = Classify(e, store);
            }
            fuelUsed = limits.Fuel - SafeFuel(store);
        }
        catch (Exception e)
        {
            status = SandboxStatus.HostError;
            detail = e.Message;
        }

        var result = new SandboxResult(status, exitCode,
            ReadCapped(outPath, limits.MaxOutputBytes), ReadCapped(errPath, limits.MaxOutputBytes),
            fuelUsed, sw.Elapsed, detail);
        TryDelete(dir);
        return result;
    }

    private static (SandboxStatus, int, string?) Classify(WasmtimeException e, Store store)
    {
        if (e is TrapException trap)
        {
            if (trap.Type == TrapCode.OutOfFuel) return (SandboxStatus.OutOfFuel, -1, "CPU budget exhausted");
            if (trap.Type == TrapCode.Interrupt) return (SandboxStatus.Timeout, -1, "wall-clock limit reached");
        }
        // WASI proc_exit(n) surfaces as an exception carrying the exit status.
        var exit = TryParseExit(e);
        if (exit.HasValue) return (exit.Value == 0 ? SandboxStatus.Completed : SandboxStatus.Failed, exit.Value, null);
        if (SafeFuel(store) == 0) return (SandboxStatus.OutOfFuel, -1, "CPU budget exhausted");
        var msg = e.Message ?? string.Empty;
        if (msg.Contains("memory", StringComparison.OrdinalIgnoreCase))
            return (SandboxStatus.OutOfMemory, -1, msg);
        return (SandboxStatus.HostError, -1, msg);
    }

    private static int? TryParseExit(WasmtimeException e)
    {
        var prop = e.GetType().GetProperty("ExitCode") ?? e.GetType().GetProperty("ExitStatus");
        if (prop?.GetValue(e) is int code) return code;
        var m = System.Text.RegularExpressions.Regex.Match(e.Message ?? "", @"exit status (\d+)");
        return m.Success ? int.Parse(m.Groups[1].Value) : null;
    }

    private static ulong SafeFuel(Store store)
    {
        try { return store.Fuel; } catch { return 0; }
    }

    private static string ReadCapped(string path, int max)
    {
        try
        {
            using var fs = File.OpenRead(path);
            var buf = new byte[Math.Min(max, (int)Math.Min(fs.Length, int.MaxValue))];
            int n = fs.Read(buf, 0, buf.Length);
            var s = Encoding.UTF8.GetString(buf, 0, n);
            return fs.Length > max ? s + "\n[output truncated]" : s;
        }
        catch { return string.Empty; }
    }

    /// <summary>Compiling a large runtime (CPython ≈ 26 MB) is slow; cache the native code next to the module.</summary>
    private static Module LoadModuleCached(Engine engine, string wasmPath)
    {
        string cache = wasmPath + ".cwasm";
        if (File.Exists(cache) && File.GetLastWriteTimeUtc(cache) >= File.GetLastWriteTimeUtc(wasmPath))
        {
            try { return Module.DeserializeFile(engine, Path.GetFileName(wasmPath), cache); }
            catch { /* stale or incompatible cache: recompile */ }
        }
        var module = Module.FromFile(engine, wasmPath);
        try { File.WriteAllBytes(cache, module.Serialize()); } catch { /* cache is optional */ }
        return module;
    }

    private static void TryDelete(string dir)
    {
        try { Directory.Delete(dir, true); } catch { }
    }

    public void Dispose()
    {
        _epochTicker.Dispose();
        _module.Dispose();
    }
}
