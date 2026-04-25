using McpOsDoctor.Core.Enums;
using McpOsDoctor.Core.Errors;
using McpOsDoctor.Core.Mappers;
using Xunit;

namespace McpOsDoctor.Core.Tests;

/// <summary>
/// Unit tests for <see cref="Mapper"/> methods.
/// </summary>
public class MapperTests
{
    #region TryToDateTimeOffset

    [Fact]
    public void TryToDateTimeOffset_ValidIso8601_ReturnsParsedValue()
    {
        // Arrange
        var input = "2025-06-15T10:30:00Z";

        // Act
        var result = Mapper.TryToDateTimeOffset(input, "from");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2025, result.Value.Year);
        Assert.Equal(6, result.Value.Month);
        Assert.Equal(15, result.Value.Day);
    }

    [Fact]
    public void TryToDateTimeOffset_ValidWithOffset_ReturnsParsedValue()
    {
        // Arrange
        var input = "2025-06-15T10:30:00+05:00";

        // Act
        var result = Mapper.TryToDateTimeOffset(input, "from");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TimeSpan.FromHours(5), result.Value.Offset);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryToDateTimeOffset_NullOrWhitespace_ReturnsNull(string value)
    {
        // Act
        var result = Mapper.TryToDateTimeOffset(value, "from");

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("not-a-date")]
    [InlineData("abc123")]
    [InlineData("2025-13-40")]
    public void TryToDateTimeOffset_InvalidFormat_ThrowsInvalidParameter(string value)
    {
        // Act & Assert
        var ex = Assert.Throws<McpOsDoctorException>(
            () => Mapper.TryToDateTimeOffset(value, "from"));
        Assert.Contains("not a valid ISO 8601", ex.Message);
        Assert.Contains("from", ex.Message);
    }

    [Fact]
    public void TryToDateTimeOffset_InvalidFormat_IncludesParameterNameInMessage()
    {
        // Act & Assert
        var ex = Assert.Throws<McpOsDoctorException>(
            () => Mapper.TryToDateTimeOffset("bad", "myParam"));
        Assert.Contains("'myParam'", ex.Message);
    }

    #endregion

    #region TryToServiceRunState

    [Theory]
    [InlineData("Running", ServiceRunState.Running)]
    [InlineData("Stopped", ServiceRunState.Stopped)]
    [InlineData("Paused", ServiceRunState.Paused)]
    [InlineData("StartPending", ServiceRunState.StartPending)]
    [InlineData("StopPending", ServiceRunState.StopPending)]
    [InlineData("Unknown", ServiceRunState.Unknown)]
    public void TryToServiceRunState_ValidValues_ReturnsParsedEnum(string input, ServiceRunState expected)
    {
        // Act
        var result = Mapper.TryToServiceRunState(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("running")]
    [InlineData("RUNNING")]
    [InlineData("rUnNiNg")]
    public void TryToServiceRunState_CaseInsensitive_ReturnsParsedEnum(string input)
    {
        // Act
        var result = Mapper.TryToServiceRunState(input);

        // Assert
        Assert.Equal(ServiceRunState.Running, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryToServiceRunState_NullOrWhitespace_ReturnsNull(string value)
    {
        // Act
        var result = Mapper.TryToServiceRunState(value);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void TryToServiceRunState_InvalidValue_ThrowsInvalidParameter()
    {
        // Act & Assert
        var ex = Assert.Throws<McpOsDoctorException>(
            () => Mapper.TryToServiceRunState("InvalidState"));
        Assert.Contains("Invalid status", ex.Message);
    }

    #endregion

    #region TryToProcessSortField

    [Theory]
    [InlineData("cpu", ProcessSortField.Cpu)]
    [InlineData("memory", ProcessSortField.Memory)]
    [InlineData("name", ProcessSortField.Name)]
    public void TryToProcessSortField_ValidValues_ReturnsParsedEnum(string input, ProcessSortField expected)
    {
        // Act
        var result = Mapper.TryToProcessSortField(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("CPU")]
    [InlineData("Cpu")]
    [InlineData("MEMORY")]
    public void TryToProcessSortField_CaseInsensitive_ReturnsParsedEnum(string input)
    {
        // Act
        var result = Mapper.TryToProcessSortField(input);

        // Assert
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryToProcessSortField_NullOrWhitespace_ReturnsNull(string value)
    {
        // Act
        var result = Mapper.TryToProcessSortField(value);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void TryToProcessSortField_InvalidValue_ThrowsInvalidParameter()
    {
        // Act & Assert
        var ex = Assert.Throws<McpOsDoctorException>(
            () => Mapper.TryToProcessSortField("invalid"));
        Assert.Contains("Invalid sortBy", ex.Message);
    }

    #endregion

    #region TryToLogSeverity

    [Theory]
    [InlineData("Verbose", LogSeverity.Verbose)]
    [InlineData("Information", LogSeverity.Information)]
    [InlineData("Warning", LogSeverity.Warning)]
    [InlineData("Error", LogSeverity.Error)]
    [InlineData("Critical", LogSeverity.Critical)]
    public void TryToLogSeverity_ValidValues_ReturnsParsedEnum(string input, LogSeverity expected)
    {
        // Act
        var result = Mapper.TryToLogSeverity(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("error")]
    [InlineData("ERROR")]
    [InlineData("Warning")]
    public void TryToLogSeverity_CaseInsensitive_ReturnsParsedEnum(string input)
    {
        // Act
        var result = Mapper.TryToLogSeverity(input);

        // Assert
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void TryToLogSeverity_NullOrEmpty_ReturnsNull(string value)
    {
        // Act
        var result = Mapper.TryToLogSeverity(value);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void TryToLogSeverity_InvalidValue_ThrowsInvalidParameter()
    {
        // Act & Assert
        var ex = Assert.Throws<McpOsDoctorException>(
            () => Mapper.TryToLogSeverity("NotALevel"));
        Assert.Contains("Invalid severity", ex.Message);
    }

    #endregion
}