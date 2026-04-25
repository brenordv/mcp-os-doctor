using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace McpOsDoctor.Server.Logging;

/// <summary>
/// Logger provider that writes structured JSON log entries to rotating files.
/// Never writes to stdout/stderr — those are reserved for the MCP transport.
/// </summary>
/// <remarks>
/// Initializes a new instance of <see cref="FileLoggerProvider"/> with the specified log directory.
/// </remarks>
/// <param name="logDirectory">Directory where log files will be written.</param>
public sealed class FileLoggerProvider(string logDirectory) : ILoggerProvider
{
    private readonly FileLoggerWriter _writer = new(logDirectory);
    private readonly ConcurrentDictionary<string, FileLogger> _loggers = new();

    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, static (name, writer) => new FileLogger(name, writer), _writer);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _writer.Dispose();
        _loggers.Clear();
    }
}