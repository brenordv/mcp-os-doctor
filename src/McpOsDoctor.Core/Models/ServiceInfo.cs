using McpOsDoctor.Core.Enums;

namespace McpOsDoctor.Core.Models;

/// <summary>
/// Normalized information about a system service.
/// </summary>
public record ServiceInfo
{
    /// <summary>
    /// Internal service name used by the service control manager.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Human-readable display the name of the service.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Current running state of the service.
    /// </summary>
    public required ServiceRunState Status { get; init; }

    /// <summary>
    /// How the service is configured to start.
    /// </summary>
    public required ServiceStartMode StartType { get; init; }

    /// <summary>
    /// Account under which the service runs.
    /// </summary>
    public required string Account { get; init; }

    /// <summary>
    /// Human-readable description of the service, if available.
    /// </summary>
    public string Description { get; init; }

    /// <summary>
    /// Full path to the service executable, if available.
    /// </summary>
    public string ExecutablePath { get; init; }

    /// <summary>
    /// Internal names of services that this service depends on.
    /// </summary>
    public IReadOnlyList<string> Dependencies { get; init; } = [];

    /// <summary>
    /// Process identifier of the running service, if applicable.
    /// </summary>
    public int? Pid { get; init; }

    /// <summary>
    /// Memory usage in megabytes, if available.
    /// </summary>
    public double? MemoryMb { get; init; }
}