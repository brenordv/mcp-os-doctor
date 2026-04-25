namespace McpOsDoctor.Core.Models;

/// <summary>
/// Point-in-time snapshot of DirectX diagnostic information.
/// </summary>
public record DirectXInfo
{
    /// <summary>
    /// DirectX version installed (e.g., "DirectX 12").
    /// </summary>
    public required string DirectXVersion { get; init; }

    /// <summary>
    /// DxDiag tool version string.
    /// </summary>
    public required string DxDiagVersion { get; init; }

    /// <summary>
    /// Diagnostic notes reported by DxDiag (e.g., "No problems found." per tab).
    /// </summary>
    public required IReadOnlyList<string> Notes { get; init; }

    /// <summary>
    /// Display adapters detected by DirectX.
    /// </summary>
    public required IReadOnlyList<DirectXDisplayDevice> DisplayDevices { get; init; }

    /// <summary>
    /// Sound devices detected by DirectX.
    /// </summary>
    public required IReadOnlyList<DirectXSoundDevice> SoundDevices { get; init; }
}