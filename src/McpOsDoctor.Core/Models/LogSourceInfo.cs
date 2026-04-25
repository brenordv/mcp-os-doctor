namespace McpOsDoctor.Core.Models;

/// <summary>
/// Describes an available system log source that can be queried.
/// </summary>
public record LogSourceInfo
{
    /// <summary>
    /// Name of the log source (e.g., "Application", "System", "Security").
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Whether this source requires elevated privileges to read.
    /// </summary>
    public required bool RequiresElevation { get; init; }

    /// <summary>
    /// Approximate number of records in this source, if known.
    /// </summary>
    public long? RecordCount { get; init; }
}