using McpOsDoctor.Core.Models;
using McpOsDoctor.Tools.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace McpOsDoctor.Tools.Tests;

/// <summary>
/// Unit tests for <see cref="ListLogSourcesTool"/>.
/// </summary>
public class ListLogSourcesToolTests
{
    [Fact]
    public async Task ExecuteAsync_WithSources_ReturnsSourceList()
    {
        // Arrange
        var logReader = new FakeSystemLogReader
        {
            Sources =
            [
                new LogSourceInfo { Name = "Application", RequiresElevation = false, RecordCount = 1000 },
                new LogSourceInfo { Name = "System", RequiresElevation = false, RecordCount = 500 },
                new LogSourceInfo { Name = "Security", RequiresElevation = true, RecordCount = 200 }
            ]
        };

        // Act
        var json = await ListLogSourcesTool.ExecuteAsync(
            NullLogger<ListLogSourcesTool>.Instance,
            logReader,
            new FakePrivilegeChecker(),
            TestContext.Current.CancellationToken);

        // Assert
        var doc = JObject.Parse(json);
        var data = (JArray)doc["data"];
        Assert.Equal(3, data.Count);

        var firstSource = data[0];
        Assert.Equal("Application", (string)firstSource["name"]);
        Assert.False((bool)firstSource["requiresElevation"]);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyList_ReturnsEmptyArray()
    {
        // Arrange
        var logReader = new FakeSystemLogReader { Sources = [] };

        // Act
        var json = await ListLogSourcesTool.ExecuteAsync(
            NullLogger<ListLogSourcesTool>.Instance,
            logReader,
            new FakePrivilegeChecker(),
            TestContext.Current.CancellationToken);

        // Assert
        var doc = JObject.Parse(json);
        var data = (JArray)doc["data"];
        Assert.Empty(data);
    }
}