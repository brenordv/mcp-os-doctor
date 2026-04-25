using System.Management;
using System.Runtime.CompilerServices;
using System.ServiceProcess;
using McpOsDoctor.Core.DataTypes;
using McpOsDoctor.Core.Enums;
using McpOsDoctor.Core.Errors;
using McpOsDoctor.Core.Filters;
using McpOsDoctor.Core.Interfaces;
using McpOsDoctor.Core.Models;

namespace McpOsDoctor.Platform.Windows;

/// <summary>
/// Windows implementation of <see cref="IServiceInspector"/> using
/// <see cref="ServiceController"/> and WMI for enriched service information.
/// </summary>
public sealed class WindowsServiceInspector : IServiceInspector
{
    /// <inheritdoc />
    public async IAsyncEnumerable<ServiceInfo> GetServicesAsync(
        ServiceFilter filter,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var services = EnsureGetServices();

        // Build a lookup of WMI service data in a single query
        var wmiLookup = await GetServiceWmiDataAsync(cancellationToken);

        foreach (var sc in services.Take(filter.MaxResults))
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (sc)
            {
                // Apply name pattern filter (prefix match)
                if (!string.IsNullOrEmpty(filter.NamePattern)
                    && !sc.ServiceName.StartsWith(filter.NamePattern, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var runState = MapStatus(sc.Status);

                // Apply status filter
                if (filter.Status.HasValue && filter.Status.Value != runState)
                {
                    continue;
                }

                var startType = GetStartType(sc);
                var dependencies = GetDependencies(sc);
                wmiLookup.TryGetValue(sc.ServiceName, out var wmiData);

                var info = new ServiceInfo
                {
                    Name = sc.ServiceName,
                    DisplayName = sc.DisplayName,
                    Status = runState,
                    StartType = startType,
                    Account = wmiData?.Account ?? "Unknown",
                    Description = wmiData?.Description,
                    ExecutablePath = wmiData?.PathName,
                    Dependencies = dependencies
                };

                yield return info;
            }
        }
    }

    private static ServiceController[] EnsureGetServices()
    {
        try
        {
            return ServiceController.GetServices();
        }
        catch (Exception ex)
        {
            throw McpOsDoctorException.PlatformError(
                $"Failed to enumerate services: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<ServiceInfo> GetServiceAsync(SourceName name, CancellationToken cancellationToken)
    {
        ServiceController sc;
        try
        {
            sc = new ServiceController(name);
            // Force a status check to validate the service exists
            _ = sc.Status;
        }
        catch (InvalidOperationException)
        {
            throw McpOsDoctorException.SourceNotFound(name);
        }
        catch (Exception ex)
        {
            throw McpOsDoctorException.PlatformError(
                $"Failed to query service '{name}': {ex.Message}", ex);
        }

        using (sc)
        {
            var wmiData = await GetSingleServiceWmiDataAsync(name, cancellationToken);
            var startType = GetStartType(sc);
            var dependencies = GetDependencies(sc);

            return new ServiceInfo
            {
                Name = sc.ServiceName,
                DisplayName = sc.DisplayName,
                Status = MapStatus(sc.Status),
                StartType = startType,
                Account = wmiData?.Account ?? "Unknown",
                Description = wmiData?.Description,
                ExecutablePath = wmiData?.PathName,
                Dependencies = dependencies
            };
        }
    }

    private static ServiceRunState MapStatus(ServiceControllerStatus status)
    {
        return status switch
        {
            ServiceControllerStatus.Running => ServiceRunState.Running,
            ServiceControllerStatus.Stopped => ServiceRunState.Stopped,
            ServiceControllerStatus.Paused => ServiceRunState.Paused,
            ServiceControllerStatus.StartPending => ServiceRunState.StartPending,
            ServiceControllerStatus.StopPending => ServiceRunState.StopPending,
            ServiceControllerStatus.ContinuePending => ServiceRunState.Running,
            ServiceControllerStatus.PausePending => ServiceRunState.Paused,
            _ => ServiceRunState.Unknown
        };
    }

    private static Core.Enums.ServiceStartMode GetStartType(ServiceController sc)
    {
        try
        {
            return sc.StartType switch
            {
                System.ServiceProcess.ServiceStartMode.Automatic => Core.Enums.ServiceStartMode.Automatic,
                System.ServiceProcess.ServiceStartMode.Manual => Core.Enums.ServiceStartMode.Manual,
                System.ServiceProcess.ServiceStartMode.Disabled => Core.Enums.ServiceStartMode.Disabled,
                System.ServiceProcess.ServiceStartMode.Boot => Core.Enums.ServiceStartMode.Boot,
                System.ServiceProcess.ServiceStartMode.System => Core.Enums.ServiceStartMode.System,
                _ => Core.Enums.ServiceStartMode.Manual
            };
        }
        catch
        {
            return Core.Enums.ServiceStartMode.Manual;
        }
    }

    private static IReadOnlyList<string> GetDependencies(ServiceController sc)
    {
        try
        {
            var dependedOn = sc.ServicesDependedOn;
            if (dependedOn is null || dependedOn.Length == 0)
            {
                return [];
            }

            return dependedOn.Select(d => d.ServiceName)
                             .ToList();
        }
        catch
        {
            return [];
        }
    }

    private static Task<Dictionary<string, WmiServiceData>> GetServiceWmiDataAsync(
        CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            var lookup = new Dictionary<string, WmiServiceData>(StringComparer.OrdinalIgnoreCase);

            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT Name, StartName, Description, PathName FROM Win32_Service");

                using var results = searcher.Get();
                foreach (var obj in results)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    using (obj)
                    {
                        var serviceName = obj["Name"]?.ToString();
                        if (!string.IsNullOrEmpty(serviceName))
                        {
                            lookup[serviceName] = new WmiServiceData(
                                obj["StartName"]?.ToString() ?? "Unknown",
                                obj["Description"]?.ToString(),
                                obj["PathName"]?.ToString());
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // WMI may fail; we'll return partial data
            }

            return lookup;
        }, cancellationToken);
    }

    private static Task<WmiServiceData> GetSingleServiceWmiDataAsync(
        string serviceName,
        CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            try
            {
                var escapedName = serviceName.Replace("'", "\\'");
                using var searcher = new ManagementObjectSearcher(
                    new ObjectQuery(
                        $"SELECT StartName, Description, PathName FROM Win32_Service WHERE Name = '{escapedName}'"));

                using var results = searcher.Get();
                foreach (var obj in results)
                {
                    using (obj)
                    {
                        return new WmiServiceData(
                            obj["StartName"]?.ToString() ?? "Unknown",
                            obj["Description"]?.ToString(),
                            obj["PathName"]?.ToString());
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw McpOsDoctorException.PlatformError(
                    $"WMI query failed for service '{serviceName}': {ex.Message}", ex);
            }

            return (WmiServiceData)null;
        }, cancellationToken);
    }

    /// <summary>
    /// Holds the WMI-sourced data for a single service.
    /// </summary>
    /// <param name="Account">Account under which the service runs.</param>
    /// <param name="Description">Human-readable description, if available.</param>
    /// <param name="PathName">Full path to the service executable, if available.</param>
    private record WmiServiceData(string Account, string Description, string PathName);
}