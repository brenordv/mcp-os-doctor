namespace McpOsDoctor.Core.Enums;

/// <summary>
/// Current running state of a system service.
/// </summary>
public enum ServiceRunState
{
    /// <summary>The service is actively running.</summary>
    Running = 1,

    /// <summary>The service is stopped.</summary>
    Stopped = 2,

    /// <summary>The service is paused.</summary>
    Paused = 3,

    /// <summary>The service is in the process of starting.</summary>
    StartPending = 4,

    /// <summary>The service is in the process of stopping.</summary>
    StopPending = 5,

    /// <summary>The service state could not be determined.</summary>
    Unknown = 6
}