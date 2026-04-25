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
/// MCP tool that starts background hardware sensor monitoring.
/// </summary>
[ToolCapabilityInfo("start_sensor_monitoring", true, false, 2000,
    elevationNote: "Elevated privileges enable reading all hardware sensors including CPU temperature and voltage.",
    parameterHints: ["intervalSeconds: int — polling interval in seconds (default 5, range 1-3600)"])]
[McpServerToolType]
public class StartSensorMonitoringTool
{
    /// <summary>
    /// Starts background hardware sensor monitoring at the specified polling interval.
    /// Collects temperature, fan speed, voltage, clock, load, and power data from
    /// CPU, GPU, motherboard, memory, and storage. Call get_sensor_data to retrieve results.
    /// </summary>
    /// <param name="logger">Logger instance for diagnostics.</param>
    /// <param name="sensorMonitor">Sensor monitor service.</param>
    /// <param name="privilegeChecker">Privilege checker for elevation status.</param>
    /// <param name="intervalSeconds">Seconds between each sensor poll (default 5, range 1–3600).</param>
    /// <returns>A JSON string containing the monitoring status.</returns>
    [McpServerTool(Name = "start_sensor_monitoring"),
     Description("Start background hardware sensor monitoring. Collects temperature, fan speed, voltage, clock, load, and power data from CPU, GPU, motherboard, memory, and storage at a configurable interval. Data is persisted to disk and survives crashes. Call get_sensor_data to retrieve aggregated results.")]
    public static string Execute(
        ILogger<StartSensorMonitoringTool> logger,
        ISensorMonitor sensorMonitor,
        IPrivilegeChecker privilegeChecker,
        int? intervalSeconds = null)
    {
        try
        {
            var sw = Stopwatch.StartNew();

            sensorMonitor.Start(intervalSeconds ?? 5);

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
            logger.LogWarning(ex, "Domain error in start_sensor_monitoring");
            return JsonConvert.SerializeObject(ex.ToolError, JsonSettings.Default);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in start_sensor_monitoring");
            var error = new ToolError
            {
                Code = ToolErrorCode.PlatformError,
                Message = "An unexpected error occurred while starting sensor monitoring.",
                IsRetryable = true
            };
            return JsonConvert.SerializeObject(error, JsonSettings.Default);
        }
    }
}