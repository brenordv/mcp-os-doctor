using McpOsDoctor.Core.Models;

namespace McpOsDoctor.Core.Attributes;

/// <summary>
/// Attribute that provides information about a tool's capabilities.'
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ToolCapabilityInfoAttribute : Attribute
{
    /// <summary>
    /// Tool capability information.
    /// </summary>
    public ToolCapabilityInfo ToolCapabilityInfo { get; init; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="name">The tool name as used in MCP tool calls</param>
    /// <param name="available">Whether the tool is available on this platform</param>
    /// <param name="requiresElevation">Whether the tool requires elevated privileges to execute</param>
    /// <param name="typicalLatencyMs">Note explaining what elevation provides for this tool</param>
    /// <param name="elevationNote">Typical latency in milliseconds for this tool</param>
    /// <param name="parameterHints">Hint strings describing each parameter's purpose and defaults</param>
    public ToolCapabilityInfoAttribute(string name, bool available, bool requiresElevation, int typicalLatencyMs,
        string elevationNote = null, string[] parameterHints = null)
    {
        ToolCapabilityInfo = new ToolCapabilityInfo
        {
            Name = name,
            Available = available,
            RequiresElevation = requiresElevation,
            TypicalLatencyMs = typicalLatencyMs,
            ElevationNote = elevationNote,
            ParameterHints = parameterHints ?? []
        };
    }
}