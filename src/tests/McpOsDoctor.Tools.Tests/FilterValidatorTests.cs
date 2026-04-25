using McpOsDoctor.Core.Errors;
using Xunit;

namespace McpOsDoctor.Tools.Tests;

/// <summary>
/// Unit tests for <see cref="FilterValidator"/>.
/// </summary>
public class FilterValidatorTests
{
    private const int DefaultMax = 100;
    private const int HardMax = 500;

    #region ClampMaxResults

    [Fact]
    public void ClampMaxResults_NullRequested_ReturnsDefault()
    {
        // Arrange
        int? requested = null;

        // Act
        var result = FilterValidator.ClampMaxResults(requested, DefaultMax, HardMax);

        // Assert
        Assert.Equal(100, result);
    }

    [Fact]
    public void ClampMaxResults_ExceedsHardMax_ClampsToHardMax()
    {
        // Arrange
        int? requested = 1000;

        // Act
        var result = FilterValidator.ClampMaxResults(requested, DefaultMax, HardMax);

        // Assert
        Assert.Equal(500, result);
    }

    [Fact]
    public void ClampMaxResults_WithinRange_ReturnsRequested()
    {
        // Arrange
        int? requested = 50;

        // Act
        var result = FilterValidator.ClampMaxResults(requested, DefaultMax, HardMax);

        // Assert
        Assert.Equal(50, result);
    }

    [Fact]
    public void ClampMaxResults_BelowMinimum_ClampsToOne()
    {
        // Arrange
        int? requested = 0;

        // Act
        var result = FilterValidator.ClampMaxResults(requested, DefaultMax, HardMax);

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void ClampMaxResults_NegativeValue_ClampsToOne()
    {
        // Arrange
        int? requested = -5;
        // Act
        var result = FilterValidator.ClampMaxResults(requested, DefaultMax, HardMax);

        // Assert
        Assert.Equal(1, result);
    }

    #endregion

    #region ValidateKeywords

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ValidateKeywords_NullOrEmpty_DoesNotThrow(string keywords)
    {
        // Arrange & Act & Assert
        var exception = Record.Exception(() => FilterValidator.ValidateKeywords(keywords));
        Assert.Null(exception);
    }

    [Fact]
    public void ValidateKeywords_WithinLimit_DoesNotThrow()
    {
        // Arrange
        var keywords = new string('a', FilterValidator.MaxKeywordsLength);

        // Act & Assert
        var exception = Record.Exception(() => FilterValidator.ValidateKeywords(keywords));
        Assert.Null(exception);
    }

    [Fact]
    public void ValidateKeywords_ExceedsLimit_ThrowsInvalidParameter()
    {
        // Arrange
        var keywords = new string('a', FilterValidator.MaxKeywordsLength + 1);

        // Act & Assert
        var ex = Assert.Throws<McpOsDoctorException>(
            () => FilterValidator.ValidateKeywords(keywords));
        Assert.Contains("exceeds maximum length", ex.Message);
    }

    #endregion
}