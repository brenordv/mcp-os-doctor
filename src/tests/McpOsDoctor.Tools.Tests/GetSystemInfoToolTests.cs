using McpOsDoctor.Core.Models;
using McpOsDoctor.Tools.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace McpOsDoctor.Tools.Tests;

/// <summary>
/// Unit tests for <see cref="GetSystemInfoTool"/>.
/// </summary>
public class GetSystemInfoToolTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsSystemSnapshot()
    {
        // Arrange
        var infoProvider = new FakeSystemInfoProvider
        {
            Snapshot = new SystemSnapshot
            {
                Hostname = "WORKSTATION-01",
                OsVersion = "Microsoft Windows 10 Enterprise",
                OsArchitecture = "X64",
                ProcessorName = "Intel Core i7-12700K",
                ProcessorCores = 12,
                TotalMemoryGb = 32.0,
                AvailableMemoryGb = 16.5,
                UptimeHours = 72.3,
                IsElevated = false,
                DiskVolumes =
                [
                    new DiskVolumeInfo
                    {
                        Name = @"C:\",
                        Label = "OS",
                        FileSystem = "NTFS",
                        TotalGb = 500.0,
                        FreeGb = 150.0
                    }
                ]
            }
        };

        // Act
        var json = await GetSystemInfoTool.ExecuteAsync(
            NullLogger<GetSystemInfoTool>.Instance,
            infoProvider,
            new FakePrivilegeChecker(),
            TestContext.Current.CancellationToken);

        // Assert
        var doc = JObject.Parse(json);
        var data = doc["data"];
        Assert.Equal("WORKSTATION-01", (string)data["hostname"]);
        Assert.Equal("X64", (string)data["osArchitecture"]);
        Assert.Equal(12, (int)data["processorCores"]);
        Assert.Equal(32.0, (double)data["totalMemoryGb"]);

        var volumes = (JArray)data["diskVolumes"];
        Assert.Single(volumes);
        Assert.Equal("NTFS", (string)volumes[0]["fileSystem"]);
    }
}