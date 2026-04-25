using System.ComponentModel;
using System.Diagnostics;
using McpOsDoctor.Core.Attributes;
using McpOsDoctor.Core.Enums;
using McpOsDoctor.Core.Errors;
using McpOsDoctor.Core.Interfaces;
using McpOsDoctor.Core.Models;
using McpOsDoctor.Core.Serialization;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Newtonsoft.Json;

namespace McpOsDoctor.Tools;

/// <summary>
/// MCP tool that provides DirectX diagnostic information via dxdiag.
/// </summary>
[ToolCapabilityInfo("get_directx_info", true, false, 15000)]
[McpServerToolType]
public class GetDirectXInfoTool
{
    /// <summary>
    /// Gets a snapshot of DirectX diagnostic information including DirectX version,
    /// display devices (GPU adapters with driver and feature details), and sound devices.
    /// </summary>
    /// <param name="logger">Logger instance for diagnostics.</param>
    /// <param name="directXInfoProvider">Provider for DirectX information.</param>
    /// <param name="privilegeChecker">Privilege checker for elevation status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A JSON string containing the DirectX diagnostic snapshot.</returns>
    [McpServerTool(Name = "get_directx_info"),
     Description("Get DirectX diagnostic information including DirectX version, display adapters (card name, manufacturer, VRAM, driver version, feature levels, WDDM model, HDR support), and sound devices. Uses dxdiag which is built into Windows.")]
    public static async Task<string> ExecuteAsync(
        ILogger<GetDirectXInfoTool> logger,
        IDirectXInfoProvider directXInfoProvider,
        IPrivilegeChecker privilegeChecker,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var sw = Stopwatch.StartNew();

            var info = await directXInfoProvider.GetDirectXInfoAsync(cancellationToken);

            var response = ToolResponseBuilder.Build<DirectXInfo>(info, sw, privilegeChecker);

            return JsonConvert.SerializeObject(response, JsonSettings.Default);
        }
        catch (McpOsDoctorException ex)
        {
            logger.LogWarning(ex, "Domain error in get_directx_info");
            return JsonConvert.SerializeObject(ex.ToolError, JsonSettings.Default);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in get_directx_info");
            var error = new ToolError
            {
                Code = ToolErrorCode.PlatformError,
                Message = "An unexpected error occurred while retrieving DirectX information.",
                IsRetryable = true
            };
            return JsonConvert.SerializeObject(error, JsonSettings.Default);
        }
    }
}