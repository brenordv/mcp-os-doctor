using McpOsDoctor.Server.Logging;
using Microsoft.Extensions.Logging;

namespace McpOsDoctor.Server.Extensions;

/// <summary>
/// Extension methods for configuring the MCP OS Doctor file logger.
/// </summary>
public static class LoggingExtensions
{
    /// <param name="builder">The logging builder to configure.</param>
    extension(ILoggingBuilder builder)
    {
        /// <summary>
        /// Adds the MCP OS Doctor structured file logger to the logging builder.
        /// Writes to %LOCALAPPDATA%/McpOsDoctor/logs/ on Windows.
        /// </summary>
        /// <returns>The logging builder for chaining.</returns>
        public ILoggingBuilder AddMcpOsDoctorFileLogger()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var logDirectory = Path.Combine(localAppData, "McpOsDoctor", "logs");

            builder.AddProvider(new FileLoggerProvider(logDirectory));
            builder.SetMinimumLevel(LogLevel.Information);

            return builder;
        }
    }
}