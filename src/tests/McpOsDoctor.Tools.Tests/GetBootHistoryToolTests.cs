using McpOsDoctor.Core.Enums;
using McpOsDoctor.Core.Models;
using McpOsDoctor.Tools.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace McpOsDoctor.Tools.Tests;

/// <summary>
/// Unit tests for <see cref="GetBootHistoryTool"/>.
/// </summary>
public class GetBootHistoryToolTests
{
    [Fact]
    public async Task ExecuteAsync_WithEvents_ReturnsBootEvents()
    {
        // Arrange
        var provider = new FakeBootHistoryProvider
        {
            Events =
            [
                new BootEvent
                {
                    Timestamp = DateTimeOffset.UtcNow.AddHours(-12),
                    Type = BootType.Normal,
                    DurationSeconds = 45.2
                },
                new BootEvent
                {
                    Timestamp = DateTimeOffset.UtcNow.AddHours(-6),
                    Type = BootType.Shutdown
                }
            ]
        };

        // Act
        var json = await GetBootHistoryTool.ExecuteAsync(
            NullLogger<GetBootHistoryTool>.Instance,
            provider,
            new FakePrivilegeChecker(),
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var doc = JObject.Parse(json);
        var data = (JArray)doc["data"];
        Assert.Equal(2, data.Count);

        var firstEvent = data[0];
        Assert.Equal("normal", (string)firstEvent["type"]);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidTimeWindow_ReturnsError()
    {
        // Arrange
        var futureDate = DateTimeOffset.UtcNow.AddHours(2).ToString("o");

        // Act
        var json = await GetBootHistoryTool.ExecuteAsync(
            NullLogger<GetBootHistoryTool>.Instance,
            new FakeBootHistoryProvider(),
            new FakePrivilegeChecker(),
            to: futureDate,
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var doc = JObject.Parse(json);
        var code = (string)doc["code"];
        Assert.Equal("invalidParameter", code);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidFromFormat_ReturnsError()
    {
        // Act
        var json = await GetBootHistoryTool.ExecuteAsync(
            NullLogger<GetBootHistoryTool>.Instance,
            new FakeBootHistoryProvider(),
            new FakePrivilegeChecker(),
            from: "not-a-date",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var doc = JObject.Parse(json);
        var code = (string)doc["code"];
        Assert.Equal("invalidParameter", code);
        Assert.Contains("not a valid ISO 8601", (string)doc["message"]);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyEvents_ReturnsEmptyArray()
    {
        // Arrange
        var provider = new FakeBootHistoryProvider { Events = [] };

        // Act
        var json = await GetBootHistoryTool.ExecuteAsync(
            NullLogger<GetBootHistoryTool>.Instance,
            provider,
            new FakePrivilegeChecker(),
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var doc = JObject.Parse(json);
        var data = (JArray)doc["data"];
        Assert.Empty(data);
    }
}