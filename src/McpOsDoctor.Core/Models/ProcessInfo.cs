namespace McpOsDoctor.Core.Models;

/// <summary>
/// Normalized information about a running process.
/// </summary>
public record ProcessInfo
{
    /// <summary>
    /// Process identifier assigned by the operating system.
    /// </summary>
    public required int Pid { get; init; }

    /// <summary>
    /// Name of the process executable.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Percentage of CPU time consumed by this process.
    /// </summary>
    public required double CpuPercent { get; init; }

    /// <summary>
    /// Working set memory usage in megabytes.
    /// </summary>
    public required double MemoryMb { get; init; }

    /// <summary>
    /// When the process started, if available.
    /// </summary>
    public DateTimeOffset? StartTime { get; init; }

    /// <summary>
    /// Sanitized command line used to launch the process.
    /// </summary>
    public string CommandLine { get; init; }
}