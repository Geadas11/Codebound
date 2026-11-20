using Codebound.Runtime.Sandbox;
using NUnit.Framework;

namespace Codebound.Runtime.Tests;

/// <summary>
/// Proves the sandbox with real CPython (wasm32-wasi). Needs the runtime module; path from
/// CODEBOUND_PYTHON_WASM or Runtime/langs/python/python.wasm (see Runtime/langs/python/README.md).
/// </summary>
[TestFixture]
public class PythonSandboxTests
{
    private static WasmSandbox? _python;

    [OneTimeSetUp]
    public void Load()
    {
        var path = Environment.GetEnvironmentVariable("CODEBOUND_PYTHON_WASM");
        if (string.IsNullOrEmpty(path))
        {
            var dir = TestContext.CurrentContext.TestDirectory;
            path = Path.GetFullPath(Path.Combine(dir, "../../../../../langs/python/python.wasm"));
        }
        if (!File.Exists(path)) Assert.Ignore($"python.wasm not found at {path}");
        _python ??= new WasmSandbox(path);
    }

    private static SandboxResult Py(string code, SandboxLimits? limits = null, string stdin = "") =>
        _python!.Run(new[] { "python", "-c", code }, stdin, limits ?? new SandboxLimits());

    [Test]
    public void RunsRealPython()
    {
        var r = Py("import sys\nprint(sum(i*i for i in range(10)), sys.version_info[:2])");
        Assert.AreEqual(SandboxStatus.Completed, r.Status, r.Stderr + r.Detail);
        StringAssert.StartsWith("285 (3, 12)", r.Stdout);
        Assert.Greater(r.FuelUsed, 0UL);
    }

    [Test]
    public void ReadsStdin()
    {
        var r = Py("import sys\nprint(sys.stdin.read().upper())", stdin: "minerio");
        Assert.AreEqual(SandboxStatus.Completed, r.Status, r.Stderr);
        StringAssert.Contains("MINERIO", r.Stdout);
    }

    [Test]
    public void UncaughtExceptionIsReportedAsFailure()
    {
        var r = Py("x = 1 / 0");
        Assert.AreEqual(SandboxStatus.Failed, r.Status, r.Detail);
        Assert.AreNotEqual(0, r.ExitCode);
        StringAssert.Contains("ZeroDivisionError", r.Stderr);
    }

    [Test]
    public void InfiniteLoopIsStoppedByCpuBudget()
    {
        var r = Py("while True:\n    pass", new SandboxLimits { Fuel = 300_000_000, WallClock = TimeSpan.FromSeconds(30) });
        Assert.AreEqual(SandboxStatus.OutOfFuel, r.Status, r.Detail);
        Assert.Less(r.Elapsed, TimeSpan.FromSeconds(30));
    }

    [Test]
    public void InfiniteLoopIsStoppedByWallClock()
    {
        var r = Py("while True:\n    pass", new SandboxLimits { Fuel = ulong.MaxValue / 2, WallClock = TimeSpan.FromMilliseconds(1500) });
        Assert.AreEqual(SandboxStatus.Timeout, r.Status, r.Detail);
        Assert.Less(r.Elapsed, TimeSpan.FromSeconds(10));
    }

    [Test]
    public void MemoryLimitIsEnforced()
    {
        var r = Py("x = bytearray(200 * 1024 * 1024)\nprint('allocated')", new SandboxLimits { MemoryBytes = 48L * 1024 * 1024 });
        Assert.AreNotEqual(SandboxStatus.Completed, r.Status);
        StringAssert.DoesNotContain("allocated", r.Stdout);
        StringAssert.Contains("MemoryError", r.Stderr + r.Detail);
    }

    [Test]
    public void NoHostDirectoriesAreMounted()
    {
        // The sandbox pre-opens no host directories, so the program sees an empty root.
        var r = Py("import os\ntry:\n    print('entries', len(os.listdir('/')))\nexcept OSError as e:\n    print('entries', 0, type(e).__name__)");
        Assert.AreEqual(SandboxStatus.Completed, r.Status, r.Stderr + r.Detail);
        StringAssert.StartsWith("entries 0", r.Stdout);
    }

    [Test]
    public void CpuBudgetReflectsWorkDone()
    {
        var small = Py("s = 0\nfor i in range(1000): s += i");
        var big = Py("s = 0\nfor i in range(200000): s += i");
        Assert.AreEqual(SandboxStatus.Completed, big.Status, big.Stderr);
        Assert.Greater(big.FuelUsed, small.FuelUsed * 5, "inefficient programs must cost more in-game CPU");
    }
}
