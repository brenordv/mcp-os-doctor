namespace McpOsDoctor.Core.Errors;

/// <summary>
/// Standard response envelope wrapping all tool output with metadata.
/// </summary>
/// <typeparam name="TData">Type of the data payload.</typeparam>
public record ToolResponse<TData>
{
    /// <summary>
    /// The primary data payload.
    /// </summary>
    public required TData Data { get; init; }

    /// <summary>
    /// Non-fatal warnings about the response (e.g., truncation, redaction applied).
    /// </summary>
    public required IReadOnlyList<string> Warnings { get; init; }

    /// <summary>
    /// Total number of matching items when the result was truncated, if known.
    /// </summary>
    public int? TotalAvailable { get; init; }

    /// <summary>
    /// Time taken to process this tool call in milliseconds.
    /// </summary>
    public required long ElapsedMs { get; init; }

    /// <summary>
    /// Platform identifier (e.g., "Windows", "Linux", "macOS").
    /// </summary>
    public required string Platform { get; init; }

    /// <summary>
    /// Whether the server process is running with elevated privileges.
    /// </summary>
    public required bool IsElevated { get; init; }
}