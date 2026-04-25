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
/// MCP tool that lists available event log sources on the system.
/// </summary>
///
[ToolCapabilityInfo("list_log_sources", true, false, 500, "Elevated access reveals additional log sources.")]
[McpServerToolType]
public class ListLogSourcesTool
{
    /// <summary>
    /// Lists available event log sources, including whether they require elevation and approximate record counts.
    /// </summary>
    /// <param name="logger">Logger instance for diagnostics.</param>
    /// <param name="logReader">System log reader for listing sources.</param>
    /// <param name="privilegeChecker">Privilege checker for elevation status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A JSON string containing available log sources.</returns>
    [McpServerTool(Name = "list_log_sources"),
     Description("List available event log sources on this system. Returns source names, whether they require elevation, and approximate record counts.")]
    public static async Task<string> ExecuteAsync(
        ILogger<ListLogSourcesTool> logger,
        ISystemLogReader logReader,
        IPrivilegeChecker privilegeChecker,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var sw = Stopwatch.StartNew();

            var sources = await logReader.ListSourcesAsync(cancellationToken);

            var response = ToolResponseBuilder.Build(sources, sw, privilegeChecker);

            return JsonConvert.SerializeObject(response, JsonSettings.Default);
        }
        catch (McpOsDoctorException ex)
        {
            logger.LogWarning(ex, "Domain error in list_log_sources");
            return JsonConvert.SerializeObject(ex.ToolError, JsonSettings.Default);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in list_log_sources");
            var error = new ToolError
            {
                Code = ToolErrorCode.PlatformError,
                Message = "An unexpected error occurred while listing log sources.",
                IsRetryable = true
            };
            return JsonConvert.SerializeObject(error, JsonSettings.Default);
        }
    }
}