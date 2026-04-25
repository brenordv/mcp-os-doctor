namespace McpOsDoctor.Core.Models;

/// <summary>
/// Describes a single tool's availability, requirements, and parameter hints.
/// </summary>
public record ToolCapabilityInfo
{
    /// <summary>
    /// The tool name as used in MCP tool calls.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Whether the tool is available on this platform.
    /// </summary>
    public required bool Available { get; init; }

    /// <summary>
    /// Whether the tool requires elevated privileges to execute.
    /// </summary>
    public required bool RequiresElevation { get; init; }

    /// <summary>
    /// Note explaining what elevation provides for this tool.
    /// </summary>
    public string ElevationNote { get; init; }

    /// <summary>
    /// Typical latency in milliseconds for this tool.
    /// </summary>
    public required int TypicalLatencyMs { get; init; }

    /// <summary>
    /// Hint strings describing each parameter's purpose and defaults.
    /// </summary>
    public IReadOnlyList<string> ParameterHints { get; init; } = [];

    /// <summary>
    /// Explanation of why the tool is unavailable, when <see cref="Available"/> is false.
    /// </summary>
    public string UnavailableReason { get; init; }
}