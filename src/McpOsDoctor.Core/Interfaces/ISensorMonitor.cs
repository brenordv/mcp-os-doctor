using McpOsDoctor.Core.Enums;
using McpOsDoctor.Core.Models;

namespace McpOsDoctor.Core.Interfaces;

/// <summary>
/// Manages background hardware sensor monitoring with periodic snapshots persisted to disk.
/// </summary>
public interface ISensorMonitor
{
    /// <summary>
    /// Current monitoring state.
    /// </summary>
    MonitoringState State { get; }

    /// <summary>
    /// When the current or last monitoring session started, if any.
    /// </summary>
    DateTimeOffset? StartedAt { get; }

    /// <summary>
    /// Polling interval in seconds for the current or last session.
    /// </summary>
    int IntervalSeconds { get; }

    /// <summary>
    /// Number of snapshots collected in the current session.
    /// </summary>
    int SnapshotCount { get; }

    /// <summary>
    /// Starts background sensor monitoring at the specified polling interval.
    /// Takes the first snapshot immediately and persists all snapshots to a JSONL file.
    /// </summary>
    /// <param name="intervalSeconds">Seconds between each sensor poll (1–3600).</param>
    /// <exception cref="Errors.McpOsDoctorException">
    /// Thrown when monitoring is already running or the interval is out of range.
    /// </exception>
    void Start(int intervalSeconds);

    /// <summary>
    /// Stops the background monitoring loop and releases hardware resources.
    /// Collected data remains available on disk for <see cref="GetResults"/>.
    /// </summary>
    void Stop();

    /// <summary>
    /// Reads all persisted snapshots and returns aggregated per-sensor statistics.
    /// Works whether monitoring is running, stopped, or after a crash recovery.
    /// </summary>
    /// <param name="hardwareType">Optional hardware category filter (e.g., "cpu", "gpu").</param>
    /// <param name="sensorType">Optional sensor category filter (e.g., "temperature", "fan").</param>
    /// <returns>A summary with min/max/average/current statistics per sensor.</returns>
    /// <exception cref="Errors.McpOsDoctorException">Thrown when a filter value is invalid.</exception>
    SensorMonitoringSummary GetResults(string hardwareType, string sensorType);
}