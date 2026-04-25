using McpOsDoctor.Core.Interfaces;
using McpOsDoctor.Core.Models;

namespace McpOsDoctor.Tools.Tests.Fakes;

/// <summary>
/// Fake implementation of <see cref="ISystemInfoProvider"/> for unit testing.
/// </summary>
public sealed class FakeSystemInfoProvider : ISystemInfoProvider
{
    /// <summary>
    /// The snapshot returned by <see cref="GetSystemInfoAsync"/>.
    /// </summary>
    public SystemSnapshot Snapshot { get; init; } = new()
    {
        Hostname = "TEST-HOST",
        OsVersion = "Windows 10 Test",
        OsArchitecture = "X64",
        ProcessorName = "Test CPU",
        ProcessorCores = 4,
        TotalMemoryGb = 16.0,
        AvailableMemoryGb = 8.0,
        UptimeHours = 24.0,
        IsElevated = false,
        DiskVolumes = []
    };

    /// <inheritdoc />
    public Task<SystemSnapshot> GetSystemInfoAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(Snapshot);
    }
}