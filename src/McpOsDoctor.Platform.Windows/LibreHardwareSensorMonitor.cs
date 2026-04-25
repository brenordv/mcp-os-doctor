using LibreHardwareMonitor.Hardware;
using McpOsDoctor.Core.Enums;
using McpOsDoctor.Core.Errors;
using McpOsDoctor.Core.Interfaces;
using McpOsDoctor.Core.Models;
using McpOsDoctor.Core.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace McpOsDoctor.Platform.Windows;

/// <summary>
/// Windows implementation of <see cref="ISensorMonitor"/> using LibreHardwareMonitorLib.
/// Polls hardware sensors on a timer and persists snapshots to a JSONL file for crash recovery.
/// </summary>
public sealed class LibreHardwareSensorMonitor(ILogger<LibreHardwareSensorMonitor> logger) : ISensorMonitor, IDisposable
{
    private static readonly string DataDirectory = Path.Combine(Path.GetTempPath(), "mcp-os-doctor");
    private static readonly string DataFilePath = Path.Combine(DataDirectory, "sensor-monitoring.jsonl");

    private const int MinIntervalSeconds = 1;
    private const int MaxIntervalSeconds = 3600;
    private const int StopTimeoutSeconds = 5;

    private readonly Lock _lock = new();

    private Computer _computer;
    private CancellationTokenSource _cts;
    private Task _monitorTask;

    /// <inheritdoc />
    public MonitoringState State { get; private set; } = MonitoringState.Stopped;

    /// <inheritdoc />
    public DateTimeOffset? StartedAt { get; private set; }

    /// <inheritdoc />
    public int IntervalSeconds { get; private set; }

    /// <inheritdoc />
    public int SnapshotCount { get; private set; }

