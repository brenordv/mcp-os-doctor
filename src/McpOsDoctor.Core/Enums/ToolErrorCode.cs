namespace McpOsDoctor.Core.Enums;

/// <summary>
/// Structured error codes returned by tool handlers to the AI client.
/// </summary>
public enum ToolErrorCode
{
    /// <summary>A parameter failed validation.</summary>
    InvalidParameter = 1,

    /// <summary>The requested source or resource was not found.</summary>
    SourceNotFound = 2,

    /// <summary>The operation requires elevated privileges.</summary>
    ElevationRequired = 3,

    /// <summary>An unexpected OS API failure occurred.</summary>
    PlatformError = 4,

    /// <summary>The requested tool is not available on this platform.</summary>
    NotSupported = 5
}