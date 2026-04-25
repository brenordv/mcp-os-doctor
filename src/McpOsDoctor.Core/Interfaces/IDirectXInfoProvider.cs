using McpOsDoctor.Core.Models;

namespace McpOsDoctor.Core.Interfaces;

/// <summary>
/// Provides DirectX diagnostic information from the system.
/// </summary>
public interface IDirectXInfoProvider
{
    /// <summary>
    /// Whether the DirectX information provider is available on this system.
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Explanation of why the provider is unavailable, when <see cref="IsAvailable"/> is false.
    /// </summary>
    string UnavailableReason { get; }

    /// <summary>
    /// Collects DirectX diagnostic information including display and sound devices.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A snapshot of DirectX diagnostic information.</returns>
    /// <exception cref="Errors.McpOsDoctorException">Thrown when the DirectX query fails.</exception>
    Task<DirectXInfo> GetDirectXInfoAsync(CancellationToken cancellationToken);
}