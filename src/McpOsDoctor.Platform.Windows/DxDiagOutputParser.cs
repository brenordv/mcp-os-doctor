using System.Globalization;
using McpOsDoctor.Core.Errors;
using McpOsDoctor.Core.Models;

namespace McpOsDoctor.Platform.Windows;

/// <summary>
/// Parses the text output produced by <c>dxdiag /t</c> into <see cref="DirectXInfo"/>.
/// </summary>
public static class DxDiagOutputParser
{
    /// <summary>
    /// Parses dxdiag text output into a <see cref="DirectXInfo"/> snapshot.
    /// </summary>
    /// <param name="output">Raw text output from <c>dxdiag /t</c>.</param>
    /// <returns>A parsed DirectX diagnostic snapshot.</returns>
    /// <exception cref="McpOsDoctorException">Thrown when parsing fails or required data is missing.</exception>
    public static DirectXInfo Parse(string output)
    {
        if (string.IsNullOrWhiteSpace(output))
        {
            throw McpOsDoctorException.PlatformError(
                "dxdiag returned no data. Verify that DirectX is installed.");
        }

        var sections = ParseSections(output);

        var systemInfo = GetSectionFields(sections, "System Information");
        var directXVersion = GetRequiredField(systemInfo, "DirectX Version");
        var dxDiagVersion = GetFieldOrDefault(systemInfo, "DxDiag Version");

        var notes = ParseNotes(sections);
        var displayDevices = ParseDisplayDevices(sections);
        var soundDevices = ParseSoundDevices(sections);

        return new DirectXInfo
        {
            DirectXVersion = directXVersion,
            DxDiagVersion = dxDiagVersion,
            Notes = notes,
            DisplayDevices = displayDevices,
            SoundDevices = soundDevices
        };
    }

    private static Dictionary<string, string> ParseSections(string output)
    {
        var sections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var lines = output.Split('\n');
        var i = 0;

        while (i < lines.Length)
        {
            // Look for a section header: a line of dashes, then a title, then another line of dashes
            if (IsDashLine(lines[i]) && i + 2 < lines.Length && IsDashLine(lines[i + 2]))
            {
                var sectionName = lines[i + 1].Trim();
                i += 3;

                var start = i;
                // Collect lines until the next section header or end of file
                while (i < lines.Length)
                {
                    if (IsDashLine(lines[i]) && i + 2 < lines.Length && IsDashLine(lines[i + 2]))
                    {
                        break;
                    }

                    i++;
                }

                var sectionContent = string.Join('\n', lines[start..i]);

                // dxdiag may have duplicate section names (e.g., multiple "Display Devices")
                // Keep the first occurrence; subsequent device blocks are within the same section content
                sections.TryAdd(sectionName, sectionContent);
                continue;
            }

            i++;
        }

        return sections;
    }

    private static bool IsDashLine(string line)
    {
        var trimmed = line.Trim();
        return trimmed.Length >= 3 && trimmed.Replace("-", "").Length == 0;
    }

    private static Dictionary<string, string> GetSectionFields(Dictionary<string, string> sections, string sectionName)
    {
        return !sections.TryGetValue(sectionName, out string content)
            ? throw McpOsDoctorException.PlatformError(
                $"dxdiag output is missing the '{sectionName}' section.")
            : ParseKeyValuePairs(content);
    }

    private static Dictionary<string, string> ParseKeyValuePairs(string content)
    {
        var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (string line in content.Split('\n'))
        {
            var colonIndex = line.IndexOf(':');
            if (colonIndex <= 0)
            {
                continue;
            }

            var key = line[..colonIndex].Trim();
            var value = line[(colonIndex + 1)..].Trim();

            if (key.Length > 0 && value.Length > 0)
            {
                // First occurrence wins (handles multi-line values like Deinterlace Caps)
                fields.TryAdd(key, value);
            }
        }

        return fields;
    }

    private static string GetRequiredField(Dictionary<string, string> fields, string key)
    {
        return fields.TryGetValue(key, out string value)
            ? value
            : throw McpOsDoctorException.PlatformError(
                $"dxdiag output is missing the required field '{key}'.");
    }

    private static string GetFieldOrDefault(Dictionary<string, string> fields, string key)
    {
        return fields.GetValueOrDefault(key);
    }

    private static IReadOnlyList<string> ParseNotes(Dictionary<string, string> sections)
    {
        if (!sections.TryGetValue("DxDiag Notes", out string content))
        {
            return [];
        }

        var notes = new List<string>();

        foreach (string line in content.Split('\n'))
        {
            int colonIndex = line.IndexOf(':');
            if (colonIndex <= 0)
            {
                continue;
            }

            var value = line[(colonIndex + 1)..].Trim();
            if (value.Length > 0)
            {
                notes.Add(value);
            }
        }

        return notes;
    }

