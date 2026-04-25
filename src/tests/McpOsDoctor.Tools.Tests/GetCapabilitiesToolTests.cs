using McpOsDoctor.Tools.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace McpOsDoctor.Tools.Tests;

/// <summary>
/// Unit tests for <see cref="GetCapabilitiesTool"/>.
/// </summary>
public class GetCapabilitiesToolTests
{
    [Fact]
    public void Execute_ReturnsValidJson_WithCapabilities()
    {
        // Act
        var json = GetCapabilitiesTool.Execute(
            NullLogger<GetCapabilitiesTool>.Instance,
            new FakePrivilegeChecker(),
            new FakeServerInfoProvider(),
            new FakeGpuInfoProvider(),
            new FakeDirectXInfoProvider());

        // Assert
        Assert.NotNull(json);
        var doc = JObject.Parse(json);
        Assert.NotNull(doc["data"]);
        Assert.NotNull(doc["data"]["platform"]);
        Assert.Equal("Windows", (string)doc["data"]["platform"]);
    }

    [Fact]
    public void Execute_IncludesAllTwelveTools()
    {
        // Act
        var json = GetCapabilitiesTool.Execute(
            NullLogger<GetCapabilitiesTool>.Instance,
            new FakePrivilegeChecker(),
            new FakeServerInfoProvider(),
            new FakeGpuInfoProvider(),
            new FakeDirectXInfoProvider());

        // Assert
        var doc = JObject.Parse(json);
        var tools = (JArray)doc["data"]["tools"];
        Assert.Equal(12, tools.Count);

        var toolNames = new List<string>();
        foreach (var tool in tools)
        {
            toolNames.Add((string)tool["name"]);
        }

        Assert.Contains("get_capabilities", toolNames);
        Assert.Contains("query_system_log", toolNames);
        Assert.Contains("list_log_sources", toolNames);
        Assert.Contains("get_service_status", toolNames);
        Assert.Contains("list_top_processes", toolNames);
        Assert.Contains("get_system_info", toolNames);
        Assert.Contains("get_boot_history", toolNames);
        Assert.Contains("get_gpu_info", toolNames);
        Assert.Contains("get_directx_info", toolNames);
        Assert.Contains("start_sensor_monitoring", toolNames);
        Assert.Contains("stop_sensor_monitoring", toolNames);
        Assert.Contains("get_sensor_data", toolNames);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Execute_ReflectsElevationStatus(bool isElevated)
    {
        // Arrange
        var privilegeChecker = new FakePrivilegeChecker { IsElevated = isElevated };

        // Act
        var json = GetCapabilitiesTool.Execute(
            NullLogger<GetCapabilitiesTool>.Instance,
            privilegeChecker,
            new FakeServerInfoProvider(),
            new FakeGpuInfoProvider(),
            new FakeDirectXInfoProvider());

        // Assert
        var doc = JObject.Parse(json);
        var dataElevated = (bool)doc["data"]["isElevated"];
        Assert.Equal(isElevated, dataElevated);

        var responseElevated = (bool)doc["isElevated"];
        Assert.Equal(isElevated, responseElevated);
    }

    [Fact]
    public void Execute_GpuAvailable_MarksGpuToolAsAvailableWithoutReason()
    {
        // Act
        var json = GetCapabilitiesTool.Execute(
            NullLogger<GetCapabilitiesTool>.Instance,
            new FakePrivilegeChecker(),
            new FakeServerInfoProvider(),
            new FakeGpuInfoProvider(),
            new FakeDirectXInfoProvider());

        // Assert
        var doc = JObject.Parse(json);
        var tools = (JArray)doc["data"]["tools"];

        var gpuTool = tools.First(t => (string)t["name"] == "get_gpu_info");
        Assert.True((bool)gpuTool["available"]);
        Assert.Null(gpuTool["unavailableReason"]);
    }

    [Fact]
    public void Execute_GpuUnavailable_MarksGpuToolAsUnavailableWithReason()
    {
        // Arrange
        const string expectedReason = "nvidia-smi was not found on this system.";
        var gpuProvider = new FakeGpuInfoProvider
        {
            IsAvailable = false,
            UnavailableReason = expectedReason
        };

        // Act
        var json = GetCapabilitiesTool.Execute(
            NullLogger<GetCapabilitiesTool>.Instance,
            new FakePrivilegeChecker(),
            new FakeServerInfoProvider(),
            gpuProvider,
            new FakeDirectXInfoProvider());

        // Assert
        var doc = JObject.Parse(json);
        var tools = (JArray)doc["data"]["tools"];

        var gpuTool = tools.First(t => (string)t["name"] == "get_gpu_info");
        Assert.False((bool)gpuTool["available"]);
        Assert.Equal(expectedReason, (string)gpuTool["unavailableReason"]);
    }

    [Fact]
    public void Execute_DirectXAvailable_MarksDirectXToolAsAvailableWithoutReason()
    {
        // Act
        var json = GetCapabilitiesTool.Execute(
            NullLogger<GetCapabilitiesTool>.Instance,
            new FakePrivilegeChecker(),
            new FakeServerInfoProvider(),
            new FakeGpuInfoProvider(),
            new FakeDirectXInfoProvider());

        // Assert
        var doc = JObject.Parse(json);
        var tools = (JArray)doc["data"]["tools"];

        var dxTool = tools.First(t => (string)t["name"] == "get_directx_info");
        Assert.True((bool)dxTool["available"]);
        Assert.Null(dxTool["unavailableReason"]);
    }

    [Fact]
    public void Execute_DirectXUnavailable_MarksDirectXToolAsUnavailableWithReason()
    {
        // Arrange
        const string expectedReason = "dxdiag was not found on this system.";
        var dxProvider = new FakeDirectXInfoProvider
        {
            IsAvailable = false,
            UnavailableReason = expectedReason
        };

        // Act
        var json = GetCapabilitiesTool.Execute(
            NullLogger<GetCapabilitiesTool>.Instance,
            new FakePrivilegeChecker(),
            new FakeServerInfoProvider(),
            new FakeGpuInfoProvider(),
            dxProvider);

        // Assert
        var doc = JObject.Parse(json);
        var tools = (JArray)doc["data"]["tools"];

        var dxTool = tools.First(t => (string)t["name"] == "get_directx_info");
        Assert.False((bool)dxTool["available"]);
        Assert.Equal(expectedReason, (string)dxTool["unavailableReason"]);
    }

    [Fact]
    public void Execute_IncludesServerVersion_FromProvider()
    {
        // Arrange
        const string expectedVersion = "2.5.0-beta";
        var serverInfo = new FakeServerInfoProvider { Version = expectedVersion };

        // Act
        var json = GetCapabilitiesTool.Execute(
            NullLogger<GetCapabilitiesTool>.Instance,
            new FakePrivilegeChecker(),
            serverInfo,
            new FakeGpuInfoProvider(),
            new FakeDirectXInfoProvider());

        // Assert
        var doc = JObject.Parse(json);
        var serverVersion = (string)doc["data"]["serverVersion"];
        Assert.Equal(expectedVersion, serverVersion);
    }
}