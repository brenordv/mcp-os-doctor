namespace McpOsDoctor.Server.Logging;

/// <summary>
/// Represents a single structured log entry written to the log file.
/// </summary>
public record LogFileEntry
{
    /// <summary>
    /// UTC timestamp of the log entry.
    /// </summary>
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// Log level (e.g., "Information", "Error").
    /// </summary>
    public required string Level { get; init; }

    /// <summary>
    /// Logger category name.
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Formatted log message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Full exception text, if an exception was logged.
    /// </summary>
    public string Exception { get; init; }
}