    private static IReadOnlyList<DirectXDisplayDevice> ParseDisplayDevices(Dictionary<string, string> sections)
    {
        if (!sections.TryGetValue("Display Devices", out string content))
        {
            return [];
        }

        var devices = new List<DirectXDisplayDevice>();

        // Each device block starts with "Card name:" — split on that boundary
        foreach (string block in SplitDeviceBlocks(content, "Card name"))
        {
            var fields = ParseKeyValuePairs(block);
            if (!fields.TryGetValue("Card name", out string cardName))
            {
                continue;
            }

            devices.Add(new DirectXDisplayDevice
            {
                CardName = cardName,
                Manufacturer = GetRequiredField(fields, "Manufacturer"),
                ChipType = GetRequiredField(fields, "Chip type"),
                DeviceType = GetFieldOrDefault(fields, "Device Type"),
                DisplayMemoryMb = ParseMemoryMb(fields, "Display Memory"),
                DedicatedMemoryMb = ParseMemoryMb(fields, "Dedicated Memory"),
                SharedMemoryMb = ParseMemoryMb(fields, "Shared Memory"),
                CurrentMode = GetFieldOrDefault(fields, "Current Mode"),
                HdrSupport = GetFieldOrDefault(fields, "HDR Support"),
                MonitorName = GetFieldOrDefault(fields, "Monitor Name"),
                MonitorModel = GetFieldOrDefault(fields, "Monitor Model"),
                OutputType = GetFieldOrDefault(fields, "Output Type"),
                DriverVersion = GetRequiredField(fields, "Driver Version"),
                DdiVersion = GetFieldOrDefault(fields, "DDI Version"),
                FeatureLevels = GetFieldOrDefault(fields, "Feature Levels"),
                DriverModel = GetFieldOrDefault(fields, "Driver Model"),
                HardwareScheduling = GetFieldOrDefault(fields, "Hardware Scheduling")
            });
        }

        return devices;
    }

    private static IReadOnlyList<DirectXSoundDevice> ParseSoundDevices(Dictionary<string, string> sections)
    {
        if (!sections.TryGetValue("Sound Devices", out string content))
        {
            return [];
        }

        var devices = new List<DirectXSoundDevice>();

        foreach (string block in SplitDeviceBlocks(content, "Description"))
        {
            var fields = ParseKeyValuePairs(block);
            if (!fields.TryGetValue("Description", out string description))
            {
                continue;
            }

            devices.Add(new DirectXSoundDevice
            {
                Description = description,
                DefaultSoundPlayback = ParseYesNo(fields, "Default Sound Playback"),
                DefaultVoicePlayback = ParseYesNo(fields, "Default Voice Playback"),
                DriverName = GetFieldOrDefault(fields, "Driver Name"),
                DriverVersion = GetFieldOrDefault(fields, "Driver Version"),
                DriverProvider = GetFieldOrDefault(fields, "Driver Provider")
            });
        }

        return devices;
    }

    /// <summary>
    /// Splits a section's content into individual device blocks by detecting lines
    /// whose key matches <paramref name="firstFieldKey"/>.
    /// </summary>
    private static IEnumerable<string> SplitDeviceBlocks(string content, string firstFieldKey)
    {
        var marker = firstFieldKey + ":";
        var lines = content.Split('\n');
        var blockStart = -1;

        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i].TrimStart().StartsWith(marker, StringComparison.OrdinalIgnoreCase))
            {
                if (blockStart >= 0)
                {
                    yield return string.Join('\n', lines[blockStart..i]);
                }

                blockStart = i;
            }
        }

        if (blockStart >= 0)
        {
            yield return string.Join('\n', lines[blockStart..]);
        }
    }

    private static int ParseMemoryMb(Dictionary<string, string> fields, string key)
    {
        if (!fields.TryGetValue(key, out string raw))
        {
            throw McpOsDoctorException.PlatformError(
                $"dxdiag output is missing the required field '{key}'.");
        }

        // Format: "16047 MB" — strip the " MB" suffix
        var numeric = raw.Replace("MB", "", StringComparison.OrdinalIgnoreCase).Trim();

        return int.TryParse(numeric, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result)
            ? result
            : throw McpOsDoctorException.PlatformError(
                $"dxdiag returned an unparseable value for {key}: '{raw}'.");
    }

    private static bool ParseYesNo(Dictionary<string, string> fields, string key)
    {
        return fields.TryGetValue(key, out string value)
            && value.Equals("Yes", StringComparison.OrdinalIgnoreCase);
    }
}