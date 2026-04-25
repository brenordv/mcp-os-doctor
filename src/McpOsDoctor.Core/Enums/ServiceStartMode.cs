namespace McpOsDoctor.Core.Enums;

/// <summary>
/// Startup configuration mode for a system service.
/// </summary>
public enum ServiceStartMode
{
    /// <summary>The service starts automatically at system startup.</summary>
    Automatic = 1,

    /// <summary>The service must be started manually.</summary>
    Manual = 2,

    /// <summary>The service is disabled and cannot be started.</summary>
    Disabled = 3,

    /// <summary>The service is a device driver started by the system loader.</summary>
    Boot = 4,

    /// <summary>The service is a device driver started during kernel initialization.</summary>
    System = 5
}