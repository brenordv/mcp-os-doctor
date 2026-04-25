using McpOsDoctor.Core.Enums;

namespace McpOsDoctor.Core.Models;

/// <summary>
/// A single sensor value captured at a point in time.
/// </summary>
public record SensorReading
{
    /// <summary>
    /// Sensor name as reported by the hardware (e.g., "CPU Core #1", "GPU Hot Spot").
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The type of measurement this sensor provides.
    /// </summary>
    public required SensorCategory Category { get; init; }

    /// <summary>
    /// The measured value in the unit implied by <see cref="Category"/>.
    /// </summary>
    public required float Value { get; init; }
}