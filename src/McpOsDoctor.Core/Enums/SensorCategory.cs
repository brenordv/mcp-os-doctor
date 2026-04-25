namespace McpOsDoctor.Core.Enums;

/// <summary>
/// Classification of hardware sensor reading types.
/// </summary>
public enum SensorCategory
{
    /// <summary>Temperature in degrees Celsius.</summary>
    Temperature = 1,

    /// <summary>Fan speed in RPM.</summary>
    Fan = 2,

    /// <summary>Voltage in volts.</summary>
    Voltage = 3,

    /// <summary>Clock speed in MHz.</summary>
    Clock = 4,

    /// <summary>Load/utilization percentage (0–100).</summary>
    Load = 5,

    /// <summary>Power consumption in watts.</summary>
    Power = 6,

    /// <summary>Data volume in gigabytes.</summary>
    Data = 7,

    /// <summary>Throughput in bytes per second.</summary>
    Throughput = 8,

    /// <summary>Electrical current in amperes.</summary>
    Current = 9,

    /// <summary>Level percentage (e.g., battery charge).</summary>
    Level = 10,

    /// <summary>Control percentage (e.g., fan control target).</summary>
    Control = 11,

    /// <summary>Small data volume in megabytes.</summary>
    SmallData = 12,

    /// <summary>Frequency in Hz.</summary>
    Frequency = 13,

    /// <summary>Energy consumption in watt-hours.</summary>
    Energy = 14,

    /// <summary>Noise level in dBA.</summary>
    Noise = 15,

    /// <summary>Dimensionless ratio or multiplier.</summary>
    Factor = 16,

    /// <summary>Flow rate in liters per hour.</summary>
    Flow = 17,

    /// <summary>Duration in seconds.</summary>
    TimeSpan = 18
}