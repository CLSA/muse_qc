using Microsoft.Extensions.Logging;
using MuseQCApp.Helpers;
using MuseQCApp.Interfaces;
using MuseQCApp.Models;

namespace MuseQCApp.Modules;

public class FileLocations : IFileLocations
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

    #endregion

    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="configHelper">Helper to access configuration settings</param>
    /// <param name="logging">The logger to use</param>
    public FileLocations(ConfigHelper configHelper, ILogger logging)
    {
        ConfigHelper = configHelper;
        Logging = logging;
    }

    #endregion

    #region Implemented interface methods

    public void DecideFilesToDownload(Dictionary<string, object> possibleFilesDict)
    {
        throw new NotImplementedException();
    }

    public ReportInfoModel GetReportInfo(DateTime StartDate, DateTime EndDate)
    {
        throw new NotImplementedException();
    }

    public List<string> NeedsQualityCheck()
    {
        throw new NotImplementedException();
    }

    #endregion
}
