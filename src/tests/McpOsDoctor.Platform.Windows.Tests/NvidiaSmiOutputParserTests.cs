using McpOsDoctor.Core.Errors;
using Xunit;

namespace McpOsDoctor.Platform.Windows.Tests;

/// <summary>
/// Unit tests for <see cref="NvidiaSmiOutputParser"/>.
/// </summary>
public class NvidiaSmiOutputParserTests
{
    private const string SingleGpuOutput =
        "NVIDIA GeForce RTX 4080 SUPER, 595.76, 16376, 1409, 14639, 42, 14, 24, 19.68, 320.00, 0, P8\n";

    [Fact]
    public void ParseOutput_SingleGpu_ReturnsOneDevice()
    {
        // Act
        var devices = NvidiaSmiOutputParser.Parse(SingleGpuOutput);

        // Assert
        Assert.Single(devices);

        var gpu = devices[0];
        Assert.Equal("NVIDIA GeForce RTX 4080 SUPER", gpu.Name);
        Assert.Equal("595.76", gpu.DriverVersion);
        Assert.Equal(16376, gpu.MemoryTotalMb);
        Assert.Equal(1409, gpu.MemoryUsedMb);
        Assert.Equal(14639, gpu.MemoryFreeMb);
        Assert.Equal(42, gpu.TemperatureCelsius);
        Assert.Equal(14, gpu.GpuUtilizationPercent);
        Assert.Equal(24, gpu.MemoryUtilizationPercent);
        Assert.Equal(19.68, gpu.PowerDrawWatts);
        Assert.Equal(320.00, gpu.PowerLimitWatts);
        Assert.Equal(0, gpu.FanSpeedPercent);
        Assert.Equal("P8", gpu.PerformanceState);
    }

    [Fact]
    public void ParseOutput_MultipleGpus_ReturnsAll()
    {
        // Arrange
        var output =
            "GPU A, 595.76, 8192, 1024, 7168, 40, 10, 12, 50.00, 250.00, 30, P0\n" +
            "GPU B, 595.76, 8192, 512, 7680, 38, 5, 6, 45.00, 250.00, 25, P2\n";

        // Act
        var devices = NvidiaSmiOutputParser.Parse(output);

        // Assert
        Assert.Equal(2, devices.Count);
        Assert.Equal("GPU A", devices[0].Name);
        Assert.Equal("GPU B", devices[1].Name);
    }

    [Theory]
    [InlineData("[Not Supported]")]
    [InlineData("N/A")]
    public void ParseOutput_UnsupportedOptionalFields_ReturnsNull(string unsupportedValue)
    {
        // Arrange
        var output = $"Test GPU, 595.76, 8192, 1024, 7168, 45, 20, 15, {unsupportedValue}, {unsupportedValue}, {unsupportedValue}, {unsupportedValue}\n";

        // Act
        var devices = NvidiaSmiOutputParser.Parse(output);

        // Assert
        var gpu = Assert.Single(devices);
        Assert.Null(gpu.PowerDrawWatts);
        Assert.Null(gpu.PowerLimitWatts);
        Assert.Null(gpu.FanSpeedPercent);
        Assert.Null(gpu.PerformanceState);
    }

    [Fact]
    public void ParseOutput_LineWithTooFewFields_SkipsLine()
    {
        // Arrange
        var output =
            "Incomplete, 595.76, 8192\n" +
            "Valid GPU, 595.76, 8192, 1024, 7168, 45, 20, 15, 100.00, 250.00, 40, P0\n";

        // Act
        var devices = NvidiaSmiOutputParser.Parse(output);

        // Assert
        Assert.Single(devices);
        Assert.Equal("Valid GPU", devices[0].Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n\n")]
    public void ParseOutput_EmptyOrWhitespace_ThrowsPlatformError(string output)
    {
        // Act & Assert
        var ex = Assert.Throws<McpOsDoctorException>(
            () => NvidiaSmiOutputParser.Parse(output));
        Assert.Equal(Core.Enums.ToolErrorCode.PlatformError, ex.ToolError.Code);
        Assert.Contains("no GPU data", ex.Message);
    }

    [Fact]
    public void ParseOutput_InvalidRequiredNumericField_ThrowsPlatformError()
    {
        // Arrange — memory.total is "not_a_number"
        var output = "Test GPU, 595.76, not_a_number, 1024, 7168, 45, 20, 15, 100.00, 250.00, 40, P0\n";

        // Act & Assert
        var ex = Assert.Throws<McpOsDoctorException>(
            () => NvidiaSmiOutputParser.Parse(output));
        Assert.Equal(Core.Enums.ToolErrorCode.PlatformError, ex.ToolError.Code);
        Assert.Contains("memory.total", ex.Message);
    }

    [Fact]
    public void ParseOutput_MixedSupportedAndUnsupported_ParsesCorrectly()
    {
        // Arrange — power draw supported, power limit and fan not supported
        var output = "Test GPU, 595.76, 8192, 1024, 7168, 45, 20, 15, 150.50, [Not Supported], N/A, P0\n";

        // Act
        var devices = NvidiaSmiOutputParser.Parse(output);

        // Assert
        var gpu = Assert.Single(devices);
        Assert.Equal(150.50, gpu.PowerDrawWatts);
        Assert.Null(gpu.PowerLimitWatts);
        Assert.Null(gpu.FanSpeedPercent);
        Assert.Equal("P0", gpu.PerformanceState);
    }
}