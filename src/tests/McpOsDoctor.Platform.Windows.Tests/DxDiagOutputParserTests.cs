using McpOsDoctor.Core.Errors;
using Xunit;

namespace McpOsDoctor.Platform.Windows.Tests;

/// <summary>
/// Unit tests for <see cref="DxDiagOutputParser"/>.
/// </summary>
public class DxDiagOutputParserTests
{
    private const string MinimalOutput =
        """
        ------------------
        System Information
        ------------------
              DirectX Version: DirectX 12
              DxDiag Version: 10.00.19041.5438 64bit Unicode

        ------------
        DxDiag Notes
        ------------
              Display Tab 1: No problems found.

        ---------------
        Display Devices
        ---------------
                   Card name: NVIDIA GeForce RTX 4080 SUPER
                Manufacturer: NVIDIA
                   Chip type: NVIDIA GeForce RTX 4080 SUPER
                 Device Type: Full Device (POST)
              Display Memory: 48767 MB
            Dedicated Memory: 16047 MB
               Shared Memory: 32720 MB
                Current Mode: 2560 x 1440 (32 bit) (144Hz)
                 HDR Support: Supported
                Monitor Name: Generic PnP Monitor
               Monitor Model: M27Q
                 Output Type: HDMI
              Driver Version: 32.0.15.9576
                 DDI Version: 12
              Feature Levels: 12_1,12_0,11_1,11_0,10_1,10_0,9_3,9_2,9_1
                Driver Model: WDDM 2.7
         Hardware Scheduling: Supported:True Enabled:True

        -------------
        Sound Devices
        -------------
                    Description: Speakers (Realtek(R) Audio)
         Default Sound Playback: Yes
         Default Voice Playback: Yes
                    Driver Name: RTKVHD64.sys
                 Driver Version: 6.0.9411.1 (English)
                Driver Provider: Realtek Semiconductor Corp.
        """;

    [Fact]
    public void Parse_MinimalOutput_ReturnsSystemInfo()
    {
        // Act
        var info = DxDiagOutputParser.Parse(MinimalOutput);

        // Assert
        Assert.Equal("DirectX 12", info.DirectXVersion);
        Assert.Equal("10.00.19041.5438 64bit Unicode", info.DxDiagVersion);
    }

    [Fact]
    public void Parse_MinimalOutput_ReturnsNotes()
    {
        // Act
        var info = DxDiagOutputParser.Parse(MinimalOutput);

        // Assert
        Assert.Single(info.Notes);
        Assert.Equal("No problems found.", info.Notes[0]);
    }

    [Fact]
    public void Parse_MinimalOutput_ReturnsDisplayDevice()
    {
        // Act
        var info = DxDiagOutputParser.Parse(MinimalOutput);

        // Assert
        Assert.Single(info.DisplayDevices);

        var display = info.DisplayDevices[0];
        Assert.Equal("NVIDIA GeForce RTX 4080 SUPER", display.CardName);
        Assert.Equal("NVIDIA", display.Manufacturer);
        Assert.Equal("NVIDIA GeForce RTX 4080 SUPER", display.ChipType);
        Assert.Equal("Full Device (POST)", display.DeviceType);
        Assert.Equal(48767, display.DisplayMemoryMb);
        Assert.Equal(16047, display.DedicatedMemoryMb);
        Assert.Equal(32720, display.SharedMemoryMb);
        Assert.Equal("2560 x 1440 (32 bit) (144Hz)", display.CurrentMode);
        Assert.Equal("Supported", display.HdrSupport);
        Assert.Equal("Generic PnP Monitor", display.MonitorName);
        Assert.Equal("M27Q", display.MonitorModel);
        Assert.Equal("HDMI", display.OutputType);
        Assert.Equal("32.0.15.9576", display.DriverVersion);
        Assert.Equal("12", display.DdiVersion);
        Assert.Equal("12_1,12_0,11_1,11_0,10_1,10_0,9_3,9_2,9_1", display.FeatureLevels);
        Assert.Equal("WDDM 2.7", display.DriverModel);
        Assert.Equal("Supported:True Enabled:True", display.HardwareScheduling);
    }

