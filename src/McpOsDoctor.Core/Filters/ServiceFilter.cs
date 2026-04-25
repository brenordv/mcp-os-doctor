using McpOsDoctor.Core.DataTypes;
using McpOsDoctor.Core.Enums;

namespace McpOsDoctor.Core.Filters;

/// <summary>
/// Filter for querying system services by name pattern and running state.
/// </summary>
public class ServiceFilter(int? configMaxResults)
{
    /// <summary>
    /// Default maximum number of results.
    /// </summary>
    private const int DefaultMaxResults = 100;

    /// <summary>
    /// Absolute maximum number of results.
    /// </summary>
    private const int HardMaxResults = 500;

    /// <summary>
    /// Prefix the match against the service name.
    /// </summary>
    public SourceName NamePattern { get; init; }

    /// <summary>
    /// Filter to only services in this running state.
    /// </summary>
    public ServiceRunState? Status { get; init; }

    /// <summary>
    /// Maximum number of results to return. Clamped to <see cref="HardMaxResults"/>.
    /// </summary>
    public int MaxResults => Math.Min(configMaxResults ?? DefaultMaxResults, HardMaxResults);
}