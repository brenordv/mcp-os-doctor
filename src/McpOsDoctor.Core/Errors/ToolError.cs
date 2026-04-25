using McpOsDoctor.Core.Enums;

namespace McpOsDoctor.Core.Errors;

/// <summary>
/// Structured error returned by a tool handler when the request cannot be fulfilled.
/// </summary>
public record ToolError
{
    /// <summary>
    /// Machine-readable error code for programmatic handling.
    /// </summary>
    public required ToolErrorCode Code { get; init; }

    /// <summary>
    /// Human-readable description of what went wrong.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Actionable suggestion for the AI client to resolve the issue.
    /// </summary>
    public string Hint { get; init; }

    /// <summary>
    /// Whether this error is transient and the request might succeed if retried.
    /// </summary>
    public required bool IsRetryable { get; init; }
}