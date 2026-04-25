namespace McpOsDoctor.Core.Enums;

/// <summary>
/// Classification of hardware component types for sensor grouping.
/// </summary>
public enum HardwareCategory
{
    /// <summary>Central processing unit.</summary>
    Cpu = 1,

    /// <summary>Graphics processing unit (NVIDIA, AMD, or Intel).</summary>
    Gpu = 2,

    /// <summary>Motherboard and Super I/O chip sensors.</summary>
    Motherboard = 3,

    /// <summary>System memory (RAM).</summary>
    Memory = 4,

    /// <summary>Storage devices (HDD, SSD, NVMe).</summary>
    Storage = 5,

    /// <summary>Network adapters.</summary>
    Network = 6,

    /// <summary>Cooling devices (fans, liquid cooling).</summary>
    Cooler = 7,

    /// <summary>Battery (laptops and UPS).</summary>
    Battery = 8,

    /// <summary>Power supply unit.</summary>
    Psu = 9,

    /// <summary>Embedded controller sensors.</summary>
    EmbeddedController = 10
}