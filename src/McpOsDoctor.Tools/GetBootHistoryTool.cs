using System.ComponentModel;
using System.Diagnostics;
using McpOsDoctor.Core.Attributes;
using McpOsDoctor.Core.Enums;
using McpOsDoctor.Core.Errors;
using McpOsDoctor.Core.Filters;
using McpOsDoctor.Core.Interfaces;
using McpOsDoctor.Core.Mappers;
using McpOsDoctor.Core.Models;
using McpOsDoctor.Core.Serialization;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Newtonsoft.Json;

namespace McpOsDoctor.Tools;

/// <summary>
/// MCP tool that retrieves system boot, shutdown, and crash event history.
/// </summary>
[ToolCapabilityInfo("get_boot_history", true, false, 1000,
    "Elevation may reveal additional power state events.",
    [
        "from: string — ISO 8601 start time (default: 24h ago)",
        "to: string — ISO 8601 end time (default: now)",
        "maxResults: int — max events to return (default 100, max 500)"
    ]
)]
[McpServerToolType]
public class GetBootHistoryTool
{
    /// <summary>
    /// Gets system boot, shutdown, and crash events within a time range.
    /// Returns timestamps, event types, and durations.
    /// </summary>
    /// <param name="logger">Logger instance for diagnostics.</param>
    /// <param name="bootHistoryProvider">Provider for boot history events.</param>
    /// <param name="privilegeChecker">Privilege checker for elevation status.</param>
    /// <param name="from">ISO 8601 start time. Defaulted to 24 hours ago. Optional.</param>
    /// <param name="to">ISO 8601 end time. Defaults to now. Optional.</param>
    /// <param name="maxResults">Max events to return. Default 100, max 500. Optional.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A JSON string containing boot history events.</returns>
    [McpServerTool(Name = "get_boot_history"),
     Description(
         "Get system boot, shutdown, and crash events. Returns timestamps, event types (Normal/Shutdown/UnexpectedShutdown/BlueScreen/Sleep/Wake), and durations. Default: last 24 hours, max 100 results.")]
    public static async Task<string> ExecuteAsync(
        ILogger<GetBootHistoryTool> logger,
        IBootHistoryProvider bootHistoryProvider,
        IPrivilegeChecker privilegeChecker,
        string from = null,
        string to = null,
        int? maxResults = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var sw = Stopwatch.StartNew();

            var fromDate = Mapper.TryToDateTimeOffset(from, "from");
            var toDate = Mapper.TryToDateTimeOffset(to, "to");

            var filter = new TimeWindowFilter(maxResults, fromDate, toDate);

            var events = new List<BootEvent>();

            await foreach (var bootEvent in bootHistoryProvider.GetBootEventsAsync(filter, cancellationToken))
            {
                events.Add(bootEvent);
            }

            var response = ToolResponseBuilder.Build<IReadOnlyList<BootEvent>>(events, sw, privilegeChecker);
            return JsonConvert.SerializeObject(response, JsonSettings.Default);
        }
        catch (McpOsDoctorException ex)
        {
            logger.LogWarning(ex, "Domain error in get_boot_history");
            return JsonConvert.SerializeObject(ex.ToolError, JsonSettings.Default);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in get_boot_history");
            var error = new ToolError
            {
                Code = ToolErrorCode.PlatformError,
                Message = "An unexpected error occurred while retrieving boot history.",
                IsRetryable = true
            };
            return JsonConvert.SerializeObject(error, JsonSettings.Default);
        }
    }
}