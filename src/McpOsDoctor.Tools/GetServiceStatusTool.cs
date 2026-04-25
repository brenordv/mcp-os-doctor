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
/// MCP tool that queries Windows services by name pattern and status filter.
/// </summary>
[ToolCapabilityInfo("get_service_status", true, false, 1000,
    parameterHints: [
        "serviceName: string — exact service name for single lookup",
        "namePattern: string — prefix pattern to match service names",
        "status: string — filter by state: Running, Stopped, Paused, StartPending, StopPending",
        "maxResults: int — max services to return (default 100, max 500)"
    ]
)]
[McpServerToolType]
public class GetServiceStatusTool
{
    /// <summary>
    /// Queries Windows services by name pattern and status filter, or retrieves a single service by exact name.
    /// </summary>
    /// <param name="logger">Logger instance for diagnostics.</param>
    /// <param name="serviceInspector">Service inspector for querying services.</param>
    /// <param name="privilegeChecker">Privilege checker for elevation status.</param>
    /// <param name="sanitizer">Output sanitizer for redacting sensitive data in executable paths.</param>
    /// <param name="serviceName">Exact service name for a single lookup. Optional.</param>
    /// <param name="namePattern">Prefix pattern to match service names. Optional.</param>
    /// <param name="status">Filter by state: Running, Stopped, Paused, StartPending, StopPending. Optional.</param>
    /// <param name="maxResults">Max services to return. Default 100, max 500. Optional.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A JSON string containing matching service information.</returns>
    [McpServerTool(Name = "get_service_status"),
     Description("Query Windows services by name pattern and status filter. Returns service name, display name, description, status, start type, account, executable path, dependencies, PID, and memory usage. Default max 100 results, max 500.")]
    public static async Task<string> ExecuteAsync(
        ILogger<GetServiceStatusTool> logger,
        IServiceInspector serviceInspector,
        IPrivilegeChecker privilegeChecker,
        IOutputSanitizer sanitizer,
        string serviceName = null,
        string namePattern = null,
        string status = null,
        int? maxResults = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var sw = Stopwatch.StartNew();
            var warnings = new List<string>();

            // Single service lookup by exact name
            if (!string.IsNullOrWhiteSpace(serviceName))
            {
                var singleService = (await serviceInspector.GetServiceAsync(serviceName, cancellationToken))
                    ?? throw McpOsDoctorException.SourceNotFound(serviceName);

                singleService = SanitizeExecutablePath(singleService, sanitizer, warnings);

                var singleResponse = ToolResponseBuilder.Build(singleService, sw, privilegeChecker, warnings);

                return JsonConvert.SerializeObject(singleResponse, JsonSettings.Default);
            }

            // Multi-service query
            var parsedStatus = Mapper.TryToServiceRunState(status);

            var filter = new ServiceFilter(maxResults)
            {
                NamePattern = namePattern,
                Status = parsedStatus
            };

            var services = new List<ServiceInfo>();

            await foreach (var service in serviceInspector.GetServicesAsync(filter, cancellationToken))
            {
                services.Add(SanitizeExecutablePath(service, sanitizer, warnings));
            }

            var response = ToolResponseBuilder.Build<IReadOnlyList<ServiceInfo>>(
                services, sw, privilegeChecker, warnings);

            return JsonConvert.SerializeObject(response, JsonSettings.Default);
        }
        catch (McpOsDoctorException ex)
        {
            logger.LogWarning(ex, "Domain error in get_service_status");
            return JsonConvert.SerializeObject(ex.ToolError, JsonSettings.Default);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in get_service_status");
            var error = new ToolError
            {
                Code = ToolErrorCode.PlatformError,
                Message = "An unexpected error occurred while querying services.",
                IsRetryable = true
            };
            return JsonConvert.SerializeObject(error, JsonSettings.Default);
        }
    }

    /// <summary>
    /// Sanitizes the executable path of a service to redact sensitive content.
    /// </summary>
    /// <param name="service">The service whose executable path should be sanitized.</param>
    /// <param name="sanitizer">Output sanitizer instance.</param>
    /// <param name="warnings">Warnings list to append a redaction notice to.</param>
    /// <returns>The service with a sanitized executable path.</returns>
    private static ServiceInfo SanitizeExecutablePath(
        ServiceInfo service,
        IOutputSanitizer sanitizer,
        List<string> warnings)
    {
        if (string.IsNullOrEmpty(service.ExecutablePath))
        {
            return service;
        }

        var sanitizedPath = sanitizer.Sanitize(service.ExecutablePath);
        if (sanitizer.LastSanitizeRedacted && !warnings.Contains(ExecutablePathRedactionWarning))
        {
            warnings.Add(ExecutablePathRedactionWarning);
        }

        return service with { ExecutablePath = sanitizedPath };
    }

    private const string ExecutablePathRedactionWarning =
        "One or more service executable paths had sensitive content redacted.";
}