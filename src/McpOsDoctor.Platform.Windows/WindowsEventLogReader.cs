using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using McpOsDoctor.Core.Enums;
using McpOsDoctor.Core.Errors;
using McpOsDoctor.Core.Filters;
using McpOsDoctor.Core.Interfaces;
using McpOsDoctor.Core.Models;

namespace McpOsDoctor.Platform.Windows;

/// <summary>
/// Windows implementation of <see cref="ISystemLogReader"/> using the Windows Event Log API
/// via <see cref="System.Diagnostics.Eventing.Reader.EventLogReader"/>.
/// </summary>
public sealed class WindowsEventLogReader : ISystemLogReader
{
    private const int MaxMessageLength = 2000;

    /// <inheritdoc />
    public async IAsyncEnumerable<LogEntry> QueryAsync(
        LogQueryFilter filter,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var logName = filter.Source.Value ?? "Application";

        var xpathQuery = BuildXPathQuery(filter.From, filter.To, filter.Severity);

        var reader = EnsureGetEventLogReader(logName, xpathQuery);

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
                        $"Error reading event log: {ex.Message}", ex);
                }

                if (record is null)
                {
                    break;
                }

                using (record)
                {
                    var message = GetEventMessage(record);

                    // Apply keywords filter if specified
                    if (!string.IsNullOrEmpty(filter.Keywords)
                        && !message.Contains(filter.Keywords, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var entry = new LogEntry
                    {
                        Timestamp = record.TimeCreated.HasValue
                            ? new DateTimeOffset(record.TimeCreated.Value.ToUniversalTime(), TimeSpan.Zero)
                            : DateTimeOffset.UtcNow,
                        Severity = MapLevel(record.Level),
                        Source = record.ProviderName ?? "Unknown",
                        EventId = record.Id,
                        Message = Truncate(message, MaxMessageLength),
                        PlatformSpecific = new Dictionary<string, object>
                        {
                            ["LogName"] = record.LogName ?? logName,
                            ["Keywords"] = record.Keywords?.ToString(CultureInfo.InvariantCulture) ?? string.Empty
                        }
                    };

                    count++;
                    yield return entry;
                }
            }
        }

        // Force async state machine creation to satisfy IAsyncEnumerable contract
        await Task.CompletedTask;
    }

    private static EventLogReader EnsureGetEventLogReader(string logName, string xpathQuery)
    {
        EventLogReader reader;
        try
        {
            var query = new EventLogQuery(logName, PathType.LogName, xpathQuery);
            reader = new EventLogReader(query);
        }
        catch (EventLogNotFoundException)
        {
            throw McpOsDoctorException.SourceNotFound(logName);
        }
        catch (Exception ex) when (ex is not McpOsDoctorException)
        {
            throw McpOsDoctorException.PlatformError(
                $"Failed to open event log '{logName}': {ex.Message}", ex);
        }

        return reader;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<LogSourceInfo>> ListSourcesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var session = EventLogSession.GlobalSession;
            var logNames = session.GetLogNames();
            var sources = new List<LogSourceInfo>();

            foreach (var logName in logNames)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    using var config = new EventLogConfiguration(logName);
                    if (!config.IsEnabled)
                    {
                        continue;
                    }

                    var requiresElevation = logName.Equals("Security", StringComparison.OrdinalIgnoreCase)
                        || logName.Equals("Setup", StringComparison.OrdinalIgnoreCase);

                    sources.Add(new LogSourceInfo
                    {
                        Name = logName,
                        RequiresElevation = requiresElevation,
                        RecordCount = null // Record count requires opening the log, skip for performance
                    });
                }
                catch (EventLogException)
                {
                    // Skip inaccessible logs
                }
            }

            return Task.FromResult<IReadOnlyList<LogSourceInfo>>(sources.AsReadOnly());
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw McpOsDoctorException.PlatformError(
                $"Failed to enumerate event log sources: {ex.Message}", ex);
        }
    }

    private static string BuildXPathQuery(DateTimeOffset from, DateTimeOffset to, LogSeverity? severity)
    {
        var fromUtc = from.UtcDateTime.ToString("o", CultureInfo.InvariantCulture);
        var toUtc = to.UtcDateTime.ToString("o", CultureInfo.InvariantCulture);

        var conditions = new List<string>
        {
            $"TimeCreated[@SystemTime>='{fromUtc}' and @SystemTime<='{toUtc}']"
        };

        if (severity.HasValue)
        {
            var levels = GetLevelsAtOrAbove(severity.Value);
            if (levels.Count == 1)
            {
                conditions.Add($"Level={levels[0]}");
            }
            else
            {
                var levelCondition = string.Join(" or ", levels.Select(l => $"Level={l}"));
                conditions.Add($"({levelCondition})");
            }
        }

        var systemConditions = string.Join(" and ", conditions);
        return $"*[System[{systemConditions}]]";
    }

    private static IList<int> GetLevelsAtOrAbove(LogSeverity severity)
    {
        // Windows event levels: 0=LogAlways, 1=Critical, 2=Error, 3=Warning, 4=Informational, 5=Verbose
        // Lower number = higher severity (except 0 which is LogAlways)
        var levels = new List<int>();

        switch (severity)
        {
            case LogSeverity.Verbose:
                levels.Add(5); // Verbose
                levels.Add(4); // Informational
                levels.Add(0); // LogAlways
                levels.Add(3); // Warning
                levels.Add(2); // Error
                levels.Add(1); // Critical
                break;
            case LogSeverity.Information:
                levels.Add(4); // Informational
                levels.Add(0); // LogAlways
                levels.Add(3); // Warning
                levels.Add(2); // Error
                levels.Add(1); // Critical
                break;
            case LogSeverity.Warning:
                levels.Add(3); // Warning
                levels.Add(2); // Error
                levels.Add(1); // Critical
                break;
            case LogSeverity.Error:
                levels.Add(2); // Error
                levels.Add(1); // Critical
                break;
            case LogSeverity.Critical:
                levels.Add(1); // Critical
                break;
        }

        return levels;
    }

    private static LogSeverity MapLevel(byte? level)
    {
        return level switch
        {
            0 => LogSeverity.Verbose,   // LogAlways
            1 => LogSeverity.Critical,
            2 => LogSeverity.Error,
            3 => LogSeverity.Warning,
            4 => LogSeverity.Information,
            5 => LogSeverity.Verbose,
            _ => LogSeverity.Information
        };
    }

    private static string GetEventMessage(EventRecord record)
    {
        try
        {
            return record.FormatDescription() ?? FormatFromXml(record);
        }
        catch
        {
            return FormatFromXml(record);
        }
    }

    private static string FormatFromXml(EventRecord record)
    {
        try
        {
            var xml = record.ToXml();
            var doc = XDocument.Parse(xml);
            var ns = doc.Root?.GetDefaultNamespace();
            var eventData = doc.Root?.Element(ns + "EventData");
            if (eventData is not null)
            {
                var dataValues = eventData.Elements(ns + "Data")
                    .Select(e => e.Value)
                    .Where(v => !string.IsNullOrWhiteSpace(v));
                return string.Join(" | ", dataValues);
            }

            return $"Event ID {record.Id}";
        }
        catch
        {
            return $"Event ID {record.Id}";
        }
    }

    private static string Truncate(string value, int maxLength)
    {
        return string.IsNullOrWhiteSpace(value) || value.Length <= maxLength
            ? value ?? string.Empty
            : string.Concat(value.AsSpan(0, maxLength - 3), "...");
    }
}