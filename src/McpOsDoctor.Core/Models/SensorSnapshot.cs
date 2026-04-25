namespace McpOsDoctor.Core.Models;

/// <summary>
/// A complete set of sensor readings captured at a single moment in time.
/// Serialized as one JSON line in the monitoring data file.
/// </summary>
public record SensorSnapshot
{
    /// <summary>
    /// When this snapshot was captured.
    /// </summary>
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// Sensor readings grouped by hardware component.
    /// </summary>
    public required IReadOnlyList<HardwareSensorGroup> Hardware { get; init; }
}