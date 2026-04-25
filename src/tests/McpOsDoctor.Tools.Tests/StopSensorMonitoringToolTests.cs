using McpOsDoctor.Core.Enums;
using McpOsDoctor.Tools.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace McpOsDoctor.Tools.Tests;

/// <summary>
/// Unit tests for <see cref="StopSensorMonitoringTool"/>.
/// </summary>
public class StopSensorMonitoringToolTests
{
    [Fact]
    public void Execute_RunningMonitor_StopsAndReturnsStatus()
    {
        // Arrange
        var monitor = new FakeSensorMonitor
        {
            State = MonitoringState.Running,
            StartedAt = DateTimeOffset.UtcNow.AddMinutes(-5),
            IntervalSeconds = 10,
            SnapshotCount = 30
        };

        // Act
        var json = StopSensorMonitoringTool.Execute(
            NullLogger<StopSensorMonitoringTool>.Instance,
            monitor,
            new FakePrivilegeChecker());

        // Assert
        var doc = JObject.Parse(json);
        var data = doc["data"];
        Assert.Equal("stopped", (string)data["state"]);
        Assert.Equal(10, (int)data["intervalSeconds"]);
        Assert.Equal(30, (int)data["snapshotCount"]);
    }

    [Fact]
    public void Execute_AlreadyStopped_ReturnsStopped()
    {
        // Arrange
        var monitor = new FakeSensorMonitor
        {
            State = MonitoringState.Stopped,
            IntervalSeconds = 5,
            SnapshotCount = 0
        };

        // Act
        var json = StopSensorMonitoringTool.Execute(
            NullLogger<StopSensorMonitoringTool>.Instance,
            monitor,
            new FakePrivilegeChecker());

        // Assert
        var doc = JObject.Parse(json);
        var data = doc["data"];
        Assert.Equal("stopped", (string)data["state"]);
    }

    [Fact]
    public void Execute_ResponseIncludesMetadata()
    {
        // Arrange
        var monitor = new FakeSensorMonitor();

        // Act
        var json = StopSensorMonitoringTool.Execute(
            NullLogger<StopSensorMonitoringTool>.Instance,
            monitor,
            new FakePrivilegeChecker());

        // Assert
        var doc = JObject.Parse(json);
        Assert.Equal("windows", (string)doc["platform"]);
        Assert.True((long)doc["elapsedMs"] >= 0);
    }
}