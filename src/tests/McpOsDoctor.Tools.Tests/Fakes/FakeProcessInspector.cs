using System.Runtime.CompilerServices;
using McpOsDoctor.Core.Filters;
using McpOsDoctor.Core.Interfaces;
using McpOsDoctor.Core.Models;

namespace McpOsDoctor.Tools.Tests.Fakes;

/// <summary>
/// Fake implementation of <see cref="IProcessInspector"/> for unit testing.
/// </summary>
public sealed class FakeProcessInspector : IProcessInspector
{
    /// <summary>
    /// Processes returned by <see cref="GetTopProcessesAsync"/>.
    /// </summary>
    public IList<ProcessInfo> Processes { get; init; } = [];

    /// <inheritdoc />
    public async IAsyncEnumerable<ProcessInfo> GetTopProcessesAsync(
        ProcessFilter filter,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var process in Processes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return process;
        }

        await Task.CompletedTask;
    }
}