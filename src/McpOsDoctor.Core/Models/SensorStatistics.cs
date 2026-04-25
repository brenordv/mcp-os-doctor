using McpOsDoctor.Core.Enums;

namespace McpOsDoctor.Core.Models;

/// <summary>
/// Aggregated statistics for a single sensor across all collected snapshots.
/// </summary>
public record SensorStatistics
{
    /// <summary>
    /// Sensor name as reported by the hardware.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The type of measurement this sensor provides.
    /// </summary>
    public required SensorCategory Category { get; init; }

    /// <summary>
    /// The most recently observed value.
    /// </summary>
    public required float Current { get; init; }

    /// <summary>
    /// The minimum value observed across all snapshots.
    /// </summary>
    public required float Min { get; init; }

    /// <summary>
    /// The maximum value observed across all snapshots.
    /// </summary>
    public required float Max { get; init; }

    /// <summary>
    /// The arithmetic mean across all snapshots.
    /// </summary>
    public required float Average { get; init; }
}