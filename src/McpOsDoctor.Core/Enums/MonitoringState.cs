namespace McpOsDoctor.Core.Enums;

/// <summary>
/// Indicates the current state of the sensor monitoring session.
/// </summary>
public enum MonitoringState
{
    /// <summary>Monitoring is not active.</summary>
    Stopped = 1,

    /// <summary>Monitoring is actively collecting sensor data.</summary>
    Running = 2
}