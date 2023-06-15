using Microsoft.Extensions.Logging;

namespace MuseQCApp.FileLogger;

/// <summary>
/// The configuration for a <see cref="FileLogger"/>
/// <summary>
public class FileLoggerConfiguration
{
    #region Public Properties

    /// <summary>
    /// The level of log that should be processed
    /// </summary>
    public LogLevel LogLevel { get; set; } = LogLevel.Trace;

    /// <summary>
    /// The path to write to
    /// </summary>
    public string FilePath { get; init; } = "";

    /// <summary>
    /// Whether to log the time as part of the message
    /// </summary>
    public bool LogTime { get; set; } = true;

    #endregion
}
