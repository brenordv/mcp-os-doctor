using McpOsDoctor.Core.Errors;

namespace McpOsDoctor.Core.Filters;

/// <summary>
/// Base filter that constrains queries by time window and result count.
/// </summary>
public class TimeWindowFilter
{
    /// <summary>
    /// Default maximum number of results.
    /// </summary>
    private const int DefaultMaxResults = 100;

    /// <summary>
    /// Absolute maximum number of results, regardless of user request.
    /// </summary>
    private const int HardMaxResults = 500;

    /// <summary>
    /// Maximum allowed time window in days.
    /// </summary>
    private const int MaxTimeWindowDays = 7;

    /// <summary>
    /// Start of the time window. Defaults to 24 hours ago if not specified.
    /// </summary>
    public DateTimeOffset From { get; init; }

    /// <summary>
    /// End of the time window. Defaults to now if not specified.
    /// </summary>
    public DateTimeOffset To { get; init; }

    /// <summary>
    /// Maximum number of results to return. Clamped to <see cref="HardMaxResults"/>.
    /// </summary>
    public int MaxResults { get; init; }

    /// <summary>
    /// Base filter that constrains queries by time window and result count.
    /// </summary>
    public TimeWindowFilter(int? maxResult, DateTimeOffset? from, DateTimeOffset? to)
    {
        (From, To) = GetNormalizeTimeWindow(from, to);
        MaxResults = Math.Min(maxResult ?? DefaultMaxResults, HardMaxResults);
    }

    /// <summary>
    /// Normalizes a time window, applying defaults for missing values and validating the range.
    /// </summary>
    /// <returns>A tuple of normalized from and to values.</returns>
    /// <exception cref="McpOsDoctorException">Thrown when the time window is invalid.</exception>
    private static (DateTimeOffset From, DateTimeOffset To) GetNormalizeTimeWindow(DateTimeOffset? from, DateTimeOffset? to)
    {
        var now = DateTimeOffset.UtcNow;
        var normalizedFrom = from ?? now.AddDays(-1);
        var normalizedTo = to ?? now;

        if (normalizedFrom > normalizedTo)
        {
            throw McpOsDoctorException.InvalidParameter(
                "'from' must be before 'to'.",
                "Swap the from/to values or omit them to use the default 24-hour window.");
        }

        if (normalizedTo > now.AddMinutes(5))
        {
            throw McpOsDoctorException.InvalidParameter(
                "'to' cannot be in the future.",
                "Omit 'to' to use the current time.");
        }

        var span = normalizedTo - normalizedFrom;

        return span.TotalDays > MaxTimeWindowDays
            ? throw McpOsDoctorException.InvalidParameter(
                $"Time window exceeds the maximum of {MaxTimeWindowDays} days ({span.TotalDays:F1} days requested).",
                $"Narrow the window to {MaxTimeWindowDays} days or fewer.")
            : (normalizedFrom, normalizedTo);
    }
}