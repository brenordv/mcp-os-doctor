using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using McpOsDoctor.Core.Attributes;
using McpOsDoctor.Core.Constants;
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
/// MCP tool that reports available tools, current platform, elevation status, and parameter hints.
/// </summary>
[ToolCapabilityInfo("get_capabilities", true, false, 10)]
[McpServerToolType]
public class GetCapabilitiesTool
{
    /// <summary>
    /// Reports available diagnostic tools, platform information, and parameter hints.
    /// Call this first to understand what diagnostics are available.
    /// </summary>
    /// <param name="logger">Logger instance for diagnostics.</param>
    /// <param name="privilegeChecker">Privilege checker for elevation status.</param>
    /// <param name="serverInfo">Server info provider for version information.</param>
    /// <param name="gpuInfoProvider">GPU info provider for runtime availability check.</param>
    /// <param name="directXInfoProvider">DirectX info provider for runtime availability check.</param>
    /// <returns>A JSON string containing capability information.</returns>
    [McpServerTool(Name = "get_capabilities"),
     Description("Report available tools, current platform, elevation status, and parameter hints for each tool. Call this first to understand what diagnostics are available.")]
    public static string Execute(
        ILogger<GetCapabilitiesTool> logger,
        IPrivilegeChecker privilegeChecker,
        IServerInfoProvider serverInfo,
        IGpuInfoProvider gpuInfoProvider,
        IDirectXInfoProvider directXInfoProvider
        )
    {
        try
        {
            var sw = Stopwatch.StartNew();

            var capabilities = new CapabilitiesInfo
            {
                Platform = PlatformLabels.Windows,
                RuntimeVersion = RuntimeInformation.FrameworkDescription,
                ServerVersion = serverInfo.Version,
                IsElevated = privilegeChecker.IsElevated,
                Tools = BuildToolList(gpuInfoProvider, directXInfoProvider)
            };

            var response = ToolResponseBuilder.Build(capabilities, sw, privilegeChecker);
            return JsonConvert.SerializeObject(response, JsonSettings.Default);
        }
        catch (McpOsDoctorException ex)
        {
            logger.LogWarning(ex, "Domain error in get_capabilities");
            return JsonConvert.SerializeObject(ex.ToolError, JsonSettings.Default);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in get_capabilities");
            var error = new ToolError
            {
                Code = ToolErrorCode.PlatformError,
                Message = "An unexpected error occurred while retrieving capabilities.",
                IsRetryable = false
            };
            return JsonConvert.SerializeObject(error, JsonSettings.Default);
        }
    }

    private static IReadOnlyList<ToolCapabilityInfo> BuildToolList(
        IGpuInfoProvider gpuInfoProvider,
        IDirectXInfoProvider directXInfoProvider)
    {
        var tools = typeof(GetCapabilitiesTool).Assembly
            .GetTypes()
            .Select(t => t.GetCustomAttribute<ToolCapabilityInfoAttribute>())
            .Where(attr => attr is not null)
            .Select(attr => attr!.ToolCapabilityInfo)
            .ToList();

        MarkUnavailableIfNeeded(tools, "get_gpu_info", gpuInfoProvider.IsAvailable, gpuInfoProvider.UnavailableReason);
        MarkUnavailableIfNeeded(tools, "get_directx_info", directXInfoProvider.IsAvailable, directXInfoProvider.UnavailableReason);

        return tools;
    }

    private static void MarkUnavailableIfNeeded(
        List<ToolCapabilityInfo> tools,
        string toolName,
        bool isAvailable,
        string unavailableReason)
    {
        if (isAvailable)
        {
            return;
        }

        int index = tools.FindIndex(t => t.Name == toolName);
        if (index >= 0)
        {
            tools[index] = tools[index] with
            {
                Available = false,
                UnavailableReason = unavailableReason
            };
        }
    }
}