    /// <inheritdoc />
    public void Start(int intervalSeconds)
    {
        lock (_lock)
        {
            if (State == MonitoringState.Running)
            {
                throw McpOsDoctorException.InvalidParameter(
                    "Sensor monitoring is already running.",
                    "Call stop_sensor_monitoring first, or use get_sensor_data to retrieve current results.");
            }

            if (intervalSeconds < MinIntervalSeconds || intervalSeconds > MaxIntervalSeconds)
            {
                throw McpOsDoctorException.InvalidParameter(
                    $"intervalSeconds must be between {MinIntervalSeconds} and {MaxIntervalSeconds}.",
                    "Use a value like 5 for frequent sampling or 60 for infrequent.");
            }

            IntervalSeconds = intervalSeconds;
            SnapshotCount = 0;

            Directory.CreateDirectory(DataDirectory);
            if (File.Exists(DataFilePath))
            {
                File.Delete(DataFilePath);
            }

            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMotherboardEnabled = true,
                IsMemoryEnabled = true,
                IsStorageEnabled = true,
                IsNetworkEnabled = true,
                IsBatteryEnabled = true,
                IsControllerEnabled = true
            };
            _computer.Open();

            TakeSnapshot();

            StartedAt = DateTimeOffset.UtcNow;
            State = MonitoringState.Running;

            _cts = new CancellationTokenSource();
            _monitorTask = MonitorLoopAsync(_cts.Token);

            logger.LogInformation(
                "Sensor monitoring started — Interval: {IntervalSeconds}s",
                intervalSeconds);
        }
    }

    /// <inheritdoc />
    public void Stop()
    {
        CancellationTokenSource cts;
        Task monitorTask;

        lock (_lock)
        {
            if (State != MonitoringState.Running)
            {
                return;
            }

            cts = _cts;
            monitorTask = _monitorTask;
            State = MonitoringState.Stopped;
        }

        cts?.Cancel();

        try
        {
            monitorTask?.Wait(TimeSpan.FromSeconds(StopTimeoutSeconds));
        }
        catch (AggregateException)
        {
            // Expected when the monitor loop is cancelled.
        }

        lock (_lock)
        {
            try
            {
                _computer?.Close();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error closing hardware monitor");
            }

            _computer = null;
            _cts?.Dispose();
            _cts = null;
            _monitorTask = null;

            logger.LogInformation(
                "Sensor monitoring stopped — Snapshots: {SnapshotCount}",
                SnapshotCount);
        }
    }

    /// <inheritdoc />
    public SensorMonitoringSummary GetResults(string hardwareType, string sensorType)
    {
        var hardwareFilter = ParseEnum<HardwareCategory>(hardwareType, "hardwareType",
            "cpu, gpu, motherboard, memory, storage, network, cooler, battery, psu, embeddedController");
        var sensorFilter = ParseEnum<SensorCategory>(sensorType, "sensorType",
            "temperature, fan, voltage, clock, load, power, data, throughput, current, level, control");

        List<SensorSnapshot> snapshots;
        lock (_lock)
        {
            snapshots = ReadSnapshots();
        }

        return new SensorMonitoringSummary
        {
            State = State,
            StartedAt = StartedAt,
            SnapshotCount = snapshots.Count,
            IntervalSeconds = IntervalSeconds,
            Hardware = BuildHardwareSummaries(snapshots, hardwareFilter, sensorFilter)
        };
    }

    /// <summary>
    /// Releases all resources used by the sensor monitor.
    /// </summary>
    public void Dispose()
    {
        Stop();
    }

    private async Task MonitorLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(IntervalSeconds));
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                lock (_lock)
                {
                    TakeSnapshot();
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Sensor monitoring loop failed unexpectedly");
            lock (_lock)
            {
                State = MonitoringState.Stopped;
            }
        }
    }

    private void TakeSnapshot()
    {
        try
        {
            var groups = new List<HardwareSensorGroup>();

            foreach (var hardware in _computer.Hardware)
            {
                hardware.Update();
                foreach (var sub in hardware.SubHardware)
                {
                    sub.Update();
                }

                CollectSensors(hardware, groups);
            }

            var snapshot = new SensorSnapshot
            {
                Timestamp = DateTimeOffset.UtcNow,
                Hardware = groups
            };

            var json = JsonConvert.SerializeObject(snapshot, JsonSettings.Default);
            File.AppendAllText(DataFilePath, json + "\n");

            SnapshotCount++;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to take sensor snapshot");
        }
    }

    private static void CollectSensors(IHardware hardware, List<HardwareSensorGroup> groups)
    {
        var sensors = hardware.Sensors
                              .Where(s => s.Value.HasValue)
                              .Select(s => new SensorReading
                              {
                                  Name = s.Name,
                                  Category = MapSensorType(s.SensorType),
                                  Value = s.Value.Value
                              })
                              .ToList();

        if (sensors.Count > 0)
        {
            groups.Add(new HardwareSensorGroup
            {
                Name = hardware.Name,
                Category = MapHardwareType(hardware.HardwareType),
                Sensors = sensors
            });
        }

        foreach (var sub in hardware.SubHardware)
        {
            CollectSensors(sub, groups);
        }
    }

    private static List<SensorSnapshot> ReadSnapshots()
    {
        if (!File.Exists(DataFilePath))
        {
            return [];
        }

        var snapshots = new List<SensorSnapshot>();
        foreach (var line in File.ReadLines(DataFilePath))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            try
            {
                var snapshot = JsonConvert.DeserializeObject<SensorSnapshot>(line, JsonSettings.Default);
                if (snapshot is not null)
                {
                    snapshots.Add(snapshot);
                }
            }
            catch (JsonException)
            {
                // Skip malformed lines (e.g., partial write from a crash).
            }
        }

        return snapshots;
    }

    private static IReadOnlyList<HardwareSensorSummary> BuildHardwareSummaries(
        List<SensorSnapshot> snapshots,
        HardwareCategory? hardwareFilter,
        SensorCategory? sensorFilter)
    {
        if (snapshots.Count == 0)
        {
            return [];
        }

        return snapshots
               .SelectMany(s => s.Hardware)
               .Where(h => hardwareFilter is null || h.Category == hardwareFilter)
               .SelectMany(h => h.Sensors
                   .Where(s => sensorFilter is null || s.Category == sensorFilter)
                   .Select(s => new { HardwareName = h.Name, HardwareCategory = h.Category, Sensor = s }))
               .GroupBy(r => (r.HardwareName, r.HardwareCategory))
               .Select(hardwareGroup => new HardwareSensorSummary
               {
                   Name = hardwareGroup.Key.HardwareName,
                   Category = hardwareGroup.Key.HardwareCategory,
                   Sensors = hardwareGroup
                             .GroupBy(r => (r.Sensor.Name, r.Sensor.Category))
                             .Select(sensorGroup => new SensorStatistics
                             {
                                 Name = sensorGroup.Key.Name,
                                 Category = sensorGroup.Key.Category,
                                 Current = sensorGroup.Last().Sensor.Value,
                                 Min = sensorGroup.Min(r => r.Sensor.Value),
                                 Max = sensorGroup.Max(r => r.Sensor.Value),
                                 Average = (float)sensorGroup.Average(r => r.Sensor.Value)
                             })
                             .ToList()
               })
               .ToList();
    }

    private static HardwareCategory MapHardwareType(HardwareType type) => type switch
    {
        HardwareType.Cpu => HardwareCategory.Cpu,
        HardwareType.GpuNvidia or HardwareType.GpuAmd or HardwareType.GpuIntel => HardwareCategory.Gpu,
        HardwareType.Motherboard or HardwareType.SuperIO => HardwareCategory.Motherboard,
        HardwareType.Memory => HardwareCategory.Memory,
        HardwareType.Storage => HardwareCategory.Storage,
        HardwareType.Network => HardwareCategory.Network,
        HardwareType.Cooler => HardwareCategory.Cooler,
        HardwareType.Battery => HardwareCategory.Battery,
        HardwareType.Psu => HardwareCategory.Psu,
        HardwareType.EmbeddedController => HardwareCategory.EmbeddedController,
        _ => HardwareCategory.Cpu
    };

    private static SensorCategory MapSensorType(SensorType type) => type switch
    {
        SensorType.Temperature => SensorCategory.Temperature,
        SensorType.Fan => SensorCategory.Fan,
        SensorType.Voltage => SensorCategory.Voltage,
        SensorType.Clock => SensorCategory.Clock,
        SensorType.Load => SensorCategory.Load,
        SensorType.Power => SensorCategory.Power,
        SensorType.Data => SensorCategory.Data,
        SensorType.SmallData => SensorCategory.SmallData,
        SensorType.Throughput => SensorCategory.Throughput,
        SensorType.Current => SensorCategory.Current,
        SensorType.Level => SensorCategory.Level,
        SensorType.Control => SensorCategory.Control,
        SensorType.Energy => SensorCategory.Energy,
        SensorType.Noise => SensorCategory.Noise,
        SensorType.Frequency => SensorCategory.Frequency,
        SensorType.Factor => SensorCategory.Factor,
        SensorType.Flow => SensorCategory.Flow,
        SensorType.TimeSpan => SensorCategory.TimeSpan,
        _ => SensorCategory.Temperature
    };

    private static TEnum? ParseEnum<TEnum>(string value, string parameterName, string validValues) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (Enum.TryParse<TEnum>(value, ignoreCase: true, out var result))
        {
            return result;
        }

        throw McpOsDoctorException.InvalidParameter(
            $"Invalid {parameterName}: '{value}'.",
            $"Valid values: {validValues}");
    }
}