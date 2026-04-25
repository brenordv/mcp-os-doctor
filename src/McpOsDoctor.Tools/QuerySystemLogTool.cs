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
/// MCP tool that searches system event log entries with filtering and time-range support.
/// </summary>
[ToolCapabilityInfo("query_system_log", true, false, 1500,
    "Some sources (e.g., Security) require elevation to read.",
    [
        "source: string — event log source name (e.g., 'Application', 'System')",
        "severity: string — minimum severity: Verbose, Information, Warning, Error, Critical",
        "keywords: string — substring to search within log messages (max 500 chars)",
        "from: string — ISO 8601 start time (default: 24h ago)",
        "to: string — ISO 8601 end time (default: now)",
        "maxResults: int — max entries to return (default 100, max 500)"
    ]
)]
[McpServerToolType]
public class QuerySystemLogTool
{
    private const int TimeoutSeconds = 15;

    /// <summary>
    /// Searches system event log entries by time range, severity, source, and keyword filters.
    /// </summary>
    /// <param name="logger">Logger instance for diagnostics.</param>
    /// <param name="logReader">System log reader for querying entries.</param>
    /// <param name="privilegeChecker">Privilege checker for elevation status.</param>
    /// <param name="sanitizer">Output sanitizer for redacting sensitive data in log messages.</param>
    /// <param name="source">Event log source name (e.g., "Application", "System"). Optional.</param>
    /// <param name="severity">Minimum severity filter: Verbose, Information, Warning, Error, Critical. Optional.</param>
    /// <param name="keywords">Substring to search within log messages. Optional, max 500 chars.</param>
    /// <param name="from">ISO 8601 start time. Defaults to 24 hours ago. Optional.</param>
    /// <param name="to">ISO 8601 end time. Defaults to now. Optional.</param>
    /// <param name="maxResults">Maximum entries to return. Default 100, max 500. Optional.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A JSON string containing matching log entries.</returns>
    [McpServerTool(Name = "query_system_log"),
     Description(
         "Search system event log entries. Accepts time range (default: last 24h), severity filter (Verbose/Information/Warning/Error/Critical), source name, keyword search, and max results (default 100, max 500). Returns normalized entries with timestamp, severity, source, eventId, message. Some sources (e.g., Security) require elevation.")]
    public static async Task<string> ExecuteAsync(
        ILogger<QuerySystemLogTool> logger,
        ISystemLogReader logReader,
        IPrivilegeChecker privilegeChecker,
        IOutputSanitizer sanitizer,
        string source = null,
        string severity = null,
        string keywords = null,
        string from = null,
        string to = null,
        int? maxResults = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var sw = Stopwatch.StartNew();

            // Validate inputs
            FilterValidator.ValidateKeywords(keywords);

            var fromDate = Mapper.TryToDateTimeOffset(from, "from");
            var toDate = Mapper.TryToDateTimeOffset(to, "to");
            var parsedSeverity = Mapper.TryToLogSeverity(severity);

            // Check privilege for specific sources
            if (!string.IsNullOrWhiteSpace(source))
            {
                var capability = $"log:{source}";

                var access = privilegeChecker.CanAccess(capability);

                if (access == PrivilegeStatus.NeedsElevation)
                {
                    throw McpOsDoctorException.ElevationRequired(capability);
                }
            }

            var filter = new LogQueryFilter(maxResults, fromDate, toDate)
            {
                Severity = parsedSeverity,
                Source = source,
                Keywords = keywords
            };

            // Use a linked timeout token
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(TimeoutSeconds));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            var warnings = new List<string>();
            var entries = new List<LogEntry>();
            var redactionWarningAdded = false;

            try
            {
                await foreach (var entry in logReader.QueryAsync(filter, linkedCts.Token))
                {
                    var sanitizedEntry = entry with { Message = sanitizer.Sanitize(entry.Message) };

                    if (sanitizer.LastSanitizeRedacted && !redactionWarningAdded)
                    {
                        warnings.Add("One or more log messages had sensitive content redacted.");
                        redactionWarningAdded = true;
                    }

                    entries.Add(sanitizedEntry);
                }
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested &&
                                                     !cancellationToken.IsCancellationRequested)
            {
                warnings.Add($"Query timed out after {TimeoutSeconds}s. Returning {entries.Count} partial results.");
            }

            var response = ToolResponseBuilder.Build<IReadOnlyList<LogEntry>>(entries, sw, privilegeChecker, warnings);
            return JsonConvert.SerializeObject(response, JsonSettings.Default);
        }
        catch (McpOsDoctorException ex)
        {
            logger.LogWarning(ex, "Domain error in query_system_log");
            return JsonConvert.SerializeObject(ex.ToolError, JsonSettings.Default);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in query_system_log");
            var error = new ToolError
            {
                Code = ToolErrorCode.PlatformError,
                Message = "An unexpected error occurred while querying system logs.",
                IsRetryable = true
            };
            return JsonConvert.SerializeObject(error, JsonSettings.Default);
        }
    }
}