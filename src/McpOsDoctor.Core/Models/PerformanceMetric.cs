namespace McpOsDoctor.Core.Models;

/// <summary>
/// A single performance counter reading from the operating system.
/// </summary>
public record PerformanceMetric
{
    /// <summary>
    /// Performance counter category (e.g., "Processor", "Memory").
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Name of the specific counter within the category.
    /// </summary>
    public required string Counter { get; init; }

    /// <summary>
    /// Instance name for multi-instance counters, if applicable.
    /// </summary>
    public string Instance { get; init; }

    /// <summary>
    /// Current counter-value.
    /// </summary>
    public required double Value { get; init; }

    /// <summary>
    /// Unit of measurement (e.g., "%", "MB", "MB/s").
    /// </summary>
    public required string Unit { get; init; }

    /// <summary>
    /// When the reading was taken.
    /// </summary>
    public required DateTimeOffset Timestamp { get; init; }
}