namespace McpOsDoctor.Core.Interfaces;

/// <summary>
/// Sanitizes string output by redacting known secret patterns before sending to the AI client.
/// </summary>
public interface IOutputSanitizer
{
    /// <summary>
    /// Redacts known secret patterns from the input string.
    /// </summary>
    /// <param name="input">The raw string to sanitize.</param>
    /// <returns>The sanitized string with secrets replaced by [REDACTED].</returns>
    string Sanitize(string input);

    /// <summary>
    /// Whether any redaction was applied during the last call to <see cref="Sanitize"/>.
    /// </summary>
    bool LastSanitizeRedacted { get; }
}