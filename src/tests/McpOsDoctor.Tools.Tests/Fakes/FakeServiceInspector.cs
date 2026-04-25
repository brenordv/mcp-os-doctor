using System.Runtime.CompilerServices;
using McpOsDoctor.Core.DataTypes;
using McpOsDoctor.Core.Filters;
using McpOsDoctor.Core.Interfaces;
using McpOsDoctor.Core.Models;

namespace McpOsDoctor.Tools.Tests.Fakes;

/// <summary>
/// Fake implementation of <see cref="IServiceInspector"/> for unit testing.
/// </summary>
public sealed class FakeServiceInspector : IServiceInspector
{
    /// <summary>
    /// Services returned by <see cref="GetServicesAsync"/>.
    /// </summary>
    public IList<ServiceInfo> Services { get; init; } = [];

    /// <summary>
    /// Single service returned by <see cref="GetServiceAsync"/>. Null simulates not found.
    /// </summary>
    public ServiceInfo SingleService { get; init; }

    /// <inheritdoc />
    public async IAsyncEnumerable<ServiceInfo> GetServicesAsync(
        ServiceFilter filter,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var service in Services)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return service;
        }

        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<ServiceInfo> GetServiceAsync(SourceName name, CancellationToken cancellationToken)
    {
        return Task.FromResult(SingleService);
    }
}