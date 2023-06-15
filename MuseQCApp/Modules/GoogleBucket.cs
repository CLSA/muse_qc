using Microsoft.Extensions.Logging;
using MuseQCApp.Helpers;
using MuseQCApp.Interfaces;
using System.Diagnostics;

namespace MuseQCApp.Modules;

/// <summary>
/// Methods for interacting with a google bucket cloud storage location
/// NOTE: The methods in this class require setup of GCloud, GSUtil
/// and some permissions changes to allow the default command line to 
/// execute the commands.
/// </summary>
public class GoogleBucket : IGoogleBucket
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
    public GoogleBucket(ConfigHelper configHelper, ILogger logging)
    {
        ConfigHelper = configHelper;
        Logging = logging;
    }

    #endregion

    #region Implemented interface methods

    public bool DownloadFiles(List<string> filePaths, string storageDirPath)
    {
        throw new NotImplementedException();
    }

    public List<string> GetFilePaths()
    {
        // Get paths
        string? bucketPath = ConfigHelper.GetGoogleBucketPath();
        string? outputTxtPath = ConfigHelper.GetFilesInBucketPath();
        
        // Return empty list if config returns a null path
        if(bucketPath == null || outputTxtPath == null)
        {
            return new List<string>();
        }

        // Remove the previous output file if there is one
        if (File.Exists(outputTxtPath))
        {
            Logging.LogInformation($"Removing previous output txt with path {outputTxtPath}");
            File.Delete(outputTxtPath);
        }

        // Run commandline command to get files on google bucket and store 
        // filepaths in a txt file
        Process cmd = CreateGetFilePathsProcess(bucketPath, outputTxtPath);
        Logging.LogInformation($"Querying google bucket for filepaths. Files will be stored in {outputTxtPath}");
        cmd.Start();
        cmd.WaitForExit();

        // Wait up to 60 seconds for command line to finish running command
        DateTime startTime = DateTime.Now;
        while (File.Exists(outputTxtPath) == false && (DateTime.Now - startTime).TotalSeconds < 60) {}

        // Read eeg filepaths from txt file and return as list
        return ReadEEGFilesFromOutputTxt(outputTxtPath);
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Creates a process to get the file paths of each file on the google bucket
    /// </summary>
    /// <param name="bucketPath">The path ot the google bucket</param>
    /// <param name="outputTxtPath">The location to store the output of the command</param>
    /// <returns>The process</returns>
    private Process CreateGetFilePathsProcess(string bucketPath, string outputTxtPath)
    {
        return new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false,
                Arguments = $"/c gsutil ls -l -h {bucketPath}>\"{outputTxtPath}\""
            }
        };
    }

    /// <summary>
    /// Reads the files paths stored in the output txt
    /// </summary>
    /// <param name="outputTxtPath">The path to read from</param>
    /// <returns>A list of the eeg filepaths</returns>
    private List<string> ReadEEGFilesFromOutputTxt(string outputTxtPath)
    {

        // log error if the output txt cannot be found
        if (File.Exists(outputTxtPath) == false)
        {
            Logging.LogError($"No file found with path {outputTxtPath}");
            return new List<string>();
        }

        // Read eeg file paths from output txt
        List<string> eegFiles = new();
        using (StreamReader sr = new(outputTxtPath))
        {
            while (sr.EndOfStream == false)
            {
                string? line = sr.ReadLine();
                if (line != null && line.Trim().EndsWith("eeg.edf"))
                {
                    eegFiles.Add(line.Split().Last());
                }
            }
        }

        Logging.LogInformation($"Read in all eeg files from {outputTxtPath}");
        return eegFiles;
    }

    #endregion
}
