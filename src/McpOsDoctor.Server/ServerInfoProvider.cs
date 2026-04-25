using McpOsDoctor.Core.Interfaces;

namespace McpOsDoctor.Server;

/// <summary>
/// Provides server metadata by reading the host assembly's version.
/// </summary>
public sealed class ServerInfoProvider : IServerInfoProvider
{
    /// <inheritdoc />
    public string Version { get; } = typeof(ServerInfoProvider).Assembly
        .GetName().Version?.ToString() ?? "0.0.0";
}