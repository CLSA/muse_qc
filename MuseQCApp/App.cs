using Microsoft.Extensions.Logging;
using MuseQCApp.Helpers;
using MuseQCApp.Interfaces;

namespace MuseQCApp;

public class App
{
    #region private properties

    /// <summary>
    /// Helper to access configuration settings
    /// </summary>
    private ConfigHelper ConfigHelper { get; init; }

    /// <summary>
    /// The logger to use
    /// </summary>
    private ILogger Logging { get; init; }

    /// <summary>
    /// A module to use to interact with the google bucket
    /// </summary>
    private IGoogleBucket Bucket {  get; init; }

    /// <summary>
    /// A module to use for making decisions on file locations
    /// </summary>
    private IFileLocations FilePaths { get; init; }

    /// <summary>
    /// A module for running the muse quality checks
    /// </summary>
    private IMuseQualityRunner QualityRunner { get; init; }

    /// <summary>
    /// A module for creating muse quality reports
    /// </summary>
    private IQualityReport QualityReport { get; init; }

    /// <summary>
    /// A module for cleaning up the unnecessary files in the file system
    /// </summary>
    private ICleanUp Clean { get; init; }


    #endregion

    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="configHelper">Helper to access configuration settings</param>
    /// <param name="logging">The logger to use</param>
    /// <param name="bucket">A module to use to interact with the google bucket</param>
    /// <param name="filePaths">A module to use for making decisions on file locations</param>
    /// <param name="qualityRunner">A module for running the muse quality checks</param>
    /// <param name="qualityReport">A module for creating muse quality reports</param>
    /// <param name="clean">A module for cleaning up the unnecessary files in the file system</param>
    public App(ConfigHelper configHelper, ILogger logging, IGoogleBucket bucket, IFileLocations filePaths, IMuseQualityRunner qualityRunner, IQualityReport qualityReport, ICleanUp clean)
    {
        ConfigHelper = configHelper;
        Logging = logging;
        Bucket = bucket;
        FilePaths = filePaths;
        QualityRunner = qualityRunner;
        QualityReport = qualityReport;
        Clean = clean;
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Runs the quality checks application process
    /// </summary>
    public void Run()
    {
    }

    #endregion

    #region Private methods

    #endregion
}
