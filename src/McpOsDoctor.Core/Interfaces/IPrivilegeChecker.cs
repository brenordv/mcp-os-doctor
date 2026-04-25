using McpOsDoctor.Core.Enums;

namespace McpOsDoctor.Core.Interfaces;

/// <summary>
/// Checks the current process's privilege level and access to specific capabilities.
/// </summary>
public interface IPrivilegeChecker
{
    /// <summary>
    /// Whether the current process is running with elevated (administrator) privileges.
    /// </summary>
    bool IsElevated { get; }

    /// <summary>
    /// Checks whether the current process can access the specified capability.
    /// </summary>
    /// <param name="capability">A capability identifier (e.g., "log:Security").</param>
    /// <returns>The privilege status for the requested capability.</returns>
    PrivilegeStatus CanAccess(string capability);
}