using System.Runtime.CompilerServices;
using McpOsDoctor.Core.Filters;
using McpOsDoctor.Core.Interfaces;
using McpOsDoctor.Core.Models;

namespace McpOsDoctor.Tools.Tests.Fakes;

/// <summary>
/// Fake implementation of <see cref="ISystemLogReader"/> for unit testing.
/// </summary>
public sealed class FakeSystemLogReader : ISystemLogReader
{
    /// <summary>
    /// Log entries returned by <see cref="QueryAsync"/>.
    /// </summary>
    public IList<LogEntry> Entries { get; init; } = [];

    /// <summary>
    /// Log sources returned by <see cref="ListSourcesAsync"/>.
    /// </summary>
    public IList<LogSourceInfo> Sources { get; init; } = [];

    /// <summary>
    /// The filter that was passed to the last <see cref="QueryAsync"/> call.
    /// </summary>
    public LogQueryFilter LastFilter { get; private set; }

    /// <inheritdoc />
    public async IAsyncEnumerable<LogEntry> QueryAsync(
        LogQueryFilter filter,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        LastFilter = filter;

        foreach (var entry in Entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return entry;
        }

        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<LogSourceInfo>> ListSourcesAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyList<LogSourceInfo>>(Sources.AsReadOnly());
    }
}