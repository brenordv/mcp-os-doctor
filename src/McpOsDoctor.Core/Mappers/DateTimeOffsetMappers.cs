using McpOsDoctor.Core.Errors;

namespace McpOsDoctor.Core.Mappers;

public static partial class Mapper
{
    /// <summary>
    /// Attempts to convert the specified string to a <see cref="DateTimeOffset"/>.
    /// </summary>
    /// <param name="value">The string value that represents a date-time in ISO 8601 format.</param>
    /// <param name="parameterName">The name of the parameter being validated, used in the exception message if the value is invalid.</param>
    /// <returns>
    /// A <see cref="DateTimeOffset"/> instance if the conversion succeeds; otherwise, <c>null</c>.
    /// </returns>
    /// <exception cref="McpOsDoctorException">
    /// Thrown when the provided string cannot be parsed as a valid ISO 8601 date-time.
    /// </exception>
    public static DateTimeOffset? TryToDateTimeOffset(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateTimeOffset.TryParse(value, out var result)
            ? result
            : throw McpOsDoctorException.InvalidParameter(
                $"'{parameterName}' is not a valid ISO 8601 date-time string: '{value}'.",
                "Use ISO 8601 format, e.g., '2025-01-15T10:30:00Z'.");
    }
}