    [Fact]
    public void Parse_MinimalOutput_ReturnsSoundDevice()
    {
        // Act
        var info = DxDiagOutputParser.Parse(MinimalOutput);

        // Assert
        Assert.Single(info.SoundDevices);

        var sound = info.SoundDevices[0];
        Assert.Equal("Speakers (Realtek(R) Audio)", sound.Description);
        Assert.True(sound.DefaultSoundPlayback);
        Assert.True(sound.DefaultVoicePlayback);
        Assert.Equal("RTKVHD64.sys", sound.DriverName);
        Assert.Equal("6.0.9411.1 (English)", sound.DriverVersion);
        Assert.Equal("Realtek Semiconductor Corp.", sound.DriverProvider);
    }

    [Fact]
    public void Parse_MultipleDisplayDevices_ReturnsAll()
    {
        // Arrange
        const string output = """
                              ------------------
                              System Information
                              ------------------
                                    DirectX Version: DirectX 12
                                    DxDiag Version: 10.00.19041.5438

                              ---------------
                              Display Devices
                              ---------------
                                         Card name: GPU A
                                      Manufacturer: NVIDIA
                                         Chip type: Chip A
                                    Display Memory: 8192 MB
                                  Dedicated Memory: 4096 MB
                                     Shared Memory: 4096 MB
                                    Driver Version: 1.0.0

                                         Card name: GPU B
                                      Manufacturer: AMD
                                         Chip type: Chip B
                                    Display Memory: 16384 MB
                                  Dedicated Memory: 8192 MB
                                     Shared Memory: 8192 MB
                                    Driver Version: 2.0.0
                              """;

        // Act
        var info = DxDiagOutputParser.Parse(output);

        // Assert
        Assert.Equal(2, info.DisplayDevices.Count);
        Assert.Equal("GPU A", info.DisplayDevices[0].CardName);
        Assert.Equal("NVIDIA", info.DisplayDevices[0].Manufacturer);
        Assert.Equal(4096, info.DisplayDevices[0].DedicatedMemoryMb);
        Assert.Equal("GPU B", info.DisplayDevices[1].CardName);
        Assert.Equal("AMD", info.DisplayDevices[1].Manufacturer);
        Assert.Equal(8192, info.DisplayDevices[1].DedicatedMemoryMb);
    }

    [Fact]
    public void Parse_MultipleSoundDevices_ReturnsAll()
    {
        // Arrange
        const string output = """
                              ------------------
                              System Information
                              ------------------
                                    DirectX Version: DirectX 12
                                    DxDiag Version: 10.00.19041.5438

                              -------------
                              Sound Devices
                              -------------
                                          Description: HDMI Audio
                               Default Sound Playback: No
                               Default Voice Playback: No
                                          Driver Name: nvhda64v.sys
                                       Driver Version: 1.4.5.7
                                      Driver Provider: NVIDIA Corporation

                                          Description: Speakers (Realtek)
                               Default Sound Playback: Yes
                               Default Voice Playback: Yes
                                          Driver Name: RTKVHD64.sys
                                       Driver Version: 6.0.9411.1
                                      Driver Provider: Realtek Semiconductor Corp.
                              """;

        // Act
        var info = DxDiagOutputParser.Parse(output);

        // Assert
        Assert.Equal(2, info.SoundDevices.Count);
        Assert.Equal("HDMI Audio", info.SoundDevices[0].Description);
        Assert.False(info.SoundDevices[0].DefaultSoundPlayback);
        Assert.Equal("Speakers (Realtek)", info.SoundDevices[1].Description);
        Assert.True(info.SoundDevices[1].DefaultSoundPlayback);
    }

    [Fact]
    public void Parse_NoDisplayOrSoundSections_ReturnsEmptyLists()
    {
        // Arrange
        const string output = """
                              ------------------
                              System Information
                              ------------------
                                    DirectX Version: DirectX 11
                                    DxDiag Version: 10.00.19041.5438
                              """;

        // Act
        var info = DxDiagOutputParser.Parse(output);

        // Assert
        Assert.Empty(info.DisplayDevices);
        Assert.Empty(info.SoundDevices);
    }

