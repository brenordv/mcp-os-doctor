using System.Runtime.InteropServices;
using McpOsDoctor.Core.Interfaces;
using McpOsDoctor.Platform.Windows.Extensions;
using McpOsDoctor.Server;
using McpOsDoctor.Server.Extensions;
using McpOsDoctor.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    Console.Error.WriteLine($"MCP OS Doctor v1 only supports Windows. Detected platform: {RuntimeInformation.OSDescription}");
    return 1;
}

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddMcpOsDoctorFileLogger();

builder.Services.AddSingleton<IServerInfoProvider, ServerInfoProvider>();
builder.Services.AddWindowsPlatform();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<GetCapabilitiesTool>()
    .WithTools<QuerySystemLogTool>()
    .WithTools<ListLogSourcesTool>()
    .WithTools<GetServiceStatusTool>()
    .WithTools<ListTopProcessesTool>()
    .WithTools<GetSystemInfoTool>()
    .WithTools<GetBootHistoryTool>()
    .WithTools<GetGpuInfoTool>()
    .WithTools<GetDirectXInfoTool>()
    .WithTools<StartSensorMonitoringTool>()
    .WithTools<StopSensorMonitoringTool>()
    .WithTools<GetSensorDataTool>();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation(
    "MCP OS Doctor starting — Platform: {Platform}, .NET: {RuntimeVersion}, PID: {ProcessId}",
    "windows",
    RuntimeInformation.FrameworkDescription,
    Environment.ProcessId);

await app.RunAsync();

return 0;