using McpOsDoctor.Core.DataTypes;
using McpOsDoctor.Core.Filters;
using McpOsDoctor.Core.Models;

namespace McpOsDoctor.Core.Interfaces;

/// <summary>
/// Provides read-only access to the operating system's service management subsystem.
/// </summary>
public interface IServiceInspector
{
    /// <summary>
    /// Lists services matching the specified filter criteria.
    /// </summary>
    /// <param name="filter">Criteria for filtering services.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An async enumerable of matching services.</returns>
    IAsyncEnumerable<ServiceInfo> GetServicesAsync(ServiceFilter filter, CancellationToken cancellationToken);

    /// <summary>
    /// Gets detailed information about a single service by name.
    /// </summary>
    /// <param name="name">The internal service name.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The service information, or null if not found.</returns>
    Task<ServiceInfo> GetServiceAsync(SourceName name, CancellationToken cancellationToken);
}