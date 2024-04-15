using Microsoft.Extensions.Configuration;
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
    public List<string> GetGoogleBucketPath()
    {
        List<string> bucketPaths = GetListFromConfig<string>("GoogleBucketPath");
        return bucketPaths is not null ? bucketPaths.ToList() : new List<string>();
    }

    /// <summary>
    /// Get the full path without extension for where to store the command line output of files 
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
        return edfStorageFolder;
    }

    /// <summary>
    /// Get the full path to the sub folder where edf files downloaded from the google bucket
    /// that could not be processed should be stored
    /// </summary>
    /// <returns>The full path</returns>
    public string? GetEdfProblemStorageSubFolderPath()
    {
        string? problemSubFolderName = GetStringFromConfig("EdfProblemStorageSubFolderName");
        if (problemSubFolderName is null)
        {
            Logging.LogWarning("Null edf problem storage sub directory returned from config");
            return null;
        }

        string? edfStorageFolder = GetEdfStorageFolderPath();
        if(edfStorageFolder is null)
        {
            return null;
        }

        string? edfProblemStorageFolder = Path.Combine(edfStorageFolder, problemSubFolderName);

        // ensure the edf storage folder is created
        if (Directory.Exists(edfProblemStorageFolder) == false)
        {
            Logging.LogWarning($"Creating edf problem storage directory at {edfProblemStorageFolder}");
            Directory.CreateDirectory(edfProblemStorageFolder);
        }
        return edfProblemStorageFolder;
    }

    /// <summary>
    /// Get the full path to the folder where output files should be stored 
    /// after running the muse quality script
    /// </summary>
    /// <returns>The full path</returns>
    public string? GetOutputStorageFolderPath()
    {
        string? folderName = GetStringFromConfig("OutputDataStorageFolderName");
        if (folderName is null)
        {
            Logging.LogWarning("Null output storage directory returned from config");
            return null;
        }

        string appName = AppDomain.CurrentDomain.FriendlyName;
        string partialDirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        string? outputStorageFolder = Path.Combine(partialDirPath, appName, folderName);

        // ensure the edf storage folder is created
        if (Directory.Exists(outputStorageFolder) == false)
        {
            Logging.LogWarning($"Creating edf storage directory at {outputStorageFolder}");
            Directory.CreateDirectory(outputStorageFolder);
        }
        return outputStorageFolder;
    }

    /// <summary>
    /// Get the full path to the folder where output jpg files should be stored 
    /// after running the muse quality script
    /// </summary>
    /// <returns>The full path</returns>
    public string? GetJpgStorageFolderPath()
    {
        string? folderName = GetStringFromConfig("JpgStorageFolderName");
        if (folderName is null)
        {
            Logging.LogWarning("Null jpg storage directory returned from config");
            return null;
        }

        string? outputStorageFolder = GetOutputStorageFolderPath();
        if (outputStorageFolder is null) return null;

        string jpgStorageFolder = Path.Combine(outputStorageFolder, folderName);

        // ensure the edf storage folder is created
        if (Directory.Exists(jpgStorageFolder) == false)
        {
            Logging.LogWarning($"Creating edf storage directory at {jpgStorageFolder}");
            Directory.CreateDirectory(jpgStorageFolder);
        }
        return jpgStorageFolder;
    }

    /// <summary>
    /// Get the full path to the site lookup table csv
    /// </summary>
    /// <returns>The path if the value can be retrieved and is formatted correctly, otherwise null</returns>
    public string? GetSiteLookupTableCsvPath()
    {
        // get path from appsettings
        string? path = GetStringFromConfig("SiteLookupTableCsvPath");
        
        // ensure path is not null
        if (path is null)
        {
            Logging.LogWarning("Null Site Lookup Table Csv Path returned from config");
            return null;
        }

        // ensure path is csv
        if(path.ToLower().EndsWith(".csv") == false)
        {
            Logging.LogWarning($"Site Lookup Table Csv Path returned from config is not a csv. Path: {path}");
            return null;
        }

        // ensure file exists
        if (File.Exists(path) == false)
        {
            Logging.LogWarning($"Site Lookup Table Csv Path returned from config does not exist. Path: {path}");
            return null;
        }

        // return path is all checks passed
        return path;
    }

    /// <summary>
    /// Get the full path to the Quality Report Csv Folder
    /// </summary>
    /// <returns>The path if the value can be retrieved, otherwise null</returns>
    public string? GetQualityReportFolderPath()
    {
        // get path from appsettings
        string? path = GetStringFromConfig("QualityReportFolderPath");

        // ensure path is not null
        if (path is null)
        {
            Logging.LogWarning("Null Quality Report Csv Folder Path returned from config");
            return null;
        }
        return path; 
    }

    /// <summary>
    /// Get the full path to the python exe path
    /// </summary>
    /// <returns>The path if the value can be retrieved, otherwise null</returns>
    public string? GetPythonExePath()
    {
        // get path from appsettings
        string? path = GetStringFromConfig("PythonExePath");

        // ensure path is not null
        if (path is null)
        {
            Logging.LogWarning("Null python exe Path returned from config");
            return null;
        }
        return path;
    }

    /// <summary>
    /// Get the user credentials to use when making requests to cenozo
    /// </summary>
    /// <returns>The password</returns>
    public string? GetCenozoRequestCredentials()
    {
        return GetStringFromConfig("Cenozo:RequestUserCredentials"); ;
    }

    /// <summary>
    /// Get the URL to use when accessing information about participant sites from cenozo
    /// </summary>
    /// <returns>The password</returns>
    public string? GetCenozoSiteLookupURL()
    {
        return GetStringFromConfig("Cenozo:SiteLookupURL"); ;
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
        return GetValueFromConfig<string>(configKey);
    }

    /// <summary>
    /// Gets a value from the configuration
    /// </summary>
    /// <typeparam name="T">The type of the value to return</typeparam>
    /// <param name="configKey">The config key</param>
    /// <returns>The value or null if not found</returns>
    private T? GetValueFromConfig<T>(string configKey)
    {
        T? val = Configuration.GetValue<T>(configKey);
        if (val is null)
        {
            Logging.LogWarning($"The key \"{configKey})\" does not exist in the configuration");
        }
        return val;
    }

    /// <summary>
    /// Gets a string value from the configuration
    /// </summary>
    /// <param name="configKey">The config key</param>
    /// <returns>A string of the value or null if not found</returns>
    private List<T> GetListFromConfig<T>(string configKey)
    {
        T[]? vals = Configuration.GetSection(configKey).Get<T[]>();
        return vals is null ? new List<T>() : vals.ToList();
    }

    #endregion
}
