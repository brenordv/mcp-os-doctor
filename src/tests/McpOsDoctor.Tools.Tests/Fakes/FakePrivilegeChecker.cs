using McpOsDoctor.Core.Enums;
using McpOsDoctor.Core.Interfaces;

namespace McpOsDoctor.Tools.Tests.Fakes;

/// <summary>
/// Fake implementation of <see cref="IPrivilegeChecker"/> for unit testing.
/// </summary>
public sealed class FakePrivilegeChecker : IPrivilegeChecker
{
    private readonly IDictionary<string, PrivilegeStatus> _accessResults = new Dictionary<string, PrivilegeStatus>(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public bool IsElevated { get; init; }

    /// <summary>
    /// Configures the result for a specific capability check.
    /// </summary>
    /// <param name="capability">The capability identifier.</param>
    /// <param name="status">The status to return.</param>
    public void SetCanAccess(string capability, PrivilegeStatus status)
    {
        _accessResults[capability] = status;
    }

    /// <inheritdoc />
    public PrivilegeStatus CanAccess(string capability)
    {
        return _accessResults.TryGetValue(capability, out var status)
            ? status
            : PrivilegeStatus.Granted;
    }
}