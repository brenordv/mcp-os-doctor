using McpOsDoctor.Core.Constants;
using McpOsDoctor.Core.Errors;

namespace McpOsDoctor.Core.DataTypes;

/// <summary>
/// Represents a source name object that encapsulates a string value with validation.
/// </summary>
/// <remarks>
/// This struct is designed to enforce constraints on source name values, ensuring they do not exceed a specified length.
/// It includes string-like ergonomic features such as implicit conversions and equality comparisons.
/// </remarks>
/// <seealso cref="McpOsDoctorException"/>
public readonly struct SourceName : IEquatable<SourceName>
{
    /// <summary>
    /// Maximum allowed length for a source name parameter.
    /// </summary>
    private const int MaxSourceLength = 200;

    /// <summary>
    /// The source name value.
    /// </summary>
    public string Value { get; init; }

    /// <summary>
    /// Represents a source name with validation for maximum length constraints.
    /// </summary>
    /// <remarks>
    /// The <see cref="SourceName"/> struct provides a way to encapsulate source name values within a
    /// defined maximum length, ensuring validity and adherence to constraints. This implementation
    /// includes ergonomic features like implicit conversions to and from strings, and supports
    /// equality comparisons.
    /// </remarks>
    /// <exception cref="McpOsDoctorException">
    /// Thrown when the provided source name exceeds the allowed maximum length, which is defined
    /// by <see cref="MaxSourceLength"/>.
    /// </exception>
    /// <seealso cref="McpOsDoctorException"/>
    public SourceName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Value = null;
            return;
        }

        if (value.Length > MaxSourceLength)
        {
            throw McpOsDoctorException.InvalidParameter(
                $"Service name exceeds maximum length of {MaxSourceLength} characters.",
                "Shorten the service name or pattern.");
        }

        if (!RegexPatterns.SourceNamePattern().IsMatch(value))
        {
            throw McpOsDoctorException.InvalidParameter(
                "Source name contains invalid characters. Only alphanumeric characters, hyphens, slashes, spaces, and asterisks are allowed.",
                "Use only [a-zA-Z0-9 \\-/*] characters.");
        }

        Value = value;
    }

    /// <summary>
    /// Defines an implicit conversion operator for converting a <see cref="SourceName"/> instance into a string.
    /// </summary>
    /// <remarks>
    /// This operator allows <see cref="SourceName"/> instances to be used in contexts where a string is expected,
    /// simplifying interactions with APIs or libraries that work with string representations.
    /// </remarks>
    /// <param name="e">
    /// The <see cref="SourceName"/> instance to convert. If the instance is <c>null</c>, the returned string will also be <c>null</c>.
    /// </param>
    /// <returns>
    /// A string representation of the encapsulated source name value within the <see cref="SourceName"/> instance.
    /// </returns>
    public static implicit operator string(SourceName e) => e.Value;

    /// <summary>
    /// Defines an implicit conversion from a string to a <see cref="SourceName"/> object.
    /// </summary>
    /// <remarks>
    /// This operator simplifies the creation of a <see cref="SourceName"/> object by allowing
    /// implicit conversion from a string, provided the string adheres to any constraints
    /// defined by the <see cref="SourceName"/> struct.
    /// </remarks>
    /// <param name="e">The string to be converted into a <see cref="SourceName"/> object.</param>
    /// <returns>
    /// A <see cref="SourceName"/> object initialized with the provided string value.
    /// </returns>
    /// <exception cref="McpOsDoctorException">
    /// Thrown if the provided string does not meet the constraints of a valid source name,
    /// such as exceeding the maximum allowed length.
    /// </exception>
    /// <seealso cref="SourceName"/>
    public static implicit operator SourceName(string e) => new(e);

    /// <summary>
    /// Converts the current instance of <see cref="SourceName"/> to its string representation.
    /// </summary>
    /// <remarks>
    /// This method returns the encapsulated string value of the <see cref="SourceName"/> instance.
    /// </remarks>
    /// <returns>
    /// A string that represents the value of the <see cref="SourceName"/> instance.
    /// </returns>
    public override string ToString() => Value;

    /// <summary>
    /// Determines whether the current <see cref="SourceName"/> instance is equal to another <see cref="SourceName"/> instance.
    /// </summary>
    /// <param name="other">
    /// The <see cref="SourceName"/> instance to compare with the current instance.
    /// </param>
    /// <returns>
    /// <c>true</c> if the current <see cref="SourceName"/> instance is equal to the specified instance; otherwise, <c>false</c>.
    /// </returns>
    public bool Equals(SourceName other) => Value == other.Value;

    /// <summary>
    /// Determines whether the current <see cref="SourceName"/> instance is equal to a specified object.
    /// </summary>
    /// <param name="obj">
    /// The object to compare with the current <see cref="SourceName"/> instance.
    /// </param>
    /// <returns>
    /// <c>true</c> if the specified object is a <see cref="SourceName"/> instance and is equal to the current instance; otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals(object obj) => obj is SourceName e && Equals(e);

    /// <summary>
    /// Provides a hash code for the current <see cref="SourceName"/> instance based on its encapsulated value.
    /// </summary>
    /// <remarks>
    /// This method computes a hash code for the underlying string value of the <see cref="SourceName"/>.
    /// If the value is null, a default hash code of zero is returned.
    /// </remarks>
    /// <returns>
    /// An integer hash code representing the current <see cref="SourceName"/> instance.
    /// </returns>
    public override int GetHashCode() => Value?.GetHashCode() ?? 0;

    /// <summary>
    /// Determines whether two <see cref="SourceName"/> instances are equal.
    /// </summary>
    /// <param name="a">
    /// The first <see cref="SourceName"/> instance to compare.
    /// </param>
    /// <param name="b">
    /// The second <see cref="SourceName"/> instance to compare.
    /// </param>
    /// <returns>
    /// <c>true</c> if both <see cref="SourceName"/> instances are equal; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This operator performs equality comparison based on the encapsulated string value of the <see cref="SourceName"/> instances.
    /// </remarks>
    /// <seealso cref="SourceName.Equals(SourceName)"/>
    public static bool operator ==(SourceName a, SourceName b) => a.Equals(b);

    /// <summary>
    /// Determines whether two <see cref="SourceName"/> instances are not equal.
    /// </summary>
    /// <param name="a">
    /// The first <see cref="SourceName"/> instance to compare.
    /// </param>
    /// <param name="b">
    /// The second <see cref="SourceName"/> instance to compare.
    /// </param>
    /// <returns>
    /// <c>true</c> if the two <see cref="SourceName"/> instances are not equal; otherwise, <c>false</c>.
    /// </returns>
    public static bool operator !=(SourceName a, SourceName b) => !a.Equals(b);
}