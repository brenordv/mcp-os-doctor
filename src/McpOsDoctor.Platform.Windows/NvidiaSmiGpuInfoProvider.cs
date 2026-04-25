using System.Diagnostics;
using McpOsDoctor.Core.Errors;
using McpOsDoctor.Core.Interfaces;
using McpOsDoctor.Core.Models;

namespace McpOsDoctor.Platform.Windows;

/// <summary>
/// Windows implementation of <see cref="IGpuInfoProvider"/> using the nvidia-smi CLI tool.
/// </summary>
public sealed class NvidiaSmiGpuInfoProvider : IGpuInfoProvider
{
    private const string NvidiaSmiExecutable = "nvidia-smi.exe";

    private const string QueryFields =
        "name,driver_version,memory.total,memory.used,memory.free," +
        "temperature.gpu,utilization.gpu,utilization.memory," +
        "power.draw,power.limit,fan.speed,pstate";

    private const int ProcessTimeoutMs = 10_000;

    private static readonly string NvidiaSmiPath = FindNvidiaSmiPath();

    /// <inheritdoc />
    public bool IsAvailable => NvidiaSmiPath is not null;

    /// <inheritdoc />
    public string UnavailableReason => IsAvailable
        ? null
        : "nvidia-smi was not found on this system. NVIDIA GPU drivers may not be installed.";

    /// <inheritdoc />
    public async Task<IReadOnlyList<GpuInfo>> GetGpuInfoAsync(CancellationToken cancellationToken)
    {
        if (!IsAvailable)
        {
            throw McpOsDoctorException.NotSupported(
                "GPU information is not available because nvidia-smi was not found.",
                "Install NVIDIA GPU drivers to enable GPU diagnostics.");
        }

        try
        {
            var output = await RunNvidiaSmiAsync(cancellationToken);
            return NvidiaSmiOutputParser.Parse(output);
        }
        catch (Exception ex) when (ex is not OperationCanceledException and not McpOsDoctorException)
        {
            throw McpOsDoctorException.PlatformError(
                $"Failed to query GPU information: {ex.Message}", ex);
        }
    }

    private static async Task<string> RunNvidiaSmiAsync(CancellationToken cancellationToken)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = NvidiaSmiPath,
                Arguments = $"--query-gpu={QueryFields} --format=csv,noheader,nounits",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();

        using var timeoutCts = new CancellationTokenSource(ProcessTimeoutMs);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        var output = await process.StandardOutput.ReadToEndAsync(linkedCts.Token);

        await process.WaitForExitAsync(linkedCts.Token);

        if (process.ExitCode != 0)
        {
            var stderr = await process.StandardError.ReadToEndAsync(linkedCts.Token);
            throw McpOsDoctorException.PlatformError(
                $"nvidia-smi exited with code {process.ExitCode}: {stderr.Trim()}");
        }

        return output;
    }

    private static string FindNvidiaSmiPath()
    {
        // Standard location for modern NVIDIA drivers
        var systemPath = Path.Combine(Environment.SystemDirectory, NvidiaSmiExecutable);
        if (File.Exists(systemPath))
        {
            return systemPath;
        }

        // Legacy NVIDIA driver location
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var nvsmiPath = Path.Combine(programFiles, "NVIDIA Corporation", "NVSMI", NvidiaSmiExecutable);
        if (File.Exists(nvsmiPath))
        {
            return nvsmiPath;
        }

        // Fall back to PATH search for non-standard installations
        return FindInPath(NvidiaSmiExecutable);
    }

    private static string FindInPath(string executable)
    {
        var pathVariable = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathVariable))
        {
            return null;
        }

        foreach (var directory in pathVariable.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var fullPath = Path.Combine(directory, executable);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        return null;
    }
}