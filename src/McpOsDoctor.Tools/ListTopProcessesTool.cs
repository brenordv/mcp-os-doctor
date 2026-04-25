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
/// MCP tool that lists top processes by CPU or memory usage.
/// </summary>
[ToolCapabilityInfo("list_top_processes", true, false, 1500,
    "Elevation provides command-line info for all processes.",
    [
        "sortBy: string — sort field: cpu, memory, or name (default: cpu)",
        "minCpuPercent: double — exclude processes below this CPU % threshold",
        "minMemoryMB: double — exclude processes below this memory (MB) threshold",
        "maxResults: int — max processes to return (default 25, max 100)"
    ]
)]
[McpServerToolType]
public class ListTopProcessesTool
{
    /// <summary>
    /// Lists top processes sorted by CPU or memory usage with optional threshold filters.
    /// </summary>
    /// <param name="logger">Logger instance for diagnostics.</param>
    /// <param name="processInspector">Process inspector for querying processes.</param>
    /// <param name="privilegeChecker">Privilege checker for elevation status.</param>
    /// <param name="sanitizer">Output sanitizer for redacting sensitive data in command lines.</param>
    /// <param name="sortBy">Sort field: "cpu", "memory", or "name". Defaults to "cpu". Optional.</param>
    /// <param name="minCpuPercent">Exclude processes below this CPU percentage threshold. Optional.</param>
    /// <param name="minMemoryMb">Exclude processes below this memory (MB) threshold. Optional.</param>
    /// <param name="maxResults">Max processes to return. Default 25, max 100. Optional.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A JSON string containing top process information.</returns>
    [McpServerTool(Name = "list_top_processes"),
     Description(
         "List top processes by CPU or memory usage. Supports sorting by cpu/memory/name, minimum thresholds, and result limits. Default: top 25 by CPU, max 100.")]
    public static async Task<string> ExecuteAsync(
        ILogger<ListTopProcessesTool> logger,
        IProcessInspector processInspector,
        IPrivilegeChecker privilegeChecker,
        IOutputSanitizer sanitizer,
        string sortBy = null,
        double? minCpuPercent = null,
        double? minMemoryMb = null,
        int? maxResults = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var sw = Stopwatch.StartNew();

            var parsedSortBy = Mapper.TryToProcessSortField(sortBy);

            if (minCpuPercent is < 0 or > 100)
            {
                throw McpOsDoctorException.InvalidParameter(
                    "minCpuPercent must be between 0 and 100.",
                    "Use a value between 0 and 100.");
            }

            if (minMemoryMb is < 0)
            {
                throw McpOsDoctorException.InvalidParameter(
                    "minMemoryMB must be a non-negative value.",
                    "Use a value >= 0.");
            }

            var filter = new ProcessFilter(maxResults, parsedSortBy)
            {
                MinCpuPercent = minCpuPercent,
                MinMemoryMb = minMemoryMb
            };

            var warnings = new List<string>();
            var processes = new List<ProcessInfo>();
            var redactionWarningAdded = false;

            await foreach (var process in processInspector.GetTopProcessesAsync(filter, cancellationToken))
            {
                var sanitizedProcess = process;
                if (!string.IsNullOrEmpty(process.CommandLine))
                {
                    var sanitizedCommandLine = sanitizer.Sanitize(process.CommandLine);
                    if (sanitizer.LastSanitizeRedacted && !redactionWarningAdded)
                    {
                        warnings.Add("One or more process command lines had sensitive content redacted.");
                        redactionWarningAdded = true;
                    }

                    sanitizedProcess = process with { CommandLine = sanitizedCommandLine };
                }

                processes.Add(sanitizedProcess);
            }

            var response =
                ToolResponseBuilder.Build<IReadOnlyList<ProcessInfo>>(processes, sw, privilegeChecker, warnings);

            return JsonConvert.SerializeObject(response, JsonSettings.Default);
        }
        catch (McpOsDoctorException ex)
        {
            logger.LogWarning(ex, "Domain error in list_top_processes");
            return JsonConvert.SerializeObject(ex.ToolError, JsonSettings.Default);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in list_top_processes");
            var error = new ToolError
            {
                Code = ToolErrorCode.PlatformError,
                Message = "An unexpected error occurred while listing processes.",
                IsRetryable = true
            };
            return JsonConvert.SerializeObject(error, JsonSettings.Default);
        }
    }
}