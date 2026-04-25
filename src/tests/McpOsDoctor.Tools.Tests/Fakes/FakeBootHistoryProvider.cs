using System.Runtime.CompilerServices;
using McpOsDoctor.Core.Filters;
using McpOsDoctor.Core.Interfaces;
using McpOsDoctor.Core.Models;

namespace McpOsDoctor.Tools.Tests.Fakes;

/// <summary>
/// Fake implementation of <see cref="IBootHistoryProvider"/> for unit testing.
/// </summary>
public sealed class FakeBootHistoryProvider : IBootHistoryProvider
{
    /// <summary>
    /// Boot events returned by <see cref="GetBootEventsAsync"/>.
    /// </summary>
    public IList<BootEvent> Events { get; init; } = [];

    /// <inheritdoc />
    public async IAsyncEnumerable<BootEvent> GetBootEventsAsync(
        TimeWindowFilter filter,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var evt in Events)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return evt;
        }

        await Task.CompletedTask;
    }
}