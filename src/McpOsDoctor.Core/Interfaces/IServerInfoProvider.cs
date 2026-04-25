namespace McpOsDoctor.Core.Interfaces;

/// <summary>
/// Provides metadata about the MCP OS Doctor server instance.
/// </summary>
public interface IServerInfoProvider
{
    /// <summary>
    /// The server's version string.
    /// </summary>
    string Version { get; }
}