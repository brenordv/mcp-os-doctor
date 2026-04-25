using McpOsDoctor.Core.Errors;
using McpOsDoctor.Core.Interfaces;
using McpOsDoctor.Core.Models;

namespace McpOsDoctor.Tools.Tests.Fakes;

/// <summary>
/// Fake implementation of <see cref="IGpuInfoProvider"/> for unit testing.
/// When <see cref="IsAvailable"/> is false, <see cref="GetGpuInfoAsync"/> throws
/// a <see cref="McpOsDoctorException"/> to match the real provider behavior.
/// </summary>
public sealed class FakeGpuInfoProvider : IGpuInfoProvider
{
    /// <inheritdoc />
    public bool IsAvailable { get; init; } = true;

    /// <inheritdoc />
    public string UnavailableReason { get; init; }

    /// <summary>
    /// The GPU device list returned by <see cref="GetGpuInfoAsync"/>.
    /// </summary>
    public IReadOnlyList<GpuInfo> Devices { get; init; } =
    [
        new GpuInfo
        {
            Name = "NVIDIA Test GPU",
            DriverVersion = "999.99",
            MemoryTotalMb = 8192,
            MemoryUsedMb = 2048,
            MemoryFreeMb = 6144,
            TemperatureCelsius = 55,
            GpuUtilizationPercent = 30,
            MemoryUtilizationPercent = 25,
            PowerDrawWatts = 100.0,
            PowerLimitWatts = 250.0,
            FanSpeedPercent = 40,
            PerformanceState = "P0"
        }
    ];

    /// <inheritdoc />
    public Task<IReadOnlyList<GpuInfo>> GetGpuInfoAsync(CancellationToken cancellationToken)
    {
        if (!IsAvailable)
        {
            throw McpOsDoctorException.NotSupported(
                "GPU information is not available because nvidia-smi was not found.",
                "Install NVIDIA GPU drivers to enable GPU diagnostics.");
        }

        return Task.FromResult(Devices);
    }
}