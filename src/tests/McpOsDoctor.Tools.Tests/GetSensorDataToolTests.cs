using McpOsDoctor.Core.Enums;
using McpOsDoctor.Core.Models;
using McpOsDoctor.Tools.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace McpOsDoctor.Tools.Tests;

/// <summary>
/// Unit tests for <see cref="GetSensorDataTool"/>.
/// </summary>
public class GetSensorDataToolTests
{
    [Fact]
    public void Execute_WithData_ReturnsSummary()
    {
        // Arrange
        var monitor = new FakeSensorMonitor
        {
            State = MonitoringState.Running,
            StartedAt = DateTimeOffset.UtcNow.AddMinutes(-2),
            IntervalSeconds = 5,
            SnapshotCount = 24,
            HardwareSummaries =
            [
                new HardwareSensorSummary
                {
                    Name = "AMD Ryzen 9 7950X",
                    Category = HardwareCategory.Cpu,
                    Sensors =
                    [
                        new SensorStatistics
                        {
                            Name = "Core (Tctl/Tdie)",
                            Category = SensorCategory.Temperature,
                            Current = 55.0f,
                            Min = 42.0f,
                            Max = 78.0f,
                            Average = 58.5f
                        }
                    ]
                }
            ]
        };

        // Act
        var json = GetSensorDataTool.Execute(
            NullLogger<GetSensorDataTool>.Instance,
            monitor,
            new FakePrivilegeChecker());

        // Assert
        var doc = JObject.Parse(json);
        var data = doc["data"];
        Assert.Equal("running", (string)data["state"]);
        Assert.Equal(24, (int)data["snapshotCount"]);
        Assert.Equal(5, (int)data["intervalSeconds"]);

        var hardware = (JArray)data["hardware"];
        Assert.Single(hardware);
        Assert.Equal("AMD Ryzen 9 7950X", (string)hardware[0]["name"]);
        Assert.Equal("cpu", (string)hardware[0]["category"]);

        var sensors = (JArray)hardware[0]["sensors"];
        Assert.Single(sensors);
        Assert.Equal("Core (Tctl/Tdie)", (string)sensors[0]["name"]);
        Assert.Equal("temperature", (string)sensors[0]["category"]);
        Assert.Equal(55.0f, (float)sensors[0]["current"]);
        Assert.Equal(42.0f, (float)sensors[0]["min"]);
        Assert.Equal(78.0f, (float)sensors[0]["max"]);
        Assert.Equal(58.5f, (float)sensors[0]["average"]);
    }

    [Fact]
    public void Execute_NoData_ReturnsEmptyHardwareList()
    {
        // Arrange
        var monitor = new FakeSensorMonitor
        {
            State = MonitoringState.Stopped,
            SnapshotCount = 0,
            IntervalSeconds = 0
        };

        // Act
        var json = GetSensorDataTool.Execute(
            NullLogger<GetSensorDataTool>.Instance,
            monitor,
            new FakePrivilegeChecker());

        // Assert
        var doc = JObject.Parse(json);
        var data = doc["data"];
        Assert.Equal("stopped", (string)data["state"]);
        Assert.Equal(0, (int)data["snapshotCount"]);

        var hardware = (JArray)data["hardware"];
        Assert.Empty(hardware);
    }

    [Fact]
    public void Execute_MultipleHardware_ReturnsAll()
    {
        // Arrange
        var monitor = new FakeSensorMonitor
        {
            State = MonitoringState.Running,
            IntervalSeconds = 5,
            SnapshotCount = 10,
            HardwareSummaries =
            [
                new HardwareSensorSummary
                {
                    Name = "CPU",
                    Category = HardwareCategory.Cpu,
                    Sensors =
                    [
                        new SensorStatistics
                        {
                            Name = "Core Temp",
                            Category = SensorCategory.Temperature,
                            Current = 60.0f,
                            Min = 40.0f,
                            Max = 80.0f,
                            Average = 55.0f
                        }
                    ]
                },
                new HardwareSensorSummary
                {
                    Name = "GPU",
                    Category = HardwareCategory.Gpu,
                    Sensors =
                    [
                        new SensorStatistics
                        {
                            Name = "GPU Temp",
                            Category = SensorCategory.Temperature,
                            Current = 45.0f,
                            Min = 35.0f,
                            Max = 70.0f,
                            Average = 50.0f
                        }
                    ]
                }
            ]
        };

        // Act
        var json = GetSensorDataTool.Execute(
            NullLogger<GetSensorDataTool>.Instance,
            monitor,
            new FakePrivilegeChecker());

        // Assert
        var doc = JObject.Parse(json);
        var hardware = (JArray)doc["data"]["hardware"];
        Assert.Equal(2, hardware.Count);
        Assert.Equal("CPU", (string)hardware[0]["name"]);
        Assert.Equal("GPU", (string)hardware[1]["name"]);
    }

    [Fact]
    public void Execute_ResponseIncludesMetadata()
    {
        // Arrange
        var monitor = new FakeSensorMonitor();
        var privilegeChecker = new FakePrivilegeChecker { IsElevated = true };

        // Act
        var json = GetSensorDataTool.Execute(
            NullLogger<GetSensorDataTool>.Instance,
            monitor,
            privilegeChecker);

        // Assert
        var doc = JObject.Parse(json);
        Assert.Equal("windows", (string)doc["platform"]);
        Assert.True((bool)doc["isElevated"]);
    }
}