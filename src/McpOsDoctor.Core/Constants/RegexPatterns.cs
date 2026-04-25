using System.Text.RegularExpressions;

namespace McpOsDoctor.Core.Constants;

/// <summary>
/// Contains compiled regex patterns used for input validation.
/// </summary>
public static partial class RegexPatterns
{
    /// <summary>
    /// Pattern that validates source names: alphanumeric characters, spaces, hyphens, slashes, and asterisks.
    /// </summary>
    /// <returns>A compiled <see cref="Regex"/> for source name validation.</returns>
    [GeneratedRegex(@"^[a-zA-Z0-9\s\-/\*]+$")]
    public static partial Regex SourceNamePattern();
}