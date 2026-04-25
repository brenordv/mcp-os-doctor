namespace McpOsDoctor.Core.Enums;

/// <summary>
/// Result of a privilege check for a specific capability.
/// </summary>
public enum PrivilegeStatus
{
    /// <summary>Access is granted under the current privilege level.</summary>
    Granted = 1,

    /// <summary>Access requires to be elevated (administrator) privileges.</summary>
    NeedsElevation = 2
}