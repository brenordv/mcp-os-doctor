using Microsoft.Extensions.Logging;

namespace McpOsDoctor.Server.Logging;

/// <summary>
/// Logger instance that formats log entries as structured JSON and delegates writing to <see cref="FileLoggerWriter"/>.
/// </summary>
public sealed class FileLogger(string categoryName, FileLoggerWriter writer) : ILogger
{
    /// <inheritdoc />
    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        return NullScope.Instance;
    }

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }

    /// <inheritdoc />
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var message = formatter(state, exception);
        var exceptionText = exception?.ToString();

        writer.WriteEntry(new LogFileEntry
        {
            Timestamp = DateTimeOffset.UtcNow,
            Level = logLevel.ToString(),
            Category = categoryName,
            Message = message,
            Exception = exceptionText
        });
    }

    /// <summary>
    /// No-op disposable for scope support.
    /// </summary>
    private sealed class NullScope : IDisposable
    {
        /// <summary>
        /// Singleton instance of the no-op scope.
        /// </summary>
        public static readonly NullScope Instance = new();

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}