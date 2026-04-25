namespace McpOsDoctor.Core.Models;

/// <summary>
/// Information about a mounted disk volume.
/// </summary>
public record DiskVolumeInfo
{
    /// <summary>
    /// Drive letter or mount point (e.g., "C:\").
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Volume label, if set.
    /// </summary>
    public string Label { get; init; }

    /// <summary>
    /// File system format (e.g., "NTFS", "ext4").
    /// </summary>
    public required string FileSystem { get; init; }

    /// <summary>
    /// Total capacity of the volume in gigabytes.
    /// </summary>
    public required double TotalGb { get; init; }

    /// <summary>
    /// Free space on the volume in gigabytes.
    /// </summary>
    public required double FreeGb { get; init; }
}