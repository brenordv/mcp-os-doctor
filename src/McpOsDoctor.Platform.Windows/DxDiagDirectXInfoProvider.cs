using System.Diagnostics;
using McpOsDoctor.Core.Errors;
using McpOsDoctor.Core.Interfaces;
using McpOsDoctor.Core.Models;

namespace McpOsDoctor.Platform.Windows;

/// <summary>
/// Windows implementation of <see cref="IDirectXInfoProvider"/> using the dxdiag CLI tool.
/// </summary>
public sealed class DxDiagDirectXInfoProvider : IDirectXInfoProvider
{
    private const string DxDiagExecutable = "dxdiag.exe";
    private const int ProcessTimeoutMs = 30_000;

    private static readonly string DxDiagPath = FindDxDiagPath();

    /// <inheritdoc />
    public bool IsAvailable => DxDiagPath is not null;

    /// <inheritdoc />
    public string UnavailableReason => IsAvailable
        ? null
        : "dxdiag was not found on this system. DirectX may not be installed.";

    /// <inheritdoc />
    public async Task<DirectXInfo> GetDirectXInfoAsync(CancellationToken cancellationToken)
    {
        if (!IsAvailable)
        {
            throw McpOsDoctorException.NotSupported(
                "DirectX diagnostics are not available because dxdiag was not found.",
                "Verify that DirectX is installed on this system.");
        }

        try
        {
            string output = await RunDxDiagAsync(cancellationToken);
            return DxDiagOutputParser.Parse(output);
        }
        catch (Exception ex) when (ex is not OperationCanceledException and not McpOsDoctorException)
        {
            throw McpOsDoctorException.PlatformError(
                $"Failed to query DirectX diagnostics: {ex.Message}", ex);
        }
    }

    private static async Task<string> RunDxDiagAsync(CancellationToken cancellationToken)
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"mcp_dxdiag_{Environment.ProcessId}.txt");

        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = DxDiagPath,
                    Arguments = $"/t \"{tempFile}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            using var timeoutCts = new CancellationTokenSource(ProcessTimeoutMs);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            await process.WaitForExitAsync(linkedCts.Token);

            if (process.ExitCode != 0)
            {
                string stderr = await process.StandardError.ReadToEndAsync(linkedCts.Token);
                throw McpOsDoctorException.PlatformError(
                    $"dxdiag exited with code {process.ExitCode}: {stderr.Trim()}");
            }

            return !File.Exists(tempFile)
                ? throw McpOsDoctorException.PlatformError("dxdiag completed but did not produce an output file.")
                : await File.ReadAllTextAsync(tempFile, linkedCts.Token);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    private static string FindDxDiagPath()
    {
        // dxdiag.exe is in System32 on all supported Windows versions
        var systemPath = Path.Combine(Environment.SystemDirectory, DxDiagExecutable);
        return File.Exists(systemPath)
            ? systemPath
            // Fall back to PATH search for non-standard installations
            : FindInPath(DxDiagExecutable);
    }

    private static string FindInPath(string executable)
    {
        var pathVariable = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathVariable))
        {
            return null;
        }

        foreach (string directory in pathVariable.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            string fullPath = Path.Combine(directory, executable);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        return null;
    }
}