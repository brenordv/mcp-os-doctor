using McpOsDoctor.Core.Filters;
using McpOsDoctor.Core.Models;

namespace McpOsDoctor.Core.Interfaces;

/// <summary>
/// Provides read-only access to the operating system's event logging subsystem.
/// </summary>
public interface ISystemLogReader
{
    /// <summary>
    /// Queries log entries matching the specified filter criteria.
    /// </summary>
    /// <param name="filter">Criteria for filtering log entries.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An async enumerable of matching log entries.</returns>
    IAsyncEnumerable<LogEntry> QueryAsync(LogQueryFilter filter, CancellationToken cancellationToken);

    /// <summary>
    /// Lists all available log sources on the system.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of available log sources.</returns>
    Task<IReadOnlyList<LogSourceInfo>> ListSourcesAsync(CancellationToken cancellationToken);
}