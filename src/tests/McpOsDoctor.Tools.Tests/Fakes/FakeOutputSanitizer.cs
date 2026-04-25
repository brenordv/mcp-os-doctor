using McpOsDoctor.Core.Interfaces;

namespace McpOsDoctor.Tools.Tests.Fakes;

/// <summary>
/// Fake implementation of <see cref="IOutputSanitizer"/> for unit testing.
/// Passes input through unchanged by default, but can be configured to simulate redaction.
/// </summary>
public sealed class FakeOutputSanitizer : IOutputSanitizer
{
    /// <summary>
    /// When true, all calls to <see cref="Sanitize"/> will replace input with "[REDACTED]"
    /// and set <see cref="LastSanitizeRedacted"/> to true.
    /// </summary>
    public bool ShouldRedact { get; init; }

    /// <inheritdoc />
    public bool LastSanitizeRedacted { get; private set; }

    /// <inheritdoc />
    public string Sanitize(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            LastSanitizeRedacted = false;
            return input;
        }

        if (ShouldRedact)
        {
            LastSanitizeRedacted = true;
            return "[REDACTED]";
        }

        LastSanitizeRedacted = false;
        return input;
    }
}