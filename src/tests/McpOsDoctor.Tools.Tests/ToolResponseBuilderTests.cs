using System.Diagnostics;
using McpOsDoctor.Tools.Tests.Fakes;
using Xunit;

namespace McpOsDoctor.Tools.Tests;

/// <summary>
/// Unit tests for <see cref="ToolResponseBuilder"/>.
/// </summary>
public class ToolResponseBuilderTests
{
    [Fact]
    public void Build_WithData_SetsDataOnResponse()
    {
        // Arrange
        var data = "test-payload";
        var sw = Stopwatch.StartNew();
        var privilegeChecker = new FakePrivilegeChecker();

        // Act
        var response = ToolResponseBuilder.Build(data, sw, privilegeChecker);

        // Assert
        Assert.Equal("test-payload", response.Data);
    }

    [Fact]
    public void Build_WithStopwatch_SetsElapsedMs()
    {
        // Arrange
        var data = "test";
        var sw = Stopwatch.StartNew();
        var privilegeChecker = new FakePrivilegeChecker();

        // Act
        var response = ToolResponseBuilder.Build(data, sw, privilegeChecker);

        // Assert
        Assert.True(response.ElapsedMs >= 0);
    }

    [Fact]
    public void Build_WithWarnings_IncludesWarnings()
    {
        // Arrange
        var data = "test";
        var sw = Stopwatch.StartNew();
        var privilegeChecker = new FakePrivilegeChecker();
        var warnings = new List<string> { "Warning 1", "Warning 2" };

        // Act
        var response = ToolResponseBuilder.Build(data, sw, privilegeChecker, warnings);

        // Assert
        Assert.Equal(2, response.Warnings.Count);
        Assert.Contains("Warning 1", response.Warnings);
        Assert.Contains("Warning 2", response.Warnings);
    }

    [Fact]
    public void Build_WithoutWarnings_SetsEmptyList()
    {
        // Arrange
        var data = "test";
        var sw = Stopwatch.StartNew();
        var privilegeChecker = new FakePrivilegeChecker();

        // Act
        var response = ToolResponseBuilder.Build(data, sw, privilegeChecker);

        // Assert
        Assert.NotNull(response.Warnings);
        Assert.Empty(response.Warnings);
    }

    [Fact]
    public void Build_WithTotalAvailable_IncludesTotalAvailable()
    {
        // Arrange
        var data = "test";
        var sw = Stopwatch.StartNew();
        var privilegeChecker = new FakePrivilegeChecker();

        // Act
        var response = ToolResponseBuilder.Build(data, sw, privilegeChecker, totalAvailable: 42);

        // Assert
        Assert.Equal(42, response.TotalAvailable);
    }

    [Fact]
    public void Build_SetsPlatformAndElevation()
    {
        // Arrange
        var data = "test";
        var sw = Stopwatch.StartNew();
        var privilegeChecker = new FakePrivilegeChecker { IsElevated = true };

        // Act
        var response = ToolResponseBuilder.Build(data, sw, privilegeChecker);

        // Assert
        Assert.Equal("windows", response.Platform);
        Assert.True(response.IsElevated);
    }

    [Fact]
    public void Build_NotElevated_SetsIsElevatedFalse()
    {
        // Arrange
        var data = "test";
        var sw = Stopwatch.StartNew();
        var privilegeChecker = new FakePrivilegeChecker { IsElevated = false };

        // Act
        var response = ToolResponseBuilder.Build(data, sw, privilegeChecker);

        // Assert
        Assert.False(response.IsElevated);
    }
}