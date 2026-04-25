using McpOsDoctor.Core.Enums;
using McpOsDoctor.Core.Models;
using McpOsDoctor.Tools.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace McpOsDoctor.Tools.Tests;

/// <summary>
/// Unit tests for <see cref="GetServiceStatusTool"/>.
/// </summary>
public class GetServiceStatusToolTests
{
    [Fact]
    public async Task ExecuteAsync_WithServices_ReturnsServicesList()
    {
        // Arrange
        var inspector = new FakeServiceInspector
        {
            Services =
            [
                new ServiceInfo
                {
                    Name = "wuauserv",
                    DisplayName = "Windows Update",
                    Status = ServiceRunState.Running,
                    StartType = ServiceStartMode.Automatic,
                    Account = "LocalSystem",
                    Description = "Enables the detection, download, and installation of updates.",
                    ExecutablePath = @"C:\Windows\system32\svchost.exe -k netsvcs",
                    Dependencies = ["rpcss"],
                    Pid = 1234,
                    MemoryMb = 50.0
                },
                new ServiceInfo
                {
                    Name = "spooler",
                    DisplayName = "Print Spooler",
                    Status = ServiceRunState.Stopped,
                    StartType = ServiceStartMode.Manual,
                    Account = "LocalSystem",
                    Description = "Manages all local and network print queues.",
                    ExecutablePath = @"C:\Windows\System32\spoolsv.exe"
                }
            ]
        };

        // Act
        var json = await GetServiceStatusTool.ExecuteAsync(
            NullLogger<GetServiceStatusTool>.Instance,
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
    public async Task ExecuteAsync_SingleServiceByName_ReturnsSingleService()
    {
        // Arrange
        var inspector = new FakeServiceInspector
        {
            SingleService = new ServiceInfo
            {
                Name = "wuauserv",
                DisplayName = "Windows Update",
                Status = ServiceRunState.Running,
                StartType = ServiceStartMode.Automatic,
                Account = "LocalSystem",
                Description = "Enables the detection, download, and installation of updates.",
                ExecutablePath = @"C:\Windows\system32\svchost.exe -k netsvcs",
                Dependencies = ["rpcss", "eventlog"],
                Pid = 1234
            }
        };

        // Act
        var json = await GetServiceStatusTool.ExecuteAsync(
            NullLogger<GetServiceStatusTool>.Instance,
            inspector,
            new FakePrivilegeChecker(),
            new FakeOutputSanitizer(),
            serviceName: "wuauserv",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var doc = JObject.Parse(json);
        var data = doc["data"];
        Assert.Equal("wuauserv", (string)data["name"]);
        Assert.Equal("running", (string)data["status"]);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceNotFound_ReturnsError()
    {
        // Arrange
        var inspector = new FakeServiceInspector { SingleService = null };

        // Act
        var json = await GetServiceStatusTool.ExecuteAsync(
            NullLogger<GetServiceStatusTool>.Instance,
            inspector,
            new FakePrivilegeChecker(),
            new FakeOutputSanitizer(),
            serviceName: "nonexistent",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var doc = JObject.Parse(json);
        var code = (string)doc["code"];
        Assert.Equal("sourceNotFound", code);
    }

    [Fact]
    public async Task ExecuteAsync_SingleService_ReturnsEnrichedFields()
    {
        // Arrange
        var inspector = new FakeServiceInspector
        {
            SingleService = new ServiceInfo
            {
                Name = "wuauserv",
                DisplayName = "Windows Update",
                Status = ServiceRunState.Running,
                StartType = ServiceStartMode.Automatic,
                Account = "LocalSystem",
                Description = "Enables updates.",
                ExecutablePath = @"C:\Windows\system32\svchost.exe -k netsvcs",
                Dependencies = ["rpcss", "eventlog"]
            }
        };

        // Act
        var json = await GetServiceStatusTool.ExecuteAsync(
            NullLogger<GetServiceStatusTool>.Instance,
            inspector,
            new FakePrivilegeChecker(),
            new FakeOutputSanitizer(),
            serviceName: "wuauserv",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var doc = JObject.Parse(json);
        var data = doc["data"];
        Assert.Equal("Enables updates.", (string)data["description"]);
        Assert.Equal(@"C:\Windows\system32\svchost.exe -k netsvcs", (string)data["executablePath"]);
        var dependencies = (JArray)data["dependencies"];
        Assert.Equal(2, dependencies.Count);
        Assert.Equal("rpcss", (string)dependencies[0]);
        Assert.Equal("eventlog", (string)dependencies[1]);
    }

    [Fact]
    public async Task ExecuteAsync_NullEnrichedFields_OmitsFromJson()
    {
        // Arrange
        var inspector = new FakeServiceInspector
        {
            SingleService = new ServiceInfo
            {
                Name = "simple",
                DisplayName = "Simple Service",
                Status = ServiceRunState.Stopped,
                StartType = ServiceStartMode.Manual,
                Account = "LocalSystem"
            }
        };

        // Act
        var json = await GetServiceStatusTool.ExecuteAsync(
            NullLogger<GetServiceStatusTool>.Instance,
            inspector,
            new FakePrivilegeChecker(),
            new FakeOutputSanitizer(),
            serviceName: "simple",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var doc = JObject.Parse(json);
        var data = doc["data"];
        Assert.Null(data["description"]);
        Assert.Null(data["executablePath"]);
    }

    [Fact]
    public async Task ExecuteAsync_SensitiveExecutablePath_IsRedactedWithWarning()
    {
        // Arrange
        var inspector = new FakeServiceInspector
        {
            SingleService = new ServiceInfo
            {
                Name = "customsvc",
                DisplayName = "Custom Service",
                Status = ServiceRunState.Running,
                StartType = ServiceStartMode.Automatic,
                Account = "LocalSystem",
                ExecutablePath = @"C:\app\service.exe --token secret123"
            }
        };

        // Act
        var json = await GetServiceStatusTool.ExecuteAsync(
            NullLogger<GetServiceStatusTool>.Instance,
            inspector,
            new FakePrivilegeChecker(),
            new FakeOutputSanitizer { ShouldRedact = true },
            serviceName: "customsvc",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var doc = JObject.Parse(json);
        var data = doc["data"];
        Assert.Equal("[REDACTED]", (string)data["executablePath"]);

        var warnings = (JArray)doc["warnings"];
        Assert.Single(warnings);
        Assert.Contains("executable paths", ((string)warnings[0]).ToLowerInvariant());
    }

    [Fact]
    public async Task ExecuteAsync_MultipleServices_RedactionWarningAppearsOnce()
    {
        // Arrange
        var inspector = new FakeServiceInspector
        {
            Services =
            [
                new ServiceInfo
                {
                    Name = "svc1",
                    DisplayName = "Service 1",
                    Status = ServiceRunState.Running,
                    StartType = ServiceStartMode.Automatic,
                    Account = "LocalSystem",
                    ExecutablePath = @"C:\app\svc1.exe --password secret"
                },
                new ServiceInfo
                {
                    Name = "svc2",
                    DisplayName = "Service 2",
                    Status = ServiceRunState.Running,
                    StartType = ServiceStartMode.Automatic,
                    Account = "LocalSystem",
                    ExecutablePath = @"C:\app\svc2.exe --token abc123"
                }
            ]
        };

        // Act
        var json = await GetServiceStatusTool.ExecuteAsync(
            NullLogger<GetServiceStatusTool>.Instance,
            inspector,
            new FakePrivilegeChecker(),
            new FakeOutputSanitizer { ShouldRedact = true },
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var doc = JObject.Parse(json);
        var warnings = (JArray)doc["warnings"];
        Assert.Single(warnings);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyDependencies_SerializesAsEmptyArray()
    {
        // Arrange
        var inspector = new FakeServiceInspector
        {
            SingleService = new ServiceInfo
            {
                Name = "nodeps",
                DisplayName = "No Dependencies Service",
                Status = ServiceRunState.Running,
                StartType = ServiceStartMode.Automatic,
                Account = "LocalSystem",
                Dependencies = []
            }
        };

        // Act
        var json = await GetServiceStatusTool.ExecuteAsync(
            NullLogger<GetServiceStatusTool>.Instance,
            inspector,
            new FakePrivilegeChecker(),
            new FakeOutputSanitizer(),
            serviceName: "nodeps",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var doc = JObject.Parse(json);
        var data = doc["data"];
        var dependencies = (JArray)data["dependencies"];
        Assert.NotNull(dependencies);
        Assert.Empty(dependencies);
    }
}