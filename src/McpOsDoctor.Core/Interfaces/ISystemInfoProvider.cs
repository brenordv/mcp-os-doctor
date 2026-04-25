using McpOsDoctor.Core.Models;

namespace McpOsDoctor.Core.Interfaces;

/// <summary>
/// Provides a point-in-time snapshot of the system's hardware and OS configuration.
/// </summary>
public interface ISystemInfoProvider
{
    /// <summary>
    /// Collects system hardware and operating system information.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A snapshot of the current system state.</returns>
    Task<SystemSnapshot> GetSystemInfoAsync(CancellationToken cancellationToken);
}