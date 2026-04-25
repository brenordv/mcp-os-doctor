using McpOsDoctor.Core.Enums;

namespace McpOsDoctor.Core.Models;

/// <summary>
/// Aggregated sensor statistics for a single hardware component.
/// </summary>
public record HardwareSensorSummary
{
    /// <summary>
    /// Hardware component name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The type of hardware component.
    /// </summary>
    public required HardwareCategory Category { get; init; }

    /// <summary>
    /// Per-sensor statistics (min, max, average, current) for this hardware component.
    /// </summary>
    public required IReadOnlyList<SensorStatistics> Sensors { get; init; }
}