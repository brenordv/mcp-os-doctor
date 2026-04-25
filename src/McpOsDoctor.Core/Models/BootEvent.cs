using McpOsDoctor.Core.Enums;

namespace McpOsDoctor.Core.Models;

/// <summary>
/// A recorded boot, shutdown, or power state change event.
/// </summary>
public record BootEvent
{
    /// <summary>
    /// When the boot/shutdown event occurred.
    /// </summary>
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// Classification of the boot or shutdown event.
    /// </summary>
    public required BootType Type { get; init; }

    /// <summary>
    /// Duration of the event in seconds (e.g., boot time), if available.
    /// </summary>
    public double? DurationSeconds { get; init; }
}