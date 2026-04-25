using System.Runtime.CompilerServices;
using McpOsDoctor.Core.Filters;
using McpOsDoctor.Core.Interfaces;
using McpOsDoctor.Core.Models;

namespace McpOsDoctor.Tools.Tests.Fakes;

/// <summary>
/// Fake implementation of <see cref="IPerformanceReader"/> for unit testing.
/// </summary>
public sealed class FakePerformanceReader : IPerformanceReader
{
    /// <summary>
    /// Metrics returned by <see cref="GetMetricsAsync"/>.
    /// </summary>
    public IList<PerformanceMetric> Metrics { get; init; } = [];

    /// <inheritdoc />
    public async IAsyncEnumerable<PerformanceMetric> GetMetricsAsync(
        MetricFilter filter,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var metric in Metrics)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return metric;
        }

        await Task.CompletedTask;
    }
}