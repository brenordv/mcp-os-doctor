namespace McpOsDoctor.Core.Models;

/// <summary>
/// Describes a single display adapter as reported by DxDiag.
/// </summary>
public record DirectXDisplayDevice
{
    /// <summary>
    /// Graphics card name (e.g., "NVIDIA GeForce RTX 4080 SUPER").
    /// </summary>
    public required string CardName { get; init; }

    /// <summary>
    /// GPU manufacturer (e.g., "NVIDIA").
    /// </summary>
    public required string Manufacturer { get; init; }

    /// <summary>
    /// GPU chip type identifier.
    /// </summary>
    public required string ChipType { get; init; }

    /// <summary>
    /// Device type (e.g., "Full Device (POST)").
    /// </summary>
    public string DeviceType { get; init; }

    /// <summary>
    /// Total display memory in megabytes (dedicated + shared).
    /// </summary>
    public required int DisplayMemoryMb { get; init; }

    /// <summary>
    /// Dedicated video memory in megabytes.
    /// </summary>
    public required int DedicatedMemoryMb { get; init; }

    /// <summary>
    /// Shared system memory in megabytes.
    /// </summary>
    public required int SharedMemoryMb { get; init; }

    /// <summary>
    /// Current display mode (e.g., "2560 x 1440 (32 bit) (144Hz)").
    /// </summary>
    public string CurrentMode { get; init; }

    /// <summary>
    /// HDR support status (e.g., "Supported", "Not Supported").
    /// </summary>
    public string HdrSupport { get; init; }

    /// <summary>
    /// Monitor name.
    /// </summary>
    public string MonitorName { get; init; }

    /// <summary>
    /// Monitor model identifier.
    /// </summary>
    public string MonitorModel { get; init; }

    /// <summary>
    /// Display output type (e.g., "HDMI", "DVI", "DisplayPort").
    /// </summary>
    public string OutputType { get; init; }

    /// <summary>
    /// Display driver version string.
    /// </summary>
    public required string DriverVersion { get; init; }

    /// <summary>
    /// Device Driver Interface version (e.g., "12").
    /// </summary>
    public string DdiVersion { get; init; }

    /// <summary>
    /// Supported Direct3D feature levels (e.g., "12_1,12_0,11_1,11_0").
    /// </summary>
    public string FeatureLevels { get; init; }

    /// <summary>
    /// Windows Display Driver Model version (e.g., "WDDM 2.7").
    /// </summary>
    public string DriverModel { get; init; }

    /// <summary>
    /// Hardware-accelerated GPU scheduling status (e.g., "Supported:True Enabled:True").
    /// </summary>
    public string HardwareScheduling { get; init; }
}