using System.Management;
using System.Runtime.InteropServices;
using McpOsDoctor.Core.Errors;
using McpOsDoctor.Core.Interfaces;
using McpOsDoctor.Core.Models;

namespace McpOsDoctor.Platform.Windows;

/// <summary>
/// Windows implementation of <see cref="ISystemInfoProvider"/> using WMI,
/// <see cref="Environment"/>, and <see cref="DriveInfo"/> for system information.
/// </summary>
public sealed class WindowsSystemInfoProvider(IPrivilegeChecker privilegeChecker) : ISystemInfoProvider
{
    /// <inheritdoc />
    public Task<SystemSnapshot> GetSystemInfoAsync(CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            try
            {
                var hostname = GetHostname(cancellationToken);
                var processorName = GetProcessorName(cancellationToken);
                var (totalMemoryGb, freeMemoryGb) = GetMemoryInfo(cancellationToken);
                var diskVolumes = GetDiskVolumes();
                var uptimeHours = Environment.TickCount64 / 3_600_000.0;

                return new SystemSnapshot
                {
                    Hostname = hostname,
                    OsVersion = Environment.OSVersion.VersionString,
                    OsArchitecture = RuntimeInformation.OSArchitecture.ToString(),
                    ProcessorName = processorName,
                    ProcessorCores = Environment.ProcessorCount,
                    TotalMemoryGb = Math.Round(totalMemoryGb, 2),
                    AvailableMemoryGb = Math.Round(freeMemoryGb, 2),
                    UptimeHours = Math.Round(uptimeHours, 2),
                    IsElevated = privilegeChecker.IsElevated,
                    DiskVolumes = diskVolumes
                };
            }
            catch (Exception ex) when (ex is not OperationCanceledException and not McpOsDoctorException)
            {
                throw McpOsDoctorException.PlatformError(
                    $"Failed to collect system information: {ex.Message}", ex);
            }
        }, cancellationToken);
    }

    private static string GetHostname(CancellationToken cancellationToken)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_ComputerSystem");
            using var results = searcher.Get();

            foreach (var obj in results)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using (obj)
                {
                    return obj["Name"]?.ToString() ?? Environment.MachineName;
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Fall back to Environment
        }

        return Environment.MachineName;
    }

    private static string GetProcessorName(CancellationToken cancellationToken)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor");
            using var results = searcher.Get();

            foreach (var obj in results)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using (obj)
                {
                    return obj["Name"]?.ToString() ?? "Unknown Processor";
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Fall back to default
        }

        return "Unknown Processor";
    }

    private static (double TotalGB, double FreeGB) GetMemoryInfo(CancellationToken cancellationToken)
    {
        double totalGB = 0;
        double freeGB = 0;

        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem");
            using var results = searcher.Get();

            foreach (var obj in results)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using (obj)
                {
                    var totalKB = Convert.ToDouble(obj["TotalVisibleMemorySize"]);
                    var freeKB = Convert.ToDouble(obj["FreePhysicalMemory"]);
                    totalGB = totalKB / (1024.0 * 1024.0);
                    freeGB = freeKB / (1024.0 * 1024.0);
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw McpOsDoctorException.PlatformError(
                $"Failed to query memory information: {ex.Message}", ex);
        }

        return (totalGB, freeGB);
    }

    private static IReadOnlyList<DiskVolumeInfo> GetDiskVolumes()
    {
        var volumes = new List<DiskVolumeInfo>();

        try
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (!drive.IsReady || drive.DriveType != DriveType.Fixed)
                {
                    continue;
                }

                volumes.Add(new DiskVolumeInfo
                {
                    Name = drive.Name,
                    Label = drive.VolumeLabel,
                    FileSystem = drive.DriveFormat,
                    TotalGb = Math.Round(drive.TotalSize / (1024.0 * 1024.0 * 1024.0), 2),
                    FreeGb = Math.Round(drive.TotalFreeSpace / (1024.0 * 1024.0 * 1024.0), 2)
                });
            }
        }
        catch (Exception ex)
        {
            throw McpOsDoctorException.PlatformError(
                $"Failed to enumerate disk volumes: {ex.Message}", ex);
        }

        return volumes.AsReadOnly();
    }
}