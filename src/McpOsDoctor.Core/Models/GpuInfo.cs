namespace McpOsDoctor.Core.Models;

/// <summary>
/// Point-in-time snapshot of a single GPU device.
/// </summary>
public record GpuInfo
{
    /// <summary>
    /// GPU model name (e.g., "NVIDIA GeForce RTX 4080 SUPER").
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Installed driver version string.
    /// </summary>
    public required string DriverVersion { get; init; }

    /// <summary>
    /// Total GPU memory in megabytes.
    /// </summary>
    public required double MemoryTotalMb { get; init; }

    /// <summary>
    /// Currently used GPU memory in megabytes.
    /// </summary>
    public required double MemoryUsedMb { get; init; }

    /// <summary>
    /// Free GPU memory in megabytes.
    /// </summary>
    public required double MemoryFreeMb { get; init; }

    /// <summary>
    /// GPU core temperature in degrees Celsius.
    /// </summary>
    public required int TemperatureCelsius { get; init; }

    /// <summary>
    /// GPU core utilization percentage (0–100).
    /// </summary>
    public required int GpuUtilizationPercent { get; init; }

    /// <summary>
    /// GPU memory controller utilization percentage (0–100).
    /// </summary>
    public required int MemoryUtilizationPercent { get; init; }

    /// <summary>
    /// Current power draw in watts, if supported by the device.
    /// </summary>
    public double? PowerDrawWatts { get; init; }

    /// <summary>
    /// Power limit in watts, if supported by the device.
    /// </summary>
    public double? PowerLimitWatts { get; init; }

    /// <summary>
    /// Fan speed percentage (0–100), if supported by the device.
    /// </summary>
    public int? FanSpeedPercent { get; init; }

    /// <summary>
    /// GPU performance state (e.g., "P0" through "P12").
    /// </summary>
    public string PerformanceState { get; init; }
}