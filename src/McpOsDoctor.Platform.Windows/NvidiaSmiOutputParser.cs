using System.Globalization;
using McpOsDoctor.Core.Errors;
using McpOsDoctor.Core.Models;

namespace McpOsDoctor.Platform.Windows;

/// <summary>
/// Parses the CSV output produced by nvidia-smi --query-gpu into <see cref="GpuInfo"/> records.
/// </summary>
public static class NvidiaSmiOutputParser
{
    private const int ExpectedFieldCount = 12;

    /// <summary>
    /// Parses nvidia-smi CSV output (one line per GPU, no header, no units) into a list of <see cref="GpuInfo"/>.
    /// </summary>
    /// <param name="output">Raw standard output from nvidia-smi.</param>
    /// <returns>A list of parsed GPU device snapshots.</returns>
    /// <exception cref="McpOsDoctorException">Thrown when parsing fails or no GPU data is found.</exception>
    public static IReadOnlyList<GpuInfo> Parse(string output)
    {
        var devices = new List<GpuInfo>();

        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var fields = line.Split(',', StringSplitOptions.TrimEntries);
            if (fields.Length < ExpectedFieldCount)
            {
                continue;
            }

            devices.Add(new GpuInfo
            {
                Name = fields[0],
                DriverVersion = fields[1],
                MemoryTotalMb = ParseRequired<double>(fields[2], "memory.total"),
                MemoryUsedMb = ParseRequired<double>(fields[3], "memory.used"),
                MemoryFreeMb = ParseRequired<double>(fields[4], "memory.free"),
                TemperatureCelsius = ParseRequired<int>(fields[5], "temperature.gpu"),
                GpuUtilizationPercent = ParseRequired<int>(fields[6], "utilization.gpu"),
                MemoryUtilizationPercent = ParseRequired<int>(fields[7], "utilization.memory"),
                PowerDrawWatts = ParseOptional<double>(fields[8]),
                PowerLimitWatts = ParseOptional<double>(fields[9]),
                FanSpeedPercent = ParseOptional<int>(fields[10]),
                PerformanceState = IsSupported(fields[11]) ? fields[11] : null
            });
        }

        if (devices.Count == 0)
        {
            throw McpOsDoctorException.PlatformError(
                "nvidia-smi returned no GPU data. Verify that an NVIDIA GPU is installed.");
        }

        return devices;
    }

    private static bool IsSupported(string value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && !value.Contains("Not Supported", StringComparison.OrdinalIgnoreCase)
            && !value.Equals("N/A", StringComparison.OrdinalIgnoreCase);
    }

    private static T ParseRequired<T>(string value, string fieldName) where T : IParsable<T>
    {
        if (T.TryParse(value, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        throw McpOsDoctorException.PlatformError(
            $"nvidia-smi returned an unparseable value for {fieldName}: '{value}'.");
    }

    private static T? ParseOptional<T>(string value) where T : struct, IParsable<T>
    {
        if (!IsSupported(value))
        {
            return null;
        }

        return T.TryParse(value, CultureInfo.InvariantCulture, out var result) ? result : null;
    }
}