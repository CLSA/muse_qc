using Microsoft.Extensions.Logging;
using MuseQCApp.Constants;
using MuseQCApp.Helpers;
using MuseQCApp.Interfaces;

namespace MuseQCApp.Modules;

public class SiteLookup : ISiteLookup
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
    public SiteLookup(ConfigHelper configHelper, ILogger logging)
    {
        ConfigHelper = configHelper;
        Logging = logging;
    }

    #endregion

    #region Implemented interface methods

    public SiteLocation LookupUsingWestonID(string westonId)
    {
        throw new NotImplementedException();
    }

    #endregion
}
