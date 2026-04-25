using McpOsDoctor.Core.Errors;
using McpOsDoctor.Core.Interfaces;
using McpOsDoctor.Core.Models;

namespace McpOsDoctor.Tools.Tests.Fakes;

/// <summary>
/// Fake implementation of <see cref="IDirectXInfoProvider"/> for unit testing.
/// When <see cref="IsAvailable"/> is false, <see cref="GetDirectXInfoAsync"/> throws
/// a <see cref="McpOsDoctorException"/> to match the real provider behavior.
/// </summary>
public sealed class FakeDirectXInfoProvider : IDirectXInfoProvider
{
    /// <inheritdoc />
    public bool IsAvailable { get; init; } = true;

    /// <inheritdoc />
    public string UnavailableReason { get; init; }

    /// <summary>
    /// The DirectX info returned by <see cref="GetDirectXInfoAsync"/>.
    /// </summary>
    public DirectXInfo Info { get; init; } = new DirectXInfo
    {
        DirectXVersion = "DirectX 12",
        DxDiagVersion = "10.00.19041.5438 64bit Unicode",
        Notes = ["No problems found."],
        DisplayDevices =
        [
            new DirectXDisplayDevice
            {
                CardName = "NVIDIA Test GPU",
                Manufacturer = "NVIDIA",
                ChipType = "NVIDIA Test GPU",
                DeviceType = "Full Device (POST)",
                DisplayMemoryMb = 48767,
                DedicatedMemoryMb = 16047,
                SharedMemoryMb = 32720,
                CurrentMode = "2560 x 1440 (32 bit) (144Hz)",
                HdrSupport = "Supported",
                MonitorName = "Generic PnP Monitor",
                MonitorModel = "Test Monitor",
                OutputType = "HDMI",
                DriverVersion = "32.0.15.9576",
                DdiVersion = "12",
                FeatureLevels = "12_1,12_0,11_1,11_0,10_1,10_0,9_3,9_2,9_1",
                DriverModel = "WDDM 2.7",
                HardwareScheduling = "Supported:True Enabled:True"
            }
        ],
        SoundDevices =
        [
            new DirectXSoundDevice
            {
                Description = "Speakers (Test Audio)",
                DefaultSoundPlayback = true,
                DefaultVoicePlayback = true,
                DriverName = "TestAudio.sys",
                DriverVersion = "1.0.0.0",
                DriverProvider = "Test Corporation"
            }
        ]
    };

    /// <inheritdoc />
    public Task<DirectXInfo> GetDirectXInfoAsync(CancellationToken cancellationToken)
    {
        if (!IsAvailable)
        {
            throw McpOsDoctorException.NotSupported(
                "DirectX diagnostics are not available because dxdiag was not found.",
                "Verify that DirectX is installed on this system.");
        }

        return Task.FromResult(Info);
    }
}