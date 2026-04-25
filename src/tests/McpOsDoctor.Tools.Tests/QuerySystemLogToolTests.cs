using McpOsDoctor.Core.Enums;
using McpOsDoctor.Core.Models;
using McpOsDoctor.Tools.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace McpOsDoctor.Tools.Tests;

/// <summary>
/// Unit tests for <see cref="QuerySystemLogTool"/>.
/// </summary>
public class QuerySystemLogToolTests
{
    [Fact]
    public async Task ExecuteAsync_WithEntries_ReturnsLogEntries()
    {
        // Arrange
        var logReader = new FakeSystemLogReader
        {
            Entries =
            [
                new LogEntry
                {
                    Timestamp = DateTimeOffset.UtcNow.AddMinutes(-10),
                    Severity = LogSeverity.Warning,
                    Source = "Application",
                    EventId = 1001,
                    Message = "Test warning message"
                },
                new LogEntry
                {
                    Timestamp = DateTimeOffset.UtcNow.AddMinutes(-5),
                    Severity = LogSeverity.Error,
                    Source = "Application",
                    EventId = 1002,
                    Message = "Test error message"
                }
            ]
        };

        // Act
        var json = await QuerySystemLogTool.ExecuteAsync(
            NullLogger<QuerySystemLogTool>.Instance,
            logReader,
            new FakePrivilegeChecker(),
            new FakeOutputSanitizer(),
            source: "Application",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var doc = JObject.Parse(json);
        var data = (JArray)doc["data"];
        Assert.Equal(2, data.Count);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidSource_ReturnsError()
    {
        // Arrange
        var invalidSource = "Source;DROP TABLE";

        // Act
        var json = await QuerySystemLogTool.ExecuteAsync(
            NullLogger<QuerySystemLogTool>.Instance,
            new FakeSystemLogReader(),
            new FakePrivilegeChecker(),
            new FakeOutputSanitizer(),
            source: invalidSource,
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var doc = JObject.Parse(json);
        var code = (string)doc["code"];
        Assert.Equal("invalidParameter", code);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidSeverity_ReturnsError()
    {
        // Act
        var json = await QuerySystemLogTool.ExecuteAsync(
            NullLogger<QuerySystemLogTool>.Instance,
            new FakeSystemLogReader(),
            new FakePrivilegeChecker(),
            new FakeOutputSanitizer(),
            severity: "NotALevel",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var doc = JObject.Parse(json);
        var code = (string)doc["code"];
        Assert.Equal("invalidParameter", code);
        Assert.Contains("Invalid severity", (string)doc["message"]);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidTimeWindow_ReturnsError()
    {
        // Arrange
        var futureDate = DateTimeOffset.UtcNow.AddHours(2).ToString("o");

        // Act
        var json = await QuerySystemLogTool.ExecuteAsync(
            NullLogger<QuerySystemLogTool>.Instance,
            new FakeSystemLogReader(),
            new FakePrivilegeChecker(),
            new FakeOutputSanitizer(),
            to: futureDate,
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var doc = JObject.Parse(json);
        var code = (string)doc["code"];
        Assert.Equal("invalidParameter", code);
    }

    [Fact]
    public async Task ExecuteAsync_WithSanitizer_SanitizesMessages()
    {
        // Arrange
        var logReader = new FakeSystemLogReader
        {
            Entries =
            [
                new LogEntry
                {
                    Timestamp = DateTimeOffset.UtcNow.AddMinutes(-5),
                    Severity = LogSeverity.Information,
                    Source = "Application",
                    Message = "Connection string: password=secret123"
                }
            ]
        };
        var sanitizer = new FakeOutputSanitizer { ShouldRedact = true };

        // Act
        var json = await QuerySystemLogTool.ExecuteAsync(
            NullLogger<QuerySystemLogTool>.Instance,
            logReader,
            new FakePrivilegeChecker(),
            sanitizer,
            source: "Application",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var doc = JObject.Parse(json);
        var warnings = (JArray)doc["warnings"];
        Assert.True(warnings.Count > 0);

        var firstEntry = doc["data"][0];
        var message = (string)firstEntry["message"];
        Assert.Equal("[REDACTED]", message);
    }

    [Fact]
    public async Task ExecuteAsync_SecurityLogNeedsElevation_ReturnsError()
    {
        // Arrange
        var privilegeChecker = new FakePrivilegeChecker();
        privilegeChecker.SetCanAccess("log:Security", PrivilegeStatus.NeedsElevation);

        // Act
        var json = await QuerySystemLogTool.ExecuteAsync(
            NullLogger<QuerySystemLogTool>.Instance,
            new FakeSystemLogReader(),
            privilegeChecker,
            new FakeOutputSanitizer(),
            source: "Security",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var doc = JObject.Parse(json);
        var code = (string)doc["code"];
        Assert.Equal("elevationRequired", code);
    }

    [Fact]
    public async Task ExecuteAsync_ElevationRequired_ReturnsElevationError()
    {
        // Arrange
        var privilegeChecker = new FakePrivilegeChecker();
        privilegeChecker.SetCanAccess("log:Security", PrivilegeStatus.NeedsElevation);

        // Act
        var json = await QuerySystemLogTool.ExecuteAsync(
            NullLogger<QuerySystemLogTool>.Instance,
            new FakeSystemLogReader(),
            privilegeChecker,
            new FakeOutputSanitizer(),
            source: "Security",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var doc = JObject.Parse(json);
        Assert.Contains("requires administrator privileges",
            (string)doc["message"]);
    }

    [Fact]
    public async Task ExecuteAsync_ValidSeverity_AcceptsCaseInsensitive()
    {
        // Arrange
        var logReader = new FakeSystemLogReader
        {
            Entries =
            [
                new LogEntry
                {
                    Timestamp = DateTimeOffset.UtcNow.AddMinutes(-5),
                    Severity = LogSeverity.Error,
                    Source = "System",
                    Message = "Test"
                }
            ]
        };

        // Act
        var json = await QuerySystemLogTool.ExecuteAsync(
            NullLogger<QuerySystemLogTool>.Instance,
            logReader,
            new FakePrivilegeChecker(),
            new FakeOutputSanitizer(),
            severity: "error",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var doc = JObject.Parse(json);
        Assert.NotNull(doc["data"]);
    }
}