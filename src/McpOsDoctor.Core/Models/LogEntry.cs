using McpOsDoctor.Core.Enums;

namespace McpOsDoctor.Core.Models;

/// <summary>
/// A normalized system log entry from the operating system's event logging subsystem.
/// </summary>
public record LogEntry
{
    /// <summary>
    /// When the log entry was recorded.
    /// </summary>
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// Severity level of the log entry.
    /// </summary>
    public required LogSeverity Severity { get; init; }

    /// <summary>
    /// Name of the source that generated the log entry.
    /// </summary>
    public required string Source { get; init; }

    /// <summary>
    /// Platform-specific event identifier, if available.
    /// </summary>
    public int? EventId { get; init; }

    /// <summary>
    /// Human-readable message content, truncated to 2000 characters.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Additional platform-specific properties not captured by the normalized fields.
    /// </summary>
    public IDictionary<string, object> PlatformSpecific { get; init; }
}