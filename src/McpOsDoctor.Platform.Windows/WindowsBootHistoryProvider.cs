using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Runtime.CompilerServices;
using McpOsDoctor.Core.Enums;
using McpOsDoctor.Core.Errors;
using McpOsDoctor.Core.Filters;
using McpOsDoctor.Core.Interfaces;
using McpOsDoctor.Core.Models;

namespace McpOsDoctor.Platform.Windows;

/// <summary>
/// Windows implementation of <see cref="IBootHistoryProvider"/> that queries the System
/// event log for boot, shutdown, sleep, and wake events using the Windows Event Log API.
/// </summary>
public sealed class WindowsBootHistoryProvider : IBootHistoryProvider
{
    // Event IDs we care about, all from the "System" log:
    // 6005 - Event Log Service started (= boot)
    // 6006 - Event Log Service stopped (= clean shutdown)
    // 6008 - Unexpected shutdown
    // 1074 - User-initiated shutdown/restart
    // 41   - Kernel-Power: unexpected shutdown / BSOD
    // 42   - Kernel-Power: sleep
    // 107  - Kernel-Power: wake

    private const string LogName = "System";

    /// <inheritdoc />
    public async IAsyncEnumerable<BootEvent> GetBootEventsAsync(
        TimeWindowFilter filter,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var xpathQuery = BuildXPathQuery(filter.From, filter.To);

        var reader = EnsureGetEventLogReader(xpathQuery);

        using (reader)
        {
            var count = 0;
            while (count < filter.MaxResults)
            {
                cancellationToken.ThrowIfCancellationRequested();

                EventRecord record;
                try
                {
                    record = reader.ReadEvent();
                }
                catch (Exception ex)
                {
                    throw McpOsDoctorException.PlatformError(
                        $"Error reading boot history events: {ex.Message}", ex);
                }

                if (record is null)
                {
                    break;
                }

                using (record)
                {
                    var bootType = ClassifyEvent(record);
                    if (bootType is null)
                    {
                        continue;
                    }

                    var timestamp = record.TimeCreated.HasValue
                        ? new DateTimeOffset(record.TimeCreated.Value.ToUniversalTime(), TimeSpan.Zero)
                        : DateTimeOffset.UtcNow;

                    var bootEvent = new BootEvent
                    {
                        Timestamp = timestamp,
                        Type = bootType.Value
                    };

                    count++;
                    yield return bootEvent;
                }
            }
        }

        // Force async state machine creation
        await Task.CompletedTask;
    }

    private static EventLogReader EnsureGetEventLogReader(string xpathQuery)
    {
        EventLogReader reader;
        try
        {
            var query = new EventLogQuery(LogName, PathType.LogName, xpathQuery)
            {
                ReverseDirection = true
            };
            reader = new EventLogReader(query);
        }
        catch (EventLogNotFoundException)
        {
            throw McpOsDoctorException.SourceNotFound(LogName);
        }
        catch (Exception ex) when (ex is not McpOsDoctorException)
        {
            throw McpOsDoctorException.PlatformError(
                $"Failed to open System event log for boot history: {ex.Message}", ex);
        }

        return reader;
    }

    private static string BuildXPathQuery(DateTimeOffset from, DateTimeOffset to)
    {
        var fromUtc = from.UtcDateTime.ToString("o", CultureInfo.InvariantCulture);
        var toUtc = to.UtcDateTime.ToString("o", CultureInfo.InvariantCulture);

        // Match events from "EventLog" source (6005, 6006, 6008),
        // "User32" source (1074), and "Microsoft-Windows-Kernel-Power" (41, 42, 107)
        return "*[System[" +
            $"TimeCreated[@SystemTime>='{fromUtc}' and @SystemTime<='{toUtc}'] and " +
            "(" +
                "(Provider[@Name='EventLog'] and (EventID=6005 or EventID=6006 or EventID=6008)) or " +
                "(Provider[@Name='User32'] and EventID=1074) or " +
                "(Provider[@Name='Microsoft-Windows-Kernel-Power'] and (EventID=41 or EventID=42 or EventID=107))" +
            ")" +
            "]]";
    }

    private static BootType? ClassifyEvent(EventRecord record)
    {
        var provider = record.ProviderName;
        var eventId = record.Id;

        // EventLog provider events
        if (string.Equals(provider, "EventLog", StringComparison.OrdinalIgnoreCase))
        {
            return eventId switch
            {
                6005 => BootType.Normal,             // Event Log Service started = boot
                6006 => BootType.Shutdown,            // Event Log Service stopped = clean shutdown
                6008 => BootType.UnexpectedShutdown,  // Unexpected shutdown
                _ => null
            };
        }

        // User32 events
        if (string.Equals(provider, "User32", StringComparison.OrdinalIgnoreCase))
        {
            return eventId switch
            {
                1074 => BootType.Shutdown, // User-initiated shutdown/restart
                _ => null
            };
        }

        // Kernel-Power events
        if (string.Equals(provider, "Microsoft-Windows-Kernel-Power", StringComparison.OrdinalIgnoreCase))
        {
            return eventId switch
            {
                41 => BootType.BlueScreen,  // Unexpected shutdown / BSOD
                42 => BootType.Sleep,       // System entering sleep
                107 => BootType.Wake,       // System waking from sleep
                _ => null
            };
        }

        return null;
    }
}