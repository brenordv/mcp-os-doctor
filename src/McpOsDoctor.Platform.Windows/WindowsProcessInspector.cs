using System.Diagnostics;
using System.Management;
using System.Runtime.CompilerServices;
using McpOsDoctor.Core.Enums;
using McpOsDoctor.Core.Errors;
using McpOsDoctor.Core.Filters;
using McpOsDoctor.Core.Interfaces;
using McpOsDoctor.Core.Models;

namespace McpOsDoctor.Platform.Windows;

/// <summary>
/// Windows implementation of <see cref="IProcessInspector"/> using
/// <see cref="Process"/> and WMI for CPU usage and command-line information.
/// </summary>
public sealed class WindowsProcessInspector(IOutputSanitizer sanitizer) : IProcessInspector
{
    private const int CommandLineTopN = 50;

    /// <inheritdoc />
    public async IAsyncEnumerable<ProcessInfo> GetTopProcessesAsync(
        ProcessFilter filter,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Get CPU data from WMI in a single shot
        var cpuData = await GetCpuDataAsync(cancellationToken);

        // Get all processes
        Process[] processes;
        try
        {
            processes = Process.GetProcesses();
        }
        catch (Exception ex)
        {
            throw McpOsDoctorException.PlatformError(
                $"Failed to enumerate processes: {ex.Message}", ex);
        }

        var processInfos = new List<ProcessInfo>();
        foreach (var proc in processes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var pid = proc.Id;
                var name = proc.ProcessName;
                var memoryMB = proc.WorkingSet64 / (1024.0 * 1024.0);

                cpuData.TryGetValue(pid, out var cpuPercent);

                DateTimeOffset? startTime = null;
                try
                {
                    startTime = new DateTimeOffset(proc.StartTime);
                }
                catch
                {
                    // Access denied for system processes
                }

                // Apply minimum CPU filter
                if (filter.MinCpuPercent.HasValue && cpuPercent < filter.MinCpuPercent.Value)
                {
                    continue;
                }

                // Apply minimum memory filter
                if (filter.MinMemoryMb.HasValue && memoryMB < filter.MinMemoryMb.Value)
                {
                    continue;
                }

                processInfos.Add(new ProcessInfo
                {
                    Pid = pid,
                    Name = name,
                    CpuPercent = Math.Round(cpuPercent, 2),
                    MemoryMb = Math.Round(memoryMB, 2),
                    StartTime = startTime
                });
            }
            catch
            {
                // Skip inaccessible processes
            }
            finally
            {
                proc.Dispose();
            }
        }

        // Sort by specified field
        var sorted = filter.SortBy switch
        {
            ProcessSortField.Cpu => processInfos.OrderByDescending(p => p.CpuPercent),
            ProcessSortField.Memory => processInfos.OrderByDescending(p => p.MemoryMb),
            ProcessSortField.Name => processInfos.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase),
            _ => processInfos.OrderByDescending(p => p.CpuPercent)
        };

        // Take top N and fetch command lines for those
        var topProcesses = sorted.Take(filter.MaxResults).ToList();

        var commandLines = await GetCommandLinesAsync(
            topProcesses.Take(CommandLineTopN).Select(p => p.Pid),
            cancellationToken);

        foreach (var proc in topProcesses)
        {
            cancellationToken.ThrowIfCancellationRequested();

            commandLines.TryGetValue(proc.Pid, out var cmdLine);
            var sanitizedCmdLine = cmdLine is not null ? sanitizer.Sanitize(cmdLine) : null;

            yield return proc with { CommandLine = sanitizedCmdLine };
        }
    }

    private static Task<Dictionary<int, double>> GetCpuDataAsync(CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            var cpuData = new Dictionary<int, double>();

            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT Name, PercentProcessorTime, IDProcess FROM Win32_PerfFormattedData_PerfProc_Process");

                using var results = searcher.Get();
                foreach (var obj in results)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    using (obj)
                    {
                        var idProcess = obj["IDProcess"];
                        var percentCpu = obj["PercentProcessorTime"];

                        if (idProcess is not null && percentCpu is not null)
                        {
                            var pid = Convert.ToInt32(idProcess);
                            var cpu = Convert.ToDouble(percentCpu);
                            cpuData[pid] = cpu;
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // WMI may fail; return empty CPU data — memory-only sorting still works
            }

            return cpuData;
        }, cancellationToken);
    }

    private static Task<Dictionary<int, string>> GetCommandLinesAsync(
        IEnumerable<int> pids,
        CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            var commandLines = new Dictionary<int, string>();
            var pidList = pids.ToList();

            if (pidList.Count == 0)
            {
                return commandLines;
            }

            try
            {
                // Build a safe WHERE clause with numeric PIDs only
                var pidConditions = string.Join(" OR ", pidList.Select(pid => $"ProcessId = {pid}"));
                var query = new ObjectQuery($"SELECT ProcessId, CommandLine FROM Win32_Process WHERE {pidConditions}");

                using var searcher = new ManagementObjectSearcher(query);
                using var results = searcher.Get();

                foreach (var obj in results)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    using (obj)
                    {
                        var pidVal = obj["ProcessId"];
                        var cmdLine = obj["CommandLine"]?.ToString();

                        if (pidVal is not null && cmdLine is not null)
                        {
                            commandLines[Convert.ToInt32(pidVal)] = cmdLine;
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Command line retrieval is best-effort
            }

            return commandLines;
        }, cancellationToken);
    }
}