namespace McpOsDoctor.Core.Models;

/// <summary>
/// Describes a single sound device as reported by DxDiag.
/// </summary>
public record DirectXSoundDevice
{
    /// <summary>
    /// Device description (e.g., "Speakers (Realtek(R) Audio)").
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Whether this is the default sound playback device.
    /// </summary>
    public required bool DefaultSoundPlayback { get; init; }

    /// <summary>
    /// Whether this is the default voice playback device.
    /// </summary>
    public required bool DefaultVoicePlayback { get; init; }

    /// <summary>
    /// Driver file name (e.g., "RTKVHD64.sys").
    /// </summary>
    public string DriverName { get; init; }

    /// <summary>
    /// Driver version string.
    /// </summary>
    public string DriverVersion { get; init; }

    /// <summary>
    /// Driver provider name (e.g., "Realtek Semiconductor Corp.").
    /// </summary>
    public string DriverProvider { get; init; }
}