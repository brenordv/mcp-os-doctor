using System.Diagnostics;
using System.Runtime.CompilerServices;
using McpOsDoctor.Core.Errors;
using McpOsDoctor.Core.Filters;
using McpOsDoctor.Core.Interfaces;
using McpOsDoctor.Core.Models;

namespace McpOsDoctor.Platform.Windows;

/// <summary>
/// Windows implementation of <see cref="IPerformanceReader"/> using
/// <see cref="PerformanceCounter"/> to read OS performance counters.
/// </summary>
public sealed class WindowsPerformanceReader : IPerformanceReader
{
    /// <summary>
    /// Curated default set of performance counters providing a useful system overview.
    /// Each tuple contains: (Category, Counter, Instance, Unit).
    /// </summary>
    private static readonly (string Category, string Counter, string Instance, string Unit)[] DefaultCounters =
    [
        ("Processor", "% Processor Time", "_Total", "%"),
        ("Memory", "Available MBytes", null, "MB"),
        ("Memory", "% Committed Bytes In Use", null, "%"),
        ("PhysicalDisk", "% Disk Time", "_Total", "%")
    ];

    /// <inheritdoc />
    public async IAsyncEnumerable<PerformanceMetric> GetMetricsAsync(
        MetricFilter filter,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (filter.Categories is null || filter.Categories.Count == 0)
        {
            // Return the curated default set
            foreach (var def in DefaultCounters.Take(filter.MaxResults))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var metric = ReadCounter(def.Category, def.Counter, def.Instance, def.Unit);
                if (metric is not null)
                {
                    yield return metric;
                }
            }
        }
        else
        {
            // Read counters from specified categories
            foreach (var category in filter.Categories.Take(filter.MaxResults))
            {
                cancellationToken.ThrowIfCancellationRequested();

                PerformanceCounterCategory cat;
                try
                {
                    cat = new PerformanceCounterCategory(category);
                }
                catch (Exception ex)
                {
                    throw McpOsDoctorException.PlatformError(
                        $"Performance counter category '{category}' not found: {ex.Message}", ex);
                }

                var counters = GetCountersFromCategory(cat);
                foreach (var (counter, instance, unit) in counters.Take(filter.MaxResults))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var metric = ReadCounter(category, counter, instance, unit);
                    if (metric is not null)
                    {
                        yield return metric;
                    }
                }
            }
        }

        // Force async state machine creation
        await Task.CompletedTask;
    }

    private static List<(string Counter, string Instance, string Unit)> GetCountersFromCategory(
        PerformanceCounterCategory category)
    {
        var result = new List<(string Counter, string Instance, string Unit)>();

        try
        {
            var instanceNames = category.GetInstanceNames();

            if (instanceNames.Length == 0)
            {
                // Single-instance category
                var counters = category.GetCounters();
                foreach (var counter in counters)
                {
                    using (counter)
                    {
                        result.Add((counter.CounterName, null, InferUnit(counter.CounterName)));
                    }
                }
            }
            else
            {
                // Multi-instance: pick _Total or first instance
                var instanceName = instanceNames.Contains("_Total")
                    ? "_Total"
                    : instanceNames[0];

                var counters = category.GetCounters(instanceName);
                foreach (var counter in counters)
                {
                    using (counter)
                    {
                        result.Add((counter.CounterName, instanceName, InferUnit(counter.CounterName)));
                    }
                }
            }
        }
        catch
        {
            // Skip categories that cannot be enumerated
        }

        return result;
    }

    private static PerformanceMetric ReadCounter(string category, string counter, string instance, string unit)
    {
        try
        {
            using var pc = instance is not null
                ? new PerformanceCounter(category, counter, instance, readOnly: true)
                : new PerformanceCounter(category, counter, readOnly: true);

            // First call initializes the counter; the value may be zero
            var value = pc.NextValue();

            return new PerformanceMetric
            {
                Category = category,
                Counter = counter,
                Instance = instance,
                Value = Math.Round(value, 2),
                Unit = unit,
                Timestamp = DateTimeOffset.UtcNow
            };
        }
        catch
        {
            // Counter may not exist on this locale or may require elevation
            return null;
        }
    }

    private static string InferUnit(string counterName)
    {
        if (counterName.Contains('%') || counterName.Contains("Percent", StringComparison.OrdinalIgnoreCase))
        {
            return "%";
        }

        if (counterName.Contains("MBytes", StringComparison.OrdinalIgnoreCase))
        {
            return "MB";
        }

        if (counterName.Contains("Bytes/sec", StringComparison.OrdinalIgnoreCase))
        {
            return "B/s";
        }

        if (counterName.Contains("Bytes", StringComparison.OrdinalIgnoreCase))
        {
            return "B";
        }

        return counterName.Contains("/sec", StringComparison.OrdinalIgnoreCase)
            ? "/s"
            : "";
    }
}