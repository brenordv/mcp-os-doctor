using McpOsDoctor.Core.DataTypes;
using McpOsDoctor.Core.Errors;
using Xunit;

namespace McpOsDoctor.Core.Tests;

/// <summary>
/// Unit tests for <see cref="SourceName"/>.
/// </summary>
public class SourceNameTests
{
    #region Validation

    [Theory]
    [InlineData("Application")]
    [InlineData("System")]
    [InlineData("Microsoft-Windows-DNS-Client/Operational")]
    [InlineData("My Source")]
    [InlineData("Test*")]
    public void Constructor_ValidNames_DoesNotThrow(string value)
    {
        // Act
        var sourceName = new SourceName(value);

        // Assert
        Assert.Equal(value, sourceName.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_NullOrWhitespace_SetsValueToNull(string value)
    {
        // Act
        var sourceName = new SourceName(value);

        // Assert
        Assert.Null(sourceName.Value);
    }

    [Fact]
    public void Constructor_ExceedsMaxLength_ThrowsInvalidParameter()
    {
        // Arrange
        var longName = new string('A', 201);

        // Act & Assert
        var ex = Assert.Throws<McpOsDoctorException>(
            () => new SourceName(longName));
        Assert.Contains("exceeds maximum length", ex.Message);
    }

    [Fact]
    public void Constructor_ExactlyMaxLength_DoesNotThrow()
    {
        // Arrange
        var name = new string('A', 200);

        // Act
        var sourceName = new SourceName(name);

        // Assert
        Assert.Equal(name, sourceName.Value);
    }

    [Theory]
    [InlineData("Source;DROP TABLE")]
    [InlineData("Test<script>")]
    [InlineData("Source$Bad")]
    [InlineData("Source@Invalid")]
    public void Constructor_InvalidCharacters_ThrowsInvalidParameter(string value)
    {
        // Act & Assert
        var ex = Assert.Throws<McpOsDoctorException>(
            () => new SourceName(value));
        Assert.Contains("invalid characters", ex.Message);
    }

    #endregion

    #region Implicit Conversions

    [Fact]
    public void ImplicitConversion_FromString_CreatesSourceName()
    {
        // Act
        SourceName sourceName = "Application";

        // Assert
        Assert.Equal("Application", sourceName.Value);
    }

    [Fact]
    public void ImplicitConversion_ToString_ReturnsValue()
    {
        // Arrange
        var sourceName = new SourceName("System");

        // Act
        string result = sourceName;

        // Assert
        Assert.Equal("System", result);
    }

    [Fact]
    public void ImplicitConversion_NullString_SetsValueToNull()
    {
        // Act
        SourceName sourceName = (string)null;

        // Assert
        Assert.Null(sourceName.Value);
    }

    #endregion

    #region Equality

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        // Arrange
        var a = new SourceName("Application");
        var b = new SourceName("Application");

        // Assert
        Assert.True(a.Equals(b));
        Assert.True(a == b);
        Assert.False(a != b);
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        // Arrange
        var a = new SourceName("Application");
        var b = new SourceName("System");

        // Assert
        Assert.False(a.Equals(b));
        Assert.False(a == b);
        Assert.True(a != b);
    }

    [Fact]
    public void Equals_BothNull_ReturnsTrue()
    {
        // Arrange
        var a = new SourceName(null);
        var b = new SourceName(null);

        // Assert
        Assert.True(a.Equals(b));
        Assert.True(a == b);
    }

    [Fact]
    public void Equals_ObjectOverload_WorksCorrectly()
    {
        // Arrange
        var a = new SourceName("Test");
        object b = new SourceName("Test");
        object c = "not a SourceName";

        // Assert
        Assert.True(a.Equals(b));
        Assert.False(a.Equals(c));
    }

    [Fact]
    public void GetHashCode_SameValue_ReturnsSameHash()
    {
        // Arrange
        var a = new SourceName("Application");
        var b = new SourceName("Application");

        // Assert
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_NullValue_ReturnsZero()
    {
        // Arrange
        var sourceName = new SourceName(null);

        // Assert
        Assert.Equal(0, sourceName.GetHashCode());
    }

    #endregion

    #region ToString

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        var sourceName = new SourceName("Application");

        // Assert
        Assert.Equal("Application", sourceName.ToString());
    }

    [Fact]
    public void ToString_NullValue_ReturnsNull()
    {
        // Arrange
        var sourceName = new SourceName(null);

        // Assert
        Assert.Null(sourceName.ToString());
    }

    #endregion
}