using McpOsDoctor.Core.Enums;
using McpOsDoctor.Core.Errors;

namespace McpOsDoctor.Core.Mappers;

/// <summary>
/// Provides methods for converting string representations to corresponding
/// enumeration values within the system, specifically for mapping service run states,
/// process sort fields, and log severity levels.
/// </summary>
public static partial class Mapper
{
    /// <summary>
    /// Attempts to parse the provided string value into a <see cref="ServiceRunState"/> enumeration value.
    /// If the input string is null, empty, or consists only of whitespace, returns null.
    /// Throws an exception if the input string cannot be successfully parsed into a valid <see cref="ServiceRunState"/> value.
    /// </summary>
    /// <param name="value">
    /// A string representing the state of a system service. This value is expected to match one of the
    /// <see cref="ServiceRunState"/> enumeration values (e.g., "Running", "Stopped", "Paused").
    /// </param>
    /// <returns>
    /// A nullable <see cref="ServiceRunState"/> that represents the parsed value of the input string,
    /// or null if the input string is null or whitespace.
    /// </returns>
    /// <exception cref="McpOsDoctorException">
    /// Thrown when the input string cannot be parsed into a valid <see cref="ServiceRunState"/> enumeration value.
    /// The exception message includes the invalid input and a hint indicating valid values.
    /// </exception>
    public static ServiceRunState? TryToServiceRunState(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (Enum.TryParse<ServiceRunState>(value, ignoreCase: true, out var result))
        {
            return result;
        }

        throw McpOsDoctorException.InvalidParameter(
            $"Invalid status '{value}'.",
            "Use one of: Running, Stopped, Paused, StartPending, StopPending, Unknown.");
    }

    /// <summary>
    /// Attempts to parse the provided string value into a <see cref="ProcessSortField"/> enumeration value.
    /// If the input string is null, empty, or consists only of whitespace, returns null.
    /// Throws an exception if the input string cannot be successfully parsed into a valid <see cref="ProcessSortField"/> value.
    /// </summary>
    /// <param name="sortBy">
    /// A string representing the field used to sort process listing results. Valid values include:
    /// "cpu", "memory", or "name" (case-insensitive).
    /// </param>
    /// <returns>
    /// A nullable <see cref="ProcessSortField"/> that represents the parsed value of the input string,
    /// or null if the input string is null or whitespace.
    /// </returns>
    /// <exception cref="McpOsDoctorException">
    /// Thrown when the input string cannot be parsed into a valid <see cref="ProcessSortField"/> enumeration value.
    /// The exception message includes the invalid input and a hint indicating valid values.
    /// </exception>
    public static ProcessSortField? TryToProcessSortField(string sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return null;
        }

        return sortBy.ToLowerInvariant() switch
        {
            "cpu" => ProcessSortField.Cpu,
            "memory" => ProcessSortField.Memory,
            "name" => ProcessSortField.Name,
            _ => throw McpOsDoctorException.InvalidParameter(
                $"Invalid sortBy value '{sortBy}'.",
                "Use one of: cpu, memory, name.")
        };
    }

    /// <summary>
    /// Attempts to parse the provided string value into a <see cref="LogSeverity"/> enumeration value.
    /// Returns null if the input string is null or empty.
    /// Throws an exception if the input string cannot be successfully parsed into a valid <see cref="LogSeverity"/> value.
    /// </summary>
    /// <param name="severity">
    /// A string representing the severity level of a log entry. This value is expected to match one of the
    /// <see cref="LogSeverity"/> enumeration values (e.g., "Verbose", "Information", "Warning", "Error", "Critical").
    /// </param>
    /// <returns>
    /// A nullable <see cref="LogSeverity"/> that represents the parsed value of the input string,
    /// or null if the input string is null or empty.
    /// </returns>
    /// <exception cref="McpOsDoctorException">
    /// Thrown when the input string cannot be parsed into a valid <see cref="LogSeverity"/> enumeration value.
    /// The exception message includes the invalid input and a hint indicating valid values.
    /// </exception>
    public static LogSeverity? TryToLogSeverity(string severity)
    {
        if (string.IsNullOrEmpty(severity))
        {
            return null;
        }

        return Enum.TryParse<LogSeverity>(severity, ignoreCase: true, out var result)
            ? result
            : throw McpOsDoctorException.InvalidParameter(
                $"Invalid severity '{severity}'.",
                "Use one of: Verbose, Information, Warning, Error, Critical.");
    }
}