using Xunit;

namespace McpOsDoctor.Platform.Windows.Tests;

/// <summary>
/// Unit tests for <see cref="OutputSanitizer"/>.
/// </summary>
public class OutputSanitizerTests
{
    [Fact]
    public void Sanitize_CleanText_PassesThrough()
    {
        // Arrange
        var sut = BuildSut();

        // Act
        var result = sut.Sanitize("This is a normal log message with no secrets.");

        // Assert
        Assert.Equal("This is a normal log message with no secrets.", result);
        Assert.False(sut.LastSanitizeRedacted);
    }

    [Theory]
    [InlineData("Server=myserver;Database=db;password=s3cret;")]
    [InlineData("Data Source=srv;pwd=mypass123;User Id=admin;")]
    public void Sanitize_ConnectionStringPasswords_Redacts(string input)
    {
        // Arrange
        var sut = BuildSut();

        // Act
        var result = sut.Sanitize(input);

        // Assert
        Assert.Contains("[REDACTED]", result);
        Assert.True(sut.LastSanitizeRedacted);
    }

    [Theory]
    [InlineData("app.exe --password super_secret_pass")]
    [InlineData("tool.exe --token abc123xyz")]
    [InlineData("cmd.exe --secret hidden_value")]
    [InlineData("tool.exe --api-key my-api-key-123")]
    [InlineData("script.exe -p mypassword")]
    public void Sanitize_CommandLineSecrets_Redacts(string input)
    {
        // Arrange
        var sut = BuildSut();

        // Act
        var result = sut.Sanitize(input);

        // Assert
        Assert.Contains("[REDACTED]", result);
        Assert.True(sut.LastSanitizeRedacted);
    }

    [Fact]
    public void Sanitize_BearerTokens_Redacts()
    {
        // Arrange
        var sut = BuildSut();
        var input = "Authorization: Bearer eyJhbGciOiJIUzI1NiJ9.eyJ0ZXN0IjoiZGF0YSJ9.abc123";

        // Act
        var result = sut.Sanitize(input);

        // Assert
        Assert.Contains("[REDACTED]", result);
        Assert.DoesNotContain("eyJhbGciOiJIUzI1NiJ9", result);
        Assert.True(sut.LastSanitizeRedacted);
    }

    [Fact]
    public void Sanitize_AwsKeys_Redacts()
    {
        // Arrange
        var sut = BuildSut();
        var input = "Detected key: AKIAIOSFODNN7EXAMPLE in environment";

        // Act
        var result = sut.Sanitize(input);

        // Assert
        Assert.Contains("[REDACTED]", result);
        Assert.DoesNotContain("AKIAIOSFODNN7EXAMPLE", result);
        Assert.True(sut.LastSanitizeRedacted);
    }

    [Theory]
    [InlineData("secret=my-super-secret-val")]
    [InlineData("token=abc123xyz")]
    [InlineData("apikey=key-1234-abcd")]
    public void Sanitize_GenericKeyValueSecrets_Redacts(string input)
    {
        // Arrange
        var sut = BuildSut();

        // Act
        var result = sut.Sanitize(input);

        // Assert
        Assert.Contains("[REDACTED]", result);
        Assert.True(sut.LastSanitizeRedacted);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Sanitize_NullOrEmpty_ReturnsInput(string input)
    {
        // Arrange
        var sut = BuildSut();

        // Act
        var result = sut.Sanitize(input);

        // Assert
        Assert.Equal(input, result);
        Assert.False(sut.LastSanitizeRedacted);
    }

    [Fact]
    public void LastSanitizeRedacted_AfterRedaction_ReturnsTrue()
    {
        // Arrange
        var sut = BuildSut();

        // Act
        sut.Sanitize("password=secret123");

        // Assert
        Assert.True(sut.LastSanitizeRedacted);
    }

    [Fact]
    public void LastSanitizeRedacted_AfterCleanInput_ReturnsFalse()
    {
        // Arrange
        var sut = BuildSut();

        // Act
        sut.Sanitize("Just a normal message");

        // Assert
        Assert.False(sut.LastSanitizeRedacted);
    }

    [Fact]
    public void LastSanitizeRedacted_ResetsOnSubsequentCall()
    {
        // Arrange
        var sut = BuildSut();

        // Act
        sut.Sanitize("password=secret123");
        Assert.True(sut.LastSanitizeRedacted);

        sut.Sanitize("Normal message");

        // Assert
        Assert.False(sut.LastSanitizeRedacted);
    }

    #region Test Helpers

    private static OutputSanitizer BuildSut()
    {
        return new OutputSanitizer();
    }

    #endregion
}