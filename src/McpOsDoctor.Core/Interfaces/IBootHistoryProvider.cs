using McpOsDoctor.Core.Filters;
using McpOsDoctor.Core.Models;

namespace McpOsDoctor.Core.Interfaces;

/// <summary>
/// Provides read-only access to system boot, shutdown, and power state history.
/// </summary>
public interface IBootHistoryProvider
{
    /// <summary>
    /// Gets boot and shutdown events within the specified time window.
    /// </summary>
    /// <param name="filter">Time window and result count constraints.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An async enumerable of boot events.</returns>
    IAsyncEnumerable<BootEvent> GetBootEventsAsync(TimeWindowFilter filter, CancellationToken cancellationToken);
}