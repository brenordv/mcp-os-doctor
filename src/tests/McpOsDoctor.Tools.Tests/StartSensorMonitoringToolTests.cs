using McpOsDoctor.Tools.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace McpOsDoctor.Tools.Tests;

/// <summary>
/// Unit tests for <see cref="StartSensorMonitoringTool"/>.
/// </summary>
public class StartSensorMonitoringToolTests
{
    [Fact]
    public void Execute_DefaultInterval_StartsMonitoringWithFiveSeconds()
    {
        // Arrange
        var monitor = new FakeSensorMonitor();

        // Act
        var json = StartSensorMonitoringTool.Execute(
            NullLogger<StartSensorMonitoringTool>.Instance,
            monitor,
            new FakePrivilegeChecker());

        // Assert
        var doc = JObject.Parse(json);
        var data = doc["data"];
        Assert.Equal("running", (string)data["state"]);
        Assert.Equal(5, (int)data["intervalSeconds"]);
        Assert.Equal(1, (int)data["snapshotCount"]);
        Assert.NotNull(data["startedAt"]);
    }

    [Fact]
    public void Execute_CustomInterval_StartsMonitoringWithSpecifiedSeconds()
    {
        // Arrange
        var monitor = new FakeSensorMonitor();

        // Act
        var json = StartSensorMonitoringTool.Execute(
            NullLogger<StartSensorMonitoringTool>.Instance,
            monitor,
            new FakePrivilegeChecker(),
            intervalSeconds: 30);

        // Assert
        var doc = JObject.Parse(json);
        var data = doc["data"];
        Assert.Equal("running", (string)data["state"]);
        Assert.Equal(30, (int)data["intervalSeconds"]);
    }

    [Fact]
    public void Execute_AlreadyRunning_ReturnsInvalidParameterError()
    {
        // Arrange
        var monitor = new FakeSensorMonitor { SimulateAlreadyRunning = true };

        // Act
        var json = StartSensorMonitoringTool.Execute(
            NullLogger<StartSensorMonitoringTool>.Instance,
            monitor,
            new FakePrivilegeChecker());

        // Assert
        var doc = JObject.Parse(json);
        Assert.Equal("invalidParameter", (string)doc["code"]);
        Assert.False((bool)doc["isRetryable"]);
    }

    [Fact]
    public void Execute_ResponseIncludesMetadata()
    {
        // Arrange
        var monitor = new FakeSensorMonitor();
        var privilegeChecker = new FakePrivilegeChecker { IsElevated = true };

        // Act
        var json = StartSensorMonitoringTool.Execute(
            NullLogger<StartSensorMonitoringTool>.Instance,
            monitor,
            privilegeChecker);

        // Assert
        var doc = JObject.Parse(json);
        Assert.Equal("windows", (string)doc["platform"]);
        Assert.True((bool)doc["isElevated"]);
        Assert.True((long)doc["elapsedMs"] >= 0);
    }
}