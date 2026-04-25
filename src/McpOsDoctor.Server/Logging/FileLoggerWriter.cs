using McpOsDoctor.Core.Serialization;
using Newtonsoft.Json;

namespace McpOsDoctor.Server.Logging;

/// <summary>
/// Writes structured JSON log entries to rotating files.
/// Max 5 MB per file, keeps last 5 files.
/// </summary>
public sealed class FileLoggerWriter : IDisposable
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;
    private const int MaxFileCount = 5;
    private const string FilePrefix = "mcposdoctor-";
    private const string FileExtension = ".jsonl";

    private readonly string _logDirectory;
    private readonly Lock _lock = new();
    private StreamWriter _currentWriter;
    private string _currentFilePath;
    private long _currentFileSize;

    /// <summary>
    /// Initializes a new instance of <see cref="FileLoggerWriter"/> targeting the specified directory.
    /// </summary>
    /// <param name="logDirectory">Directory where log files will be created.</param>
    public FileLoggerWriter(string logDirectory)
    {
        _logDirectory = logDirectory;
        Directory.CreateDirectory(logDirectory);
        RotateIfNeeded();
    }

    /// <summary>
    /// Writes a single log entry as a JSON line to the current log file.
    /// </summary>
    /// <param name="entry">The log entry to write.</param>
    public void WriteEntry(LogFileEntry entry)
    {
        var json = JsonConvert.SerializeObject(entry, JsonSettings.Default);

        lock (_lock)
        {
            RotateIfNeeded();

            if (_currentWriter is null)
            {
                return;
            }

            _currentWriter.WriteLine(json);
            _currentWriter.Flush();
            _currentFileSize += json.Length + Environment.NewLine.Length;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        lock (_lock)
        {
            _currentWriter?.Dispose();
            _currentWriter = null;
        }
    }

    private void RotateIfNeeded()
    {
        if (_currentWriter is not null && _currentFileSize < MaxFileSizeBytes)
        {
            return;
        }

        _currentWriter?.Dispose();

        CleanOldFiles();

        var fileName = $"{FilePrefix}{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}{FileExtension}";
        _currentFilePath = Path.Combine(_logDirectory, fileName);
        _currentWriter = new StreamWriter(_currentFilePath, append: true);
        _currentFileSize = new FileInfo(_currentFilePath).Length;
    }

    private void CleanOldFiles()
    {
        try
        {
            var files = Directory.GetFiles(_logDirectory, $"{FilePrefix}*{FileExtension}");
            if (files.Length < MaxFileCount)
            {
                return;
            }

            Array.Sort(files);
            var toDelete = files.Length - MaxFileCount + 1;
            for (var i = 0; i < toDelete; i++)
            {
                File.Delete(files[i]);
            }
        }
        catch
        {
            // Log file cleanup is best-effort — don't crash the server
        }
    }
}