using System.ComponentModel;
using System.Diagnostics;
using McpOsDoctor.Core.Attributes;
using McpOsDoctor.Core.Enums;
using McpOsDoctor.Core.Errors;
using McpOsDoctor.Core.Interfaces;
using McpOsDoctor.Core.Serialization;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Newtonsoft.Json;

namespace McpOsDoctor.Tools;

/// <summary>
/// MCP tool that provides a snapshot of system hardware and OS information.
/// </summary>
[ToolCapabilityInfo("get_system_info", true, false, 500)]
[McpServerToolType]
public class GetSystemInfoTool
{
    /// <summary>
    /// Gets a snapshot of system hardware and OS information including hostname, OS version,
    /// CPU, memory, uptime, and disk volumes.
    /// </summary>
    /// <param name="logger">Logger instance for diagnostics.</param>
    /// <param name="systemInfoProvider">Provider for system information.</param>
    /// <param name="privilegeChecker">Privilege checker for elevation status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A JSON string containing the system information snapshot.</returns>
    [McpServerTool(Name = "get_system_info"),
     Description("Get a snapshot of system hardware and OS information including hostname, OS version, CPU, memory, uptime, and disk volumes.")]
    public static async Task<string> ExecuteAsync(
        ILogger<GetSystemInfoTool> logger,
        ISystemInfoProvider systemInfoProvider,
        IPrivilegeChecker privilegeChecker,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var sw = Stopwatch.StartNew();

            var snapshot = await systemInfoProvider.GetSystemInfoAsync(cancellationToken);

            var response = ToolResponseBuilder.Build(snapshot, sw, privilegeChecker);

            return JsonConvert.SerializeObject(response, JsonSettings.Default);
        }
        catch (McpOsDoctorException ex)
        {
            logger.LogWarning(ex, "Domain error in get_system_info");
            return JsonConvert.SerializeObject(ex.ToolError, JsonSettings.Default);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in get_system_info");
            var error = new ToolError
            {
                Code = ToolErrorCode.PlatformError,
                Message = "An unexpected error occurred while retrieving system information.",
                IsRetryable = true
            };
            return JsonConvert.SerializeObject(error, JsonSettings.Default);
        }
    }
}