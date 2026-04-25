using System.Text.RegularExpressions;
using McpOsDoctor.Core.Interfaces;

namespace McpOsDoctor.Platform.Windows;

/// <summary>
/// Sanitizes string output by redacting known secret patterns before sending to the AI client.
/// Uses compiled regular expressions for efficient repeated matching.
/// </summary>
public sealed partial class OutputSanitizer : IOutputSanitizer
{
    private const string Redacted = "[REDACTED]";

    private static readonly Regex[] SecretPatterns =
    [
        // Connection string passwords: password=xxx or pwd=xxx
        PasswordPattern1Regex(),

        // Command-line secrets: -p value, --password value, --token value, etc.
        PasswordPattern2Regex(),

        // Bearer tokens
        BearerTokenRegex(),

        // AWS access key IDs
        AwsAccessKeyRegex(),

        // Generic key=value where key contains secret-related words
        GenericSecretKeyValueRegex()
    ];

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

        var redacted = false;
        var result = input;

        foreach (var pattern in SecretPatterns)
        {
            var replaced = pattern.Replace(result, match =>
            {
                redacted = true;
                return Redacted;
            });
            result = replaced;
        }

        LastSanitizeRedacted = redacted;
        return result;
    }

    [GeneratedRegex(@"(?i)(password|pwd)=[^;]+", RegexOptions.Compiled, "en-US")]
    private static partial Regex PasswordPattern1Regex();

    [GeneratedRegex(@"(?i)(-p|--password|--token|--secret|--key|--api-key)\s+\S+", RegexOptions.Compiled, "en-US")]
    private static partial Regex PasswordPattern2Regex();

    [GeneratedRegex(@"Bearer\s+[A-Za-z0-9\-._~+/]+=*", RegexOptions.Compiled)]
    private static partial Regex BearerTokenRegex();

    [GeneratedRegex(@"AKIA[0-9A-Z]{16}", RegexOptions.Compiled)]
    private static partial Regex AwsAccessKeyRegex();

    [GeneratedRegex(@"(?i)(secret|token|password|apikey)[""']?\s*[:=]\s*[""']?[^\s;,""']+", RegexOptions.Compiled, "en-US")]
    private static partial Regex GenericSecretKeyValueRegex();
}