namespace Codebound.Runtime.Sandbox;

/// <summary>Resource budget of one program run. Maps to the in-game computer's hardware.</summary>
public sealed record SandboxLimits
{
    /// <summary>Wasm "fuel" (≈ instructions) the program may consume. In-game CPU budget.</summary>
    public ulong Fuel { get; init; } = 2_000_000_000;
    /// <summary>Maximum linear memory in bytes. In-game RAM.</summary>
    public long MemoryBytes { get; init; } = 64L * 1024 * 1024;
    /// <summary>Hard wall-clock limit, independent of fuel (protects against host-side stalls).</summary>
    public TimeSpan WallClock { get; init; } = TimeSpan.FromSeconds(10);
    /// <summary>Maximum captured stdout/stderr bytes (rest is truncated).</summary>
    public int MaxOutputBytes { get; init; } = 64 * 1024;
}

public enum SandboxStatus
{
    Completed,      // program ended normally (exit code 0)
    Failed,         // program ended with a non-zero exit code (e.g. uncaught exception)
    OutOfFuel,      // CPU budget exhausted (infinite loop or too slow)
    OutOfMemory,    // memory limit reached
    Timeout,        // wall-clock limit reached
    HostError       // the sandbox itself failed (bad module, IO...)
}

public sealed record SandboxResult(
    SandboxStatus Status,
    int ExitCode,
    string Stdout,
    string Stderr,
    ulong FuelUsed,
    TimeSpan Elapsed,
    string? Detail = null);
