using System.Security.Principal;
using McpOsDoctor.Core.Enums;
using McpOsDoctor.Core.Interfaces;

namespace McpOsDoctor.Platform.Windows;

/// <summary>
/// Windows implementation of <see cref="IPrivilegeChecker"/> that uses
/// <see cref="WindowsIdentity"/> to determine elevation status.
/// </summary>
public sealed class WindowsPrivilegeChecker : IPrivilegeChecker
{
    private static readonly HashSet<string> ElevatedCapabilities = new(StringComparer.OrdinalIgnoreCase)
    {
        "log:Security",
        "log:Setup"
    };

    /// <inheritdoc />
    public bool IsElevated => DetermineElevation();

    /// <inheritdoc />
    public PrivilegeStatus CanAccess(string capability)
    {
        if (string.IsNullOrWhiteSpace(capability))
        {
            return PrivilegeStatus.Granted;
        }

        if (!ElevatedCapabilities.Contains(capability))
        {
            return PrivilegeStatus.Granted;
        }

        return IsElevated ? PrivilegeStatus.Granted : PrivilegeStatus.NeedsElevation;
    }

    private static bool DetermineElevation()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}