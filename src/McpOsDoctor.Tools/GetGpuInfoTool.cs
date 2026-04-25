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
/// MCP tool that provides NVIDIA GPU information via nvidia-smi.
/// </summary>
[ToolCapabilityInfo("get_gpu_info", true, false, 2000)]
[McpServerToolType]
public class GetGpuInfoTool
{
    /// <summary>
    /// Gets a snapshot of NVIDIA GPU information including name, driver version,
    /// memory usage, temperature, utilization, power draw, and performance state.
    /// </summary>
    /// <param name="logger">Logger instance for diagnostics.</param>
    /// <param name="gpuInfoProvider">Provider for GPU information.</param>
    /// <param name="privilegeChecker">Privilege checker for elevation status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A JSON string containing the GPU information snapshot.</returns>
    [McpServerTool(Name = "get_gpu_info"),
     Description("Get NVIDIA GPU information including model name, driver version, VRAM usage, temperature, utilization, power draw, and performance state. Requires nvidia-smi (included with NVIDIA GPU drivers).")]
    public static async Task<string> ExecuteAsync(
        ILogger<GetGpuInfoTool> logger,
        IGpuInfoProvider gpuInfoProvider,
        IPrivilegeChecker privilegeChecker,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var sw = Stopwatch.StartNew();

            var devices = await gpuInfoProvider.GetGpuInfoAsync(cancellationToken);

            var response = ToolResponseBuilder.Build<IReadOnlyList<GpuInfo>>(devices, sw, privilegeChecker);

            return JsonConvert.SerializeObject(response, JsonSettings.Default);
        }
        catch (McpOsDoctorException ex)
        {
            logger.LogWarning(ex, "Domain error in get_gpu_info");
            return JsonConvert.SerializeObject(ex.ToolError, JsonSettings.Default);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in get_gpu_info");
            var error = new ToolError
            {
                Code = ToolErrorCode.PlatformError,
                Message = "An unexpected error occurred while retrieving GPU information.",
                IsRetryable = true
            };
            return JsonConvert.SerializeObject(error, JsonSettings.Default);
        }
    }
}