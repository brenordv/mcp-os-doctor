using McpOsDoctor.Core.Filters;
using McpOsDoctor.Core.Models;

namespace McpOsDoctor.Core.Interfaces;

/// <summary>
/// Provides read-only access to the operating system's process table.
/// </summary>
public interface IProcessInspector
{
    /// <summary>
    /// Gets the top processes sorted and filtered by the specified criteria.
    /// </summary>
    /// <param name="filter">Criteria for filtering and sorting processes.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An async enumerable of matching processes.</returns>
    IAsyncEnumerable<ProcessInfo> GetTopProcessesAsync(ProcessFilter filter, CancellationToken cancellationToken);
}