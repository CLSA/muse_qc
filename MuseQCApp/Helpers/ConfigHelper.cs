using Microsoft.Extensions.Configuration;

namespace MuseQCApp.Helpers;

/// <summary>
/// A helper with methods to access data from the configuration settings
/// </summary>
public class ConfigHelper
{
    /// <summary>
    /// The configuration settings
    /// </summary>
    public IConfiguration Configuration { get; init; }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="config">The configuration settings</param>
    public ConfigHelper(IConfiguration config)
    {
        Configuration = config;        
    }
}
