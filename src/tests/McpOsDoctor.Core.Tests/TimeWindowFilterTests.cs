using McpOsDoctor.Core.Errors;
using McpOsDoctor.Core.Filters;
using Xunit;

namespace McpOsDoctor.Core.Tests;

/// <summary>
/// Unit tests for <see cref="TimeWindowFilter"/>.
/// </summary>
public class TimeWindowFilterTests
{
    #region Time Window Normalization

    [Fact]
    public void Constructor_NullFromAndTo_DefaultsToLast24Hours()
    {
        // Act
        var filter = new TimeWindowFilter(null, null, null);

        // Assert
        Assert.True(filter.To <= DateTimeOffset.UtcNow);
        Assert.True((filter.To - filter.From).TotalHours is >= 23.9 and <= 24.1);
    }

    [Fact]
    public void Constructor_ExplicitFromAndTo_UsesProvidedValues()
    {
        // Arrange
        var expectedFrom = DateTimeOffset.UtcNow.AddDays(-2);
        var expectedTo = DateTimeOffset.UtcNow.AddHours(-1);

        // Act
        var filter = new TimeWindowFilter(null, expectedFrom, expectedTo);

        // Assert
        Assert.Equal(expectedFrom, filter.From);
        Assert.Equal(expectedTo, filter.To);
    }

    [Fact]
    public void Constructor_OnlyFromProvided_DefaultsToToNow()
    {
        // Arrange
        var from = DateTimeOffset.UtcNow.AddHours(-6);

        // Act
        var filter = new TimeWindowFilter(null, from, null);

        // Assert
        Assert.Equal(from, filter.From);
        Assert.True((DateTimeOffset.UtcNow - filter.To).TotalSeconds < 5);
    }

    [Fact]
    public void Constructor_OnlyToProvided_DefaultsFromToNowMinus24Hours()
    {
        // Arrange
        var to = DateTimeOffset.UtcNow;

        // Act
        var filter = new TimeWindowFilter(null, null, to);

        // Assert
        Assert.Equal(to, filter.To);
        Assert.True((DateTimeOffset.UtcNow - filter.From).TotalHours is >= 23.9 and <= 24.1);
    }

    [Fact]
    public void Constructor_FromAfterTo_ThrowsInvalidParameter()
    {
        // Arrange
        var from = DateTimeOffset.UtcNow;
        var to = DateTimeOffset.UtcNow.AddHours(-2);

        // Act & Assert
        var ex = Assert.Throws<McpOsDoctorException>(
            () => new TimeWindowFilter(null, from, to));
        Assert.Contains("'from' must be before 'to'", ex.Message);
    }

    [Fact]
    public void Constructor_FutureTo_ThrowsInvalidParameter()
    {
        // Arrange
        var from = DateTimeOffset.UtcNow.AddHours(-1);
        var to = DateTimeOffset.UtcNow.AddHours(1);

        // Act & Assert
        var ex = Assert.Throws<McpOsDoctorException>(
            () => new TimeWindowFilter(null, from, to));
        Assert.Contains("'to' cannot be in the future", ex.Message);
    }

    [Fact]
    public void Constructor_RangeExceedsMaxDays_ThrowsInvalidParameter()
    {
        // Arrange
        var from = DateTimeOffset.UtcNow.AddDays(-10);
        var to = DateTimeOffset.UtcNow;

        // Act & Assert
        var ex = Assert.Throws<McpOsDoctorException>(
            () => new TimeWindowFilter(null, from, to));
        Assert.Contains("exceeds the maximum of 7 days", ex.Message);
    }

    [Fact]
    public void Constructor_ExactlyMaxDays_DoesNotThrow()
    {
        // Arrange
        var to = DateTimeOffset.UtcNow;
        var from = to.AddDays(-7);

        // Act
        var filter = new TimeWindowFilter(null, from, to);

        // Assert
        Assert.Equal(from, filter.From);
        Assert.Equal(to, filter.To);
    }

    #endregion

    #region MaxResults Clamping

    [Fact]
    public void Constructor_NullMaxResults_DefaultsTo100()
    {
        // Act
        var filter = new TimeWindowFilter(null, null, null);

        // Assert
        Assert.Equal(100, filter.MaxResults);
    }

    [Fact]
    public void Constructor_MaxResultsWithinRange_UsesProvidedValue()
    {
        // Act
        var filter = new TimeWindowFilter(50, null, null);

        // Assert
        Assert.Equal(50, filter.MaxResults);
    }

    [Fact]
    public void Constructor_MaxResultsExceedsHardMax_ClampsTo500()
    {
        // Act
        var filter = new TimeWindowFilter(1000, null, null);

        // Assert
        Assert.Equal(500, filter.MaxResults);
    }

    #endregion
}