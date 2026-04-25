using McpOsDoctor.Core.Models;
using McpOsDoctor.Tools.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace McpOsDoctor.Tools.Tests;

/// <summary>
/// Unit tests for <see cref="GetGpuInfoTool"/>.
/// </summary>
public class GetGpuInfoToolTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsGpuDeviceList()
    {
        // Arrange
        var gpuProvider = new FakeGpuInfoProvider
        {
            Devices =
            [
                new GpuInfo
                {
                    Name = "NVIDIA GeForce RTX 4080",
                    DriverVersion = "595.76",
                    MemoryTotalMb = 16376,
                    MemoryUsedMb = 1409,
                    MemoryFreeMb = 14639,
                    TemperatureCelsius = 42,
                    GpuUtilizationPercent = 14,
                    MemoryUtilizationPercent = 24,
                    PowerDrawWatts = 19.68,
                    PowerLimitWatts = 320.00,
                    FanSpeedPercent = 0,
                    PerformanceState = "P8"
                }
            ]
        };

        // Act
        var json = await GetGpuInfoTool.ExecuteAsync(
            NullLogger<GetGpuInfoTool>.Instance,
            gpuProvider,
            new FakePrivilegeChecker(),
            TestContext.Current.CancellationToken);

        // Assert
        var doc = JObject.Parse(json);
        var data = (JArray)doc["data"];
        Assert.Single(data);

        var device = data[0];
        Assert.Equal("NVIDIA GeForce RTX 4080", (string)device["name"]);
        Assert.Equal("595.76", (string)device["driverVersion"]);
        Assert.Equal(16376, (double)device["memoryTotalMb"]);
        Assert.Equal(42, (int)device["temperatureCelsius"]);
        Assert.Equal(14, (int)device["gpuUtilizationPercent"]);
        Assert.Equal("P8", (string)device["performanceState"]);
    }

    [Fact]
    public async Task ExecuteAsync_MultipleGpus_ReturnsAll()
    {
        // Arrange
        var gpuProvider = new FakeGpuInfoProvider
        {
            Devices =
            [
                new GpuInfo
                {
                    Name = "GPU 0",
                    DriverVersion = "595.76",
                    MemoryTotalMb = 8192,
                    MemoryUsedMb = 1024,
                    MemoryFreeMb = 7168,
                    TemperatureCelsius = 40,
                    GpuUtilizationPercent = 10,
                    MemoryUtilizationPercent = 12
                },
                new GpuInfo
                {
                    Name = "GPU 1",
                    DriverVersion = "595.76",
                    MemoryTotalMb = 8192,
                    MemoryUsedMb = 512,
                    MemoryFreeMb = 7680,
                    TemperatureCelsius = 38,
                    GpuUtilizationPercent = 5,
                    MemoryUtilizationPercent = 6
                }
            ]
        };

        // Act
        var json = await GetGpuInfoTool.ExecuteAsync(
            NullLogger<GetGpuInfoTool>.Instance,
            gpuProvider,
            new FakePrivilegeChecker(),
            TestContext.Current.CancellationToken);

        // Assert
        var doc = JObject.Parse(json);
        var data = (JArray)doc["data"];
        Assert.Equal(2, data.Count);
        Assert.Equal("GPU 0", (string)data[0]["name"]);
        Assert.Equal("GPU 1", (string)data[1]["name"]);
    }

    [Fact]
    public async Task ExecuteAsync_NvidiaSmiUnavailable_ReturnsNotSupportedError()
    {
        // Arrange
        var gpuProvider = new FakeGpuInfoProvider
        {
            IsAvailable = false,
            UnavailableReason = "nvidia-smi was not found on this system."
        };

        // Act
        var json = await GetGpuInfoTool.ExecuteAsync(
            NullLogger<GetGpuInfoTool>.Instance,
            gpuProvider,
            new FakePrivilegeChecker(),
            TestContext.Current.CancellationToken);

        // Assert
        var doc = JObject.Parse(json);
        Assert.Equal("notSupported", (string)doc["code"]);
        Assert.False((bool)doc["isRetryable"]);
    }
}