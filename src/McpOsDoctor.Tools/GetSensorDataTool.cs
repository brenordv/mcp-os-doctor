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
/// MCP tool that retrieves aggregated sensor monitoring results with min/max/average statistics.
/// </summary>
[ToolCapabilityInfo("get_sensor_data", true, false, 500,
    parameterHints:
    [
        "hardwareType: string — filter by hardware: cpu, gpu, motherboard, memory, storage, network, cooler, battery, psu, embeddedController",
        "sensorType: string — filter by sensor: temperature, fan, voltage, clock, load, power, data, throughput, current, level, control"
    ])]
[McpServerToolType]
public class GetSensorDataTool
{
    /// <summary>
    /// Retrieves hardware sensor monitoring results with min/max/average/current statistics per sensor.
    /// Reads persisted data, so results are available even after a crash recovery.
    /// Requires start_sensor_monitoring to have been called at least once.
    /// </summary>
    /// <param name="logger">Logger instance for diagnostics.</param>
    /// <param name="sensorMonitor">Sensor monitor service.</param>
    /// <param name="privilegeChecker">Privilege checker for elevation status.</param>
    /// <param name="hardwareType">Optional filter by hardware category (e.g., "cpu", "gpu").</param>
    /// <param name="sensorType">Optional filter by sensor category (e.g., "temperature", "fan").</param>
    /// <returns>A JSON string containing the sensor monitoring summary.</returns>
    [McpServerTool(Name = "get_sensor_data"),
     Description("Get hardware sensor monitoring results with min/max/average/current statistics per sensor. Supports filtering by hardware type and sensor type. Data is read from disk, so results survive server crashes. Requires start_sensor_monitoring to have been called at least once.")]
    public static string Execute(
        ILogger<GetSensorDataTool> logger,
        ISensorMonitor sensorMonitor,
        IPrivilegeChecker privilegeChecker,
        string hardwareType = null,
        string sensorType = null)
    {
        try
        {
            var sw = Stopwatch.StartNew();

            var summary = sensorMonitor.GetResults(hardwareType, sensorType);

            var response = ToolResponseBuilder.Build(summary, sw, privilegeChecker);
            return JsonConvert.SerializeObject(response, JsonSettings.Default);
        }
        catch (McpOsDoctorException ex)
        {
            logger.LogWarning(ex, "Domain error in get_sensor_data");
            return JsonConvert.SerializeObject(ex.ToolError, JsonSettings.Default);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in get_sensor_data");
            var error = new ToolError
            {
                Code = ToolErrorCode.PlatformError,
                Message = "An unexpected error occurred while retrieving sensor data.",
                IsRetryable = true
            };
            return JsonConvert.SerializeObject(error, JsonSettings.Default);
        }
    }
}