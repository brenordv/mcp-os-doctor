using McpOsDoctor.Core.Interfaces;

namespace McpOsDoctor.Tools.Tests.Fakes;

/// <summary>
/// Fake implementation of <see cref="IServerInfoProvider"/> for unit testing.
/// </summary>
public sealed class FakeServerInfoProvider : IServerInfoProvider
{
    /// <inheritdoc />
    public string Version { get; init; } = "1.0.0-test";
}