    [Fact]
    public void Parse_SoundDeviceDefaultsNo_ReturnsFalse()
    {
        // Arrange
        const string output = """
                              ------------------
                              System Information
                              ------------------
                                    DirectX Version: DirectX 12
                                    DxDiag Version: 10.00.19041.5438

                              -------------
                              Sound Devices
                              -------------
                                          Description: HDMI Audio
                               Default Sound Playback: No
                               Default Voice Playback: No
                                          Driver Name: nvhda64v.sys
                                       Driver Version: 1.0.0
                                      Driver Provider: NVIDIA
                              """;

        // Act
        var info = DxDiagOutputParser.Parse(output);

        // Assert
        var sound = Assert.Single(info.SoundDevices);
        Assert.False(sound.DefaultSoundPlayback);
        Assert.False(sound.DefaultVoicePlayback);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n\n")]
    public void Parse_EmptyOrWhitespace_ThrowsPlatformError(string output)
    {
        // Act & Assert
        var ex = Assert.Throws<McpOsDoctorException>(
            () => DxDiagOutputParser.Parse(output));
        Assert.Equal(Core.Enums.ToolErrorCode.PlatformError, ex.ToolError.Code);
        Assert.Contains("no data", ex.Message);
    }

    [Fact]
    public void Parse_MissingSystemInformationSection_ThrowsPlatformError()
    {
        // Arrange
        const string output = """
                              ---------------
                              Display Devices
                              ---------------
                                         Card name: Test GPU
                                      Manufacturer: NVIDIA
                                         Chip type: Test Chip
                                    Display Memory: 8192 MB
                                  Dedicated Memory: 4096 MB
                                     Shared Memory: 4096 MB
                                    Driver Version: 1.0.0
                              """;

        // Act & Assert
        var ex = Assert.Throws<McpOsDoctorException>(
            () => DxDiagOutputParser.Parse(output));
        Assert.Equal(Core.Enums.ToolErrorCode.PlatformError, ex.ToolError.Code);
        Assert.Contains("System Information", ex.Message);
    }

    [Fact]
    public void Parse_MissingDirectXVersionField_ThrowsPlatformError()
    {
        // Arrange
        const string output = """
                              ------------------
                              System Information
                              ------------------
                                    DxDiag Version: 10.00.19041.5438
                              """;

        // Act & Assert
        var ex = Assert.Throws<McpOsDoctorException>(
            () => DxDiagOutputParser.Parse(output));
        Assert.Equal(Core.Enums.ToolErrorCode.PlatformError, ex.ToolError.Code);
        Assert.Contains("DirectX Version", ex.Message);
    }

    [Fact]
    public void Parse_UnparseableMemoryValue_ThrowsPlatformError()
    {
        // Arrange
        const string output = """
                              ------------------
                              System Information
                              ------------------
                                    DirectX Version: DirectX 12
                                    DxDiag Version: 10.00.19041.5438

                              ---------------
                              Display Devices
                              ---------------
                                         Card name: Test GPU
                                      Manufacturer: NVIDIA
                                         Chip type: Test Chip
                                    Display Memory: not_a_number MB
                                  Dedicated Memory: 4096 MB
                                     Shared Memory: 4096 MB
                                    Driver Version: 1.0.0
                              """;

        // Act & Assert
        var ex = Assert.Throws<McpOsDoctorException>(
            () => DxDiagOutputParser.Parse(output));
        Assert.Equal(Core.Enums.ToolErrorCode.PlatformError, ex.ToolError.Code);
        Assert.Contains("Display Memory", ex.Message);
    }

    [Fact]
    public void Parse_DisplayDeviceWithOptionalFieldsMissing_ReturnsNullForOptionals()
    {
        // Arrange
        const string output = """
                              ------------------
                              System Information
                              ------------------
                                    DirectX Version: DirectX 12
                                    DxDiag Version: 10.00.19041.5438

                              ---------------
                              Display Devices
                              ---------------
                                         Card name: Minimal GPU
                                      Manufacturer: Generic
                                         Chip type: Generic Chip
                                    Display Memory: 1024 MB
                                  Dedicated Memory: 512 MB
                                     Shared Memory: 512 MB
                                    Driver Version: 1.0.0
                              """;

        // Act
        var info = DxDiagOutputParser.Parse(output);

        // Assert
        var display = Assert.Single(info.DisplayDevices);
        Assert.Equal("Minimal GPU", display.CardName);
        Assert.Null(display.DeviceType);
        Assert.Null(display.CurrentMode);
        Assert.Null(display.HdrSupport);
        Assert.Null(display.MonitorName);
        Assert.Null(display.MonitorModel);
        Assert.Null(display.OutputType);
        Assert.Null(display.DdiVersion);
        Assert.Null(display.FeatureLevels);
        Assert.Null(display.DriverModel);
        Assert.Null(display.HardwareScheduling);
    }
}