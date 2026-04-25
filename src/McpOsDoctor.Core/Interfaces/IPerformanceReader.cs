using McpOsDoctor.Core.Filters;
using McpOsDoctor.Core.Models;

namespace McpOsDoctor.Core.Interfaces;

/// <summary>
/// Provides read-only access to operating system performance counters.
/// </summary>
public interface IPerformanceReader
{
    /// <summary>
    /// Reads performance metrics matching the specified filter.
    /// </summary>
    /// <param name="filter">Criteria for selecting performance counters.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An async enumerable of performance metric readings.</returns>
    IAsyncEnumerable<PerformanceMetric> GetMetricsAsync(MetricFilter filter, CancellationToken cancellationToken);
}