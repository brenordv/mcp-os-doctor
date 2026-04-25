using McpOsDoctor.Core.Models;
using McpOsDoctor.Tools.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace McpOsDoctor.Tools.Tests;

/// <summary>
/// Unit tests for <see cref="GetDirectXInfoTool"/>.
/// </summary>
public class GetDirectXInfoToolTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsDirectXInfo()
    {
        // Arrange
        var provider = new FakeDirectXInfoProvider
        {
            Info = new DirectXInfo
            {
                DirectXVersion = "DirectX 12",
                DxDiagVersion = "10.00.19041.5438 64bit Unicode",
                Notes = ["No problems found."],
                DisplayDevices =
                [
                    new DirectXDisplayDevice
                    {
                        CardName = "NVIDIA GeForce RTX 4080",
                        Manufacturer = "NVIDIA",
                        ChipType = "NVIDIA GeForce RTX 4080",
                        DisplayMemoryMb = 48767,
                        DedicatedMemoryMb = 16047,
                        SharedMemoryMb = 32720,
                        DriverVersion = "32.0.15.9576",
                        FeatureLevels = "12_1,12_0,11_1,11_0",
                        DriverModel = "WDDM 2.7"
                    }
                ],
                SoundDevices =
                [
                    new DirectXSoundDevice
                    {
                        Description = "Speakers (Realtek Audio)",
                        DefaultSoundPlayback = true,
                        DefaultVoicePlayback = true,
                        DriverName = "RTKVHD64.sys",
                        DriverVersion = "6.0.9411.1",
                        DriverProvider = "Realtek Semiconductor Corp."
                    }
                ]
            }
        };

        // Act
        var json = await GetDirectXInfoTool.ExecuteAsync(
            NullLogger<GetDirectXInfoTool>.Instance,
            provider,
            new FakePrivilegeChecker(),
            TestContext.Current.CancellationToken);

        // Assert
        var doc = JObject.Parse(json);
        var data = doc["data"];
        Assert.Equal("DirectX 12", (string)data["directXVersion"]);
        Assert.Equal("10.00.19041.5438 64bit Unicode", (string)data["dxDiagVersion"]);

        var displays = (JArray)data["displayDevices"];
        Assert.Single(displays);
        Assert.Equal("NVIDIA GeForce RTX 4080", (string)displays[0]["cardName"]);
        Assert.Equal(16047, (int)displays[0]["dedicatedMemoryMb"]);
        Assert.Equal("WDDM 2.7", (string)displays[0]["driverModel"]);

        var sounds = (JArray)data["soundDevices"];
        Assert.Single(sounds);
        Assert.Equal("Speakers (Realtek Audio)", (string)sounds[0]["description"]);
        Assert.True((bool)sounds[0]["defaultSoundPlayback"]);
    }

    [Fact]
    public async Task ExecuteAsync_MultipleDisplayDevices_ReturnsAll()
    {
        // Arrange
        var provider = new FakeDirectXInfoProvider
        {
            Info = new DirectXInfo
            {
                DirectXVersion = "DirectX 12",
                DxDiagVersion = "10.00.19041.5438 64bit Unicode",
                Notes = [],
                DisplayDevices =
                [
                    new DirectXDisplayDevice
                    {
                        CardName = "Display 0",
                        Manufacturer = "NVIDIA",
                        ChipType = "GPU A",
                        DisplayMemoryMb = 8192,
                        DedicatedMemoryMb = 4096,
                        SharedMemoryMb = 4096,
                        DriverVersion = "32.0.15.9576"
                    },
                    new DirectXDisplayDevice
                    {
                        CardName = "Display 1",
                        Manufacturer = "NVIDIA",
                        ChipType = "GPU B",
                        DisplayMemoryMb = 8192,
                        DedicatedMemoryMb = 4096,
                        SharedMemoryMb = 4096,
                        DriverVersion = "32.0.15.9576"
                    }
                ],
                SoundDevices = []
            }
        };

        // Act
        var json = await GetDirectXInfoTool.ExecuteAsync(
            NullLogger<GetDirectXInfoTool>.Instance,
            provider,
            new FakePrivilegeChecker(),
            TestContext.Current.CancellationToken);

        // Assert
        var doc = JObject.Parse(json);
        var displays = (JArray)doc["data"]["displayDevices"];
        Assert.Equal(2, displays.Count);
        Assert.Equal("Display 0", (string)displays[0]["cardName"]);
        Assert.Equal("Display 1", (string)displays[1]["cardName"]);
    }

    [Fact]
    public async Task ExecuteAsync_DxDiagUnavailable_ReturnsNotSupportedError()
    {
        // Arrange
        var provider = new FakeDirectXInfoProvider
        {
            IsAvailable = false,
            UnavailableReason = "dxdiag was not found on this system."
        };

        // Act
        var json = await GetDirectXInfoTool.ExecuteAsync(
            NullLogger<GetDirectXInfoTool>.Instance,
            provider,
            new FakePrivilegeChecker(),
            TestContext.Current.CancellationToken);

        // Assert
        var doc = JObject.Parse(json);
        Assert.Equal("notSupported", (string)doc["code"]);
        Assert.False((bool)doc["isRetryable"]);
    }
}