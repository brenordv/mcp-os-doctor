using McpOsDoctor.Core.Models;

namespace McpOsDoctor.Core.Interfaces;

/// <summary>
/// Provides GPU hardware information from the system.
/// </summary>
public interface IGpuInfoProvider
{
    /// <summary>
    /// Whether the GPU information provider is available on this system.
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Explanation of why the provider is unavailable, when <see cref="IsAvailable"/> is false.
    /// </summary>
    string UnavailableReason { get; }

    /// <summary>
    /// Collects information about all detected NVIDIA GPUs.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of GPU device snapshots.</returns>
    /// <exception cref="Errors.McpOsDoctorException">Thrown when the GPU query fails.</exception>
    Task<IReadOnlyList<GpuInfo>> GetGpuInfoAsync(CancellationToken cancellationToken);
}