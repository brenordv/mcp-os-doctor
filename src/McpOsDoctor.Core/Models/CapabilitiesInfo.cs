namespace McpOsDoctor.Core.Models;

/// <summary>
/// Describes the server's available capabilities returned by the get_capabilities tool.
/// </summary>
public record CapabilitiesInfo
{
    /// <summary>
    /// The platform the server is running on (e.g., "Windows").
    /// </summary>
    public required string Platform { get; init; }

    /// <summary>
    /// The .NET runtime version string.
    /// </summary>
    public required string RuntimeVersion { get; init; }

    /// <summary>
    /// The MCP OS Doctor server version.
    /// </summary>
    public required string ServerVersion { get; init; }

    /// <summary>
    /// Whether the server process is running with elevated (administrator) privileges.
    /// </summary>
    public required bool IsElevated { get; init; }

    /// <summary>
    /// List of available diagnostic tools and their metadata.
    /// </summary>
    public required IReadOnlyList<ToolCapabilityInfo> Tools { get; init; }
}