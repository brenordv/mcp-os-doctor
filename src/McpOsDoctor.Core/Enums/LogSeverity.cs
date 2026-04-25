namespace McpOsDoctor.Core.Enums;

/// <summary>
/// Severity level for system log entries, ordered from least to most severe.
/// </summary>
public enum LogSeverity
{
    /// <summary>Detailed tracing information.</summary>
    Verbose = 1,

    /// <summary>General informational messages.</summary>
    Information = 2,

    /// <summary>Conditions that are not errors but may warrant attention.</summary>
    Warning = 3,

    /// <summary>An error condition occurred.</summary>
    Error = 4,

    /// <summary>A critical failure requiring immediate attention.</summary>
    Critical = 5
}