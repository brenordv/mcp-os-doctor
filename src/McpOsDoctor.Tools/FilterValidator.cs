using McpOsDoctor.Core.Errors;

namespace McpOsDoctor.Tools;

/// <summary>
/// Validates and normalizes filter parameters for tool handlers.
/// </summary>
public static partial class FilterValidator
{
    /// <summary>
    /// Maximum allowed length for a keywords' parameter.
    /// </summary>
    public const int MaxKeywordsLength = 500;

    /// <summary>
    /// Clamps a requested max results value to a valid range.
    /// </summary>
    /// <param name="requested">The requested maximum, or null for the default.</param>
    /// <param name="defaultMax">Default value when no request is made.</param>
    /// <param name="hardMax">Absolute ceiling that cannot be exceeded.</param>
    /// <returns>The clamped result count.</returns>
    public static int ClampMaxResults(int? requested, int defaultMax, int hardMax)
    {
        return requested is null
            ? defaultMax
            : Math.Clamp(requested.Value, 1, hardMax);
    }

    /// <summary>
    /// Validates that a keyword string is within length limits.
    /// </summary>
    /// <param name="keywords">The keyword string to validate.</param>
    /// <exception cref="McpOsDoctorException">Thrown when the keyword string is too long.</exception>
    public static void ValidateKeywords(string keywords)
    {
        if (string.IsNullOrEmpty(keywords))
        {
            return;
        }

        if (keywords.Length > MaxKeywordsLength)
        {
            throw McpOsDoctorException.InvalidParameter(
                $"Keywords string exceeds maximum length of {MaxKeywordsLength} characters.",
                "Shorten the search keywords.");
        }
    }
}