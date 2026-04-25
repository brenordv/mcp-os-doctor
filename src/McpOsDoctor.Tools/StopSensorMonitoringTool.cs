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
/// MCP tool that stops background hardware sensor monitoring.
/// </summary>
[ToolCapabilityInfo("stop_sensor_monitoring", true, false, 500)]
[McpServerToolType]
public class StopSensorMonitoringTool
{
    /// <summary>
    /// Stops the background sensor monitoring loop and releases hardware resources.
    /// Collected data remains available for retrieval via get_sensor_data.
    /// </summary>
    /// <param name="logger">Logger instance for diagnostics.</param>
    /// <param name="sensorMonitor">Sensor monitor service.</param>
    /// <param name="privilegeChecker">Privilege checker for elevation status.</param>
    /// <returns>A JSON string containing the final monitoring status.</returns>
    [McpServerTool(Name = "stop_sensor_monitoring"),
     Description("Stop background hardware sensor monitoring. Collected data remains available via get_sensor_data.")]
    public static string Execute(
        ILogger<StopSensorMonitoringTool> logger,
        ISensorMonitor sensorMonitor,
        IPrivilegeChecker privilegeChecker)
    {
        try
        {
            var sw = Stopwatch.StartNew();

            sensorMonitor.Stop();

            var status = new SensorMonitoringSummary
            {
                State = sensorMonitor.State,
                StartedAt = sensorMonitor.StartedAt,
                SnapshotCount = sensorMonitor.SnapshotCount,
                IntervalSeconds = sensorMonitor.IntervalSeconds,
                Hardware = []
            };

            var response = ToolResponseBuilder.Build(status, sw, privilegeChecker);
            return JsonConvert.SerializeObject(response, JsonSettings.Default);
        }
        catch (McpOsDoctorException ex)
        {
            logger.LogWarning(ex, "Domain error in stop_sensor_monitoring");
            return JsonConvert.SerializeObject(ex.ToolError, JsonSettings.Default);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in stop_sensor_monitoring");
            var error = new ToolError
            {
                Code = ToolErrorCode.PlatformError,
                Message = "An unexpected error occurred while stopping sensor monitoring.",
                IsRetryable = false
            };
            return JsonConvert.SerializeObject(error, JsonSettings.Default);
        }
    }
}