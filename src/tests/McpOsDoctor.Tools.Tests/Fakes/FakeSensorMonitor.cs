using System;
using System.Collections.Generic;
using McpOsDoctor.Core.Enums;
using McpOsDoctor.Core.Errors;
using McpOsDoctor.Core.Interfaces;
using McpOsDoctor.Core.Models;

namespace McpOsDoctor.Tools.Tests.Fakes;

/// <summary>
/// Fake implementation of <see cref="ISensorMonitor"/> for unit testing.
/// Simulates start/stop lifecycle and returns preconfigured results.
/// </summary>
public sealed class FakeSensorMonitor : ISensorMonitor
{
    /// <inheritdoc />
    public MonitoringState State { get; set; } = MonitoringState.Stopped;

    /// <inheritdoc />
    public DateTimeOffset? StartedAt { get; set; }

    /// <inheritdoc />
    public int IntervalSeconds { get; set; }

    /// <inheritdoc />
    public int SnapshotCount { get; set; }

    /// <summary>
    /// The hardware summaries returned by <see cref="GetResults"/>.
    /// </summary>
    public IReadOnlyList<HardwareSensorSummary> HardwareSummaries { get; init; } = [];

    /// <summary>
    /// When true, <see cref="Start"/> throws to simulate an already-running monitor.
    /// </summary>
    public bool SimulateAlreadyRunning { get; init; }

    /// <inheritdoc />
    public void Start(int intervalSeconds)
    {
        if (SimulateAlreadyRunning)
        {
            throw McpOsDoctorException.InvalidParameter(
                "Sensor monitoring is already running.",
                "Call stop_sensor_monitoring first.");
        }

        IntervalSeconds = intervalSeconds;
        SnapshotCount = 1;
        StartedAt = DateTimeOffset.UtcNow;
        State = MonitoringState.Running;
    }

    /// <inheritdoc />
    public void Stop()
    {
        State = MonitoringState.Stopped;
    }

    /// <inheritdoc />
    public SensorMonitoringSummary GetResults(string hardwareType, string sensorType)
    {
        return new SensorMonitoringSummary
        {
            State = State,
            StartedAt = StartedAt,
            SnapshotCount = SnapshotCount,
            IntervalSeconds = IntervalSeconds,
            Hardware = HardwareSummaries
        };
    }
}