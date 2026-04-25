namespace McpOsDoctor.Core.Filters;

/// <summary>
/// Filter for selecting which performance metrics to read.
/// </summary>
public class MetricFilter(int? configMaxResults)
{
    /// <summary>
    /// Default maximum number of results.
    /// </summary>
    private const int DefaultMaxResults = 20;

    /// <summary>
    /// Absolute maximum number of results.
    /// </summary>
    private const int HardMaxResults = 50;

    /// <summary>
    /// Performance counter categories to include. If null, returns a curated default set.
    /// </summary>
    public IReadOnlyList<string> Categories { get; init; }

    /// <summary>
    /// Maximum number of results to return. Clamped to <see cref="HardMaxResults"/>.
    /// </summary>
    public int MaxResults => Math.Min(configMaxResults ?? DefaultMaxResults, HardMaxResults);
}