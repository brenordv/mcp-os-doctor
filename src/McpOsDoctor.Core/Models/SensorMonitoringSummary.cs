using McpOsDoctor.Core.Enums;

namespace McpOsDoctor.Core.Models;

/// <summary>
/// Complete monitoring result containing session metadata and per-sensor statistics.
/// </summary>
public record SensorMonitoringSummary
{
    /// <summary>
    /// Current monitoring state.
    /// </summary>
    public required MonitoringState State { get; init; }

    /// <summary>
    /// When the current or last monitoring session started.
    /// </summary>
    public DateTimeOffset? StartedAt { get; init; }

    /// <summary>
    /// Total number of sensor snapshots collected.
    /// </summary>
    public required int SnapshotCount { get; init; }

    /// <summary>
    /// Polling interval in seconds.
    /// </summary>
    public required int IntervalSeconds { get; init; }

    /// <summary>
    /// Per-hardware aggregated sensor statistics.
    /// </summary>
    public required IReadOnlyList<HardwareSensorSummary> Hardware { get; init; }
}