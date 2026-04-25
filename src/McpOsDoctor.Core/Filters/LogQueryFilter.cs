using McpOsDoctor.Core.DataTypes;
using McpOsDoctor.Core.Enums;

namespace McpOsDoctor.Core.Filters;

/// <summary>
/// Filter for querying system log entries, extending <see cref="TimeWindowFilter"/>
/// with log-specific criteria.
/// </summary>
public class LogQueryFilter(int? maxResults, DateTimeOffset? from, DateTimeOffset? to)
    : TimeWindowFilter(maxResults, from, to)
{
    /// <summary>
    /// Minimum severity level to include. Entries below this level are excluded.
    /// </summary>
    public LogSeverity? Severity { get; init; }

    /// <summary>
    /// Exact source name or prefix match (when ending with *).
    /// </summary>
    public SourceName Source { get; init; }

    /// <summary>
    /// Substring to search for within log messages.
    /// </summary>
    public string Keywords { get; init; }
}