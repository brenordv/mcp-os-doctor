namespace McpOsDoctor.Core.Enums;

/// <summary>
/// Type of boot or shutdown event recorded by the operating system.
/// </summary>
public enum BootType
{
    /// <summary>A normal system boot.</summary>
    Normal = 1,

    /// <summary>A clean system shutdown.</summary>
    Shutdown = 2,

    /// <summary>An unexpected shutdown (power loss, crash).</summary>
    UnexpectedShutdown = 3,

    /// <summary>A blue screen of death (bugcheck) occurred.</summary>
    BlueScreen = 4,

    /// <summary>The system entered a sleep or hibernate state.</summary>
    Sleep = 5,

    /// <summary>The system resumed from sleep or hibernate.</summary>
    Wake = 6
}