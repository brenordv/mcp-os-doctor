using McpOsDoctor.Core.Models;
using McpOsDoctor.Tools.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace McpOsDoctor.Tools.Tests;

/// <summary>
/// Unit tests for <see cref="ListTopProcessesTool"/>.
/// </summary>
public class ListTopProcessesToolTests
{
    [Fact]
    public async Task ExecuteAsync_WithProcesses_ReturnsProcessList()
    {
        // Arrange
        var inspector = new FakeProcessInspector
        {
            Processes =
            [
                new ProcessInfo
                {
                    Pid = 100,
                    Name = "chrome",
                    CpuPercent = 25.5,
                    MemoryMb = 512.0,
                    CommandLine = "chrome.exe --flag"
                },
                new ProcessInfo
                {
                    Pid = 200,
                    Name = "code",
                    CpuPercent = 10.0,
                    MemoryMb = 300.0,
                    CommandLine = "code.exe"
                }
            ]
        };

        // Act
        var json = await ListTopProcessesTool.ExecuteAsync(
            NullLogger<ListTopProcessesTool>.Instance,
            inspector,
            new FakePrivilegeChecker(),
            new FakeOutputSanitizer(),
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var doc = JObject.Parse(json);
        var data = (JArray)doc["data"];
        Assert.Equal(2, data.Count);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidSortBy_ReturnsError()
    {
        // Act
        var json = await ListTopProcessesTool.ExecuteAsync(
            NullLogger<ListTopProcessesTool>.Instance,
            new FakeProcessInspector(),
            new FakePrivilegeChecker(),
            new FakeOutputSanitizer(),
            sortBy: "invalid",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var doc = JObject.Parse(json);
        var code = (string)doc["code"];
        Assert.Equal("invalidParameter", code);
        Assert.Contains("Invalid sortBy", (string)doc["message"]);
    }

    [Fact]
    public async Task ExecuteAsync_SanitizerRedactsCommandLines_AddsWarning()
    {
        // Arrange
        var inspector = new FakeProcessInspector
        {
            Processes =
            [
                new ProcessInfo
                {
                    Pid = 100,
                    Name = "app",
                    CpuPercent = 5.0,
                    MemoryMb = 100.0,
                    CommandLine = "app.exe --password secret123"
                }
            ]
        };
        var sanitizer = new FakeOutputSanitizer { ShouldRedact = true };

        // Act
        var json = await ListTopProcessesTool.ExecuteAsync(
            NullLogger<ListTopProcessesTool>.Instance,
            inspector,
            new FakePrivilegeChecker(),
            sanitizer,
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var doc = JObject.Parse(json);
        var warnings = (JArray)doc["warnings"];
        Assert.True(warnings.Count > 0);

        var firstProcess = doc["data"][0];
        Assert.Equal("[REDACTED]", (string)firstProcess["commandLine"]);
    }

    [Theory]
    [InlineData("cpu")]
    [InlineData("memory")]
    [InlineData("name")]
    public async Task ExecuteAsync_ValidSortBy_DoesNotReturnError(string sortBy)
    {
        // Act
        var json = await ListTopProcessesTool.ExecuteAsync(
            NullLogger<ListTopProcessesTool>.Instance,
            new FakeProcessInspector(),
            new FakePrivilegeChecker(),
            new FakeOutputSanitizer(),
            sortBy: sortBy,
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var doc = JObject.Parse(json);
        Assert.NotNull(doc["data"]);
    }
}