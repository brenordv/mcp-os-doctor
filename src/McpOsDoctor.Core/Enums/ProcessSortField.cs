namespace McpOsDoctor.Core.Enums;

/// <summary>
/// Field used to sort a process listing results.
/// </summary>
public enum ProcessSortField
{
    /// <summary>Sort by CPU usage (descending).</summary>
    Cpu = 1,

    /// <summary>Sort by memory usage (descending).</summary>
    Memory = 2,

    /// <summary>Sort alphabetically by process name.</summary>
    Name = 3
}