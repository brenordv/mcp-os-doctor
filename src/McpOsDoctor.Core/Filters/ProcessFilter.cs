using McpOsDoctor.Core.Enums;

namespace McpOsDoctor.Core.Filters;

/// <summary>
/// Filter for listing top processes by resource consumption.
/// </summary>
public class ProcessFilter(int? maxResults, ProcessSortField? configSortBy)
{
    /// <summary>
    /// Default maximum number of results.
    /// </summary>
    private const int DefaultMaxResults = 25;

    /// <summary>
    /// Absolute maximum number of results.
    /// </summary>
    private const int HardMaxResults = 100;

    /// <summary>
    /// Field used to sort the process list. Defaults to <see cref="ProcessSortField.Cpu"/>.
    /// </summary>
    public ProcessSortField SortBy => configSortBy ?? ProcessSortField.Cpu;

    /// <summary>
    /// Exclude processes using less than this percentage of CPU.
    /// </summary>
    public double? MinCpuPercent { get; init; }

    /// <summary>
    /// Exclude processes using less than this amount of memory in megabytes.
    /// </summary>
    public double? MinMemoryMb { get; init; }

    /// <summary>
    /// Gets the maximum number of results to return. Clamped to <see cref="HardMaxResults"/>.
    /// </summary>
    public int MaxResults => Math.Min(maxResults ?? DefaultMaxResults, HardMaxResults);
}