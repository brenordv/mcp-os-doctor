namespace McpOsDoctor.Core.Models;

/// <summary>
/// Point-in-time snapshot of the system's hardware and operating system state.
/// </summary>
public record SystemSnapshot
{
    /// <summary>
    /// Machine hostname.
    /// </summary>
    public required string Hostname { get; init; }

    /// <summary>
    /// Operating system version string.
    /// </summary>
    public required string OsVersion { get; init; }

    /// <summary>
    /// Processor architecture (e.g., "X64", "ARM64").
    /// </summary>
    public required string OsArchitecture { get; init; }

    /// <summary>
    /// Name of the primary processor.
    /// </summary>
    public required string ProcessorName { get; init; }

    /// <summary>
    /// Number of logical processor cores.
    /// </summary>
    public required int ProcessorCores { get; init; }

    /// <summary>
    /// Total installed RAM in gigabytes.
    /// </summary>
    public required double TotalMemoryGb { get; init; }

    /// <summary>
    /// Available (free) RAM in gigabytes.
    /// </summary>
    public required double AvailableMemoryGb { get; init; }

    /// <summary>
    /// System uptime in hours since last boot.
    /// </summary>
    public required double UptimeHours { get; init; }

    /// <summary>
    /// Whether the server process is running with elevated privileges.
    /// </summary>
    public required bool IsElevated { get; init; }

    /// <summary>
    /// Information about mounted disk volumes.
    /// </summary>
    public required IReadOnlyList<DiskVolumeInfo> DiskVolumes { get; init; }
}