﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MuseQCApp.Helpers;

/// <summary>
/// A helper with methods to access data from the configuration settings
/// </summary>
public class ConfigHelper
{
    #region private properties

    /// <summary>
    /// The configuration settings
    /// </summary>
    private IConfiguration Configuration { get; init; }


    /// <summary>
    /// The logger to use
    /// </summary>
    private ILogger Logging { get; init; }

    #endregion

    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="config">The configuration settings</param>
    public ConfigHelper(IConfiguration config, ILogger logging)
    {
        Configuration = config;
        Logging = logging;
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Get the full path to the google bucket from configuration
    /// </summary>
    /// <returns>The full path to the google bucket</returns>
    public string? GetGoogleBucketPath()
    {
        return GetStringFromConfig("GoogleBucketPath");
    }

    /// <summary>
    /// Get the full path for where to store the command line output of files 
    /// located in the google bucket
    /// </summary>
    /// <returns>The full path</returns>
    public string? GetFilesInBucketPath()
    {
        string? fileName = GetStringFromConfig("FilesInBucketFileName");
        if (fileName is null)
        {
            return null;
        }

        string appName = AppDomain.CurrentDomain.FriendlyName;
        string partialDirPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(partialDirPath, appName, fileName);
    }

    /// <summary>
    /// Get the full path to the folder where edf files downloaded from the google bucket
    /// should be stored
    /// </summary>
    /// <returns>The full path</returns>
    public string? GetEdfStorageFolderPath()
    {
        string? folderName = GetStringFromConfig("EdfStorageFolderName");
        if (folderName is null)
        {
            Logging.LogWarning("Null edf storage directory returned from config");
            return null;
        }

        string appName = AppDomain.CurrentDomain.FriendlyName;
        string partialDirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        string? edfStorageFolder = Path.Combine(partialDirPath, appName, folderName);

        // ensure the edf storage folder is created
        if (Directory.Exists(edfStorageFolder) == false)
        {
            Logging.LogWarning($"Creating edf storage directory at {edfStorageFolder}");
            Directory.CreateDirectory(edfStorageFolder);
        }
        return Path.Combine(partialDirPath, appName, folderName);
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Gets a string value from the configuration
    /// </summary>
    /// <param name="configKey">The config key</param>
    /// <returns>A string of the value or null if not found</returns>
    private string? GetStringFromConfig(string configKey)
    {
        string? val = Configuration.GetValue<string>(configKey);
        if (val is null)
        {
            Logging.LogWarning($"The key \"{configKey})\" does not exist in the configuration");
        }
        return val;
    }

    #endregion
}
