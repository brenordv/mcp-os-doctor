using McpOsDoctor.Core.Enums;

namespace McpOsDoctor.Core.Errors;

/// <summary>
/// Base exception for all MCP OS Doctor domain errors.
/// Carries a structured <see cref="ToolError"/> for serialization to the AI client.
/// </summary>
/// <remarks>
/// Initializes a new instance of <see cref="McpOsDoctorException"/> with a structured tool error.
/// </remarks>
/// <param name="toolError">The structured error to return to the client.</param>
/// <param name="innerException">Optional inner exception for diagnostic logging.</param>
public class McpOsDoctorException(ToolError toolError, Exception innerException = null)
    : Exception(toolError.Message, innerException)
{
    /// <summary>
    /// The structured error associated with this exception.
    /// </summary>
    public ToolError ToolError { get; } = toolError;

    /// <summary>
    /// Creates an <see cref="McpOsDoctorException"/> for an invalid parameter error.
    /// </summary>
    /// <param name="message">Description of the validation failure.</param>
    /// <param name="hint">Actionable hint for the AI client.</param>
    /// <returns>A new exception instance.</returns>
    public static McpOsDoctorException InvalidParameter(string message, string hint = null)
    {
        return new McpOsDoctorException(new ToolError
        {
            Code = ToolErrorCode.InvalidParameter,
            Message = message,
            Hint = hint,
            IsRetryable = false
        });
    }

    /// <summary>
    /// Creates an <see cref="McpOsDoctorException"/> for a source not found error.
    /// </summary>
    /// <param name="sourceName">Name of the source that was not found.</param>
    /// <returns>A new exception instance.</returns>
    public static McpOsDoctorException SourceNotFound(string sourceName)
    {
        return new McpOsDoctorException(new ToolError
        {
            Code = ToolErrorCode.SourceNotFound,
            Message = $"Event log source '{sourceName}' does not exist",
            Hint = "Use list_log_sources to see available sources",
            IsRetryable = false
        });
    }

    /// <summary>
    /// Creates an <see cref="McpOsDoctorException"/> for an elevation-required error.
    /// </summary>
    /// <param name="capability">The capability that requires elevation.</param>
    /// <returns>A new exception instance.</returns>
    public static McpOsDoctorException ElevationRequired(string capability)
    {
        return new McpOsDoctorException(new ToolError
        {
            Code = ToolErrorCode.ElevationRequired,
            Message = $"The '{capability}' capability requires administrator privileges",
            Hint = "Re-launch the MCP server as Administrator",
            IsRetryable = false
        });
    }

    /// <summary>
    /// Creates an <see cref="McpOsDoctorException"/> for a not-supported error.
    /// </summary>
    /// <param name="message">Description of the unsupported capability.</param>
    /// <param name="hint">Actionable hint for the AI client.</param>
    /// <returns>A new exception instance.</returns>
    public static McpOsDoctorException NotSupported(string message, string hint = null)
    {
        return new McpOsDoctorException(new ToolError
        {
            Code = ToolErrorCode.NotSupported,
            Message = message,
            Hint = hint,
            IsRetryable = false
        });
    }

    /// <summary>
    /// Creates an <see cref="McpOsDoctorException"/> for a platform error.
    /// </summary>
    /// <param name="message">Sanitized error message.</param>
    /// <param name="innerException">The original platform exception.</param>
    /// <returns>A new exception instance.</returns>
    public static McpOsDoctorException PlatformError(string message, Exception innerException = null)
    {
        return new McpOsDoctorException(new ToolError
        {
            Code = ToolErrorCode.PlatformError,
            Message = message,
            IsRetryable = false
        }, innerException);
    }
}