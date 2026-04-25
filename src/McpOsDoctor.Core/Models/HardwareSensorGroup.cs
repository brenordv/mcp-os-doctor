using McpOsDoctor.Core.Enums;

namespace McpOsDoctor.Core.Models;

/// <summary>
/// Groups all sensor readings from a single hardware component.
/// </summary>
public record HardwareSensorGroup
{
    /// <summary>
    /// Hardware component name (e.g., "AMD Ryzen 9 7950X", "NVIDIA GeForce RTX 4080").
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The type of hardware component.
    /// </summary>
    public required HardwareCategory Category { get; init; }

    /// <summary>
    /// All sensor readings from this hardware component.
    /// </summary>
    public required IReadOnlyList<SensorReading> Sensors { get; init; }
}