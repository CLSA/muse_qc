using Microsoft.Extensions.Logging;
using MuseQCApp.Helpers;
using MuseQCApp.Interfaces;
using MuseQCApp.Models;
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

    public bool DownloadFiles(List<GBDownloadInfoModel> filesToDownload)
    {
        // Get the edf storage folder path
        string? edfStorageFolder = ConfigHelper.GetEdfStorageFolderPath();
        if(edfStorageFolder is null)
        {
            Logging.LogWarning("Null google edf storage path returned from config");
            return false;
        }

        // ensure the edf storage folder is created
        if(Directory.Exists(edfStorageFolder) == false)
        {
            Logging.LogWarning($"Creating edf storage directory at {edfStorageFolder}");
            Directory.CreateDirectory(edfStorageFolder);
        }

        // Get process ready
        Process cmd = CreateDownloadFilesProcess();
        cmd.Start();

        // Download files
        bool allFilesDownloaded = true;
        foreach(var gbInfo in filesToDownload)
        {
            string fileName = Path.GetFileName(gbInfo.FullFilePath).Replace(":", "");
            string fullFilepath = Path.Combine(edfStorageFolder, fileName);
            cmd.StandardInput.WriteLine($"gsutil cp \"{gbInfo.FullFilePath}\" \"{fullFilepath}\"");
            if (File.Exists(fullFilepath))
            {
                Logging.LogInformation($"Downloaded: {fullFilepath}");
            }
            else
            {
                allFilesDownloaded = false;
                Logging.LogError($"Failed to download: {gbInfo.FullFilePath}");
            }
        }
        
        // Close connection with command line
        cmd.StandardInput.Flush();
        cmd.StandardInput.Close();
        cmd.WaitForExit();

        return allFilesDownloaded;
    }

    public List<GBDownloadInfoModel> GetFilePaths()
    {
        // Get paths
        string? bucketPath = ConfigHelper.GetGoogleBucketPath();
        string? outputTxtPath = ConfigHelper.GetFilesInBucketPath();
        
        // Return empty list if config returns a null path
        if(bucketPath == null || outputTxtPath == null)
        {
            Logging.LogWarning("Null google bucket path returned from config");
            return new List<GBDownloadInfoModel>();
        }

        // Remove the previous output file if there is one
        if (File.Exists(outputTxtPath))
        {
            Logging.LogInformation($"Removing previous output txt with path {outputTxtPath}");
            File.Delete(outputTxtPath);
        }

        // Run commandline command to get files on google bucket and store 
        // file paths in a txt file
        Process cmd = CreateGetFilePathsProcess(bucketPath, outputTxtPath);
        Logging.LogInformation($"Querying google bucket for file paths. Files will be stored in {outputTxtPath}");
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
    /// <returns>A list of the eeg file paths</returns>
    private List<GBDownloadInfoModel> ReadEEGFilesFromOutputTxt(string outputTxtPath)
    {
        // log error if the output txt cannot be found
        if (File.Exists(outputTxtPath) == false)
        {
            Logging.LogError($"No file found with path {outputTxtPath}");
            return new List<GBDownloadInfoModel>();
        }

        // Read eeg file paths from output txt
        List<GBDownloadInfoModel> eegFiles = new();
        using (StreamReader sr = new(outputTxtPath))
        {
            if(sr.EndOfStream == true)
            {
                Logging.LogError("Output txt empty. Google bucket query was unsuccessful.");
                return eegFiles;
            }

            while (sr.EndOfStream == false)
            {
                string? line = sr.ReadLine();
                if (line != null && line.Trim().EndsWith("eeg.edf"))
                {
                    GBDownloadInfoModel? gbInfo = GetGbInfoFromLine(line);
                    if(gbInfo != null)
                    {
                        Logging.LogInformation($"Added file to list of eeg files that exist in google bucket:\n\t{gbInfo.FileNameWithExtension}");
                        eegFiles.Add(gbInfo);
                    }
                }
            }
        }

        Logging.LogInformation($"Read in all eeg files from {outputTxtPath}");
        return eegFiles;
    }

    /// <summary>
    /// Gets google bucket info from a line of text stored when querying the google bucket
    /// </summary>
    /// <param name="line">A line of text output from querying the google bucket</param>
    /// <returns>The google bucket info for that line if it can be interpreted, otherwise null</returns>
    private GBDownloadInfoModel? GetGbInfoFromLine(string line)
    {
        if (line.ToLower().StartsWith("total"))
        {
            Logging.LogInformation("Read in final line from available files on google bucket");
            return null;
        }

        string fullPath = line.Trim().Split().Last();
        DateTime uploadDateTime = DateTime.Now;
        try
        {
            string[] lineSplit = line.Trim().Split()
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToArray();
            string uploadDateStr = lineSplit[2];
            uploadDateTime = DateTime.Parse(uploadDateStr);
        }
        catch
        {
            Logging.LogError($"Date could not be identified in the following line:\n\t{line.Trim()}");
            return null;
        }

        GBDownloadInfoModel gbInfo = new(fullPath, uploadDateTime);
        if (gbInfo.NoNullValues)
        {
            if (gbInfo.DataType.ToLower().Equals("eeg"))
            {
                return gbInfo;
            }
        }
        else
        {
            Logging.LogError($"File name not in the expected format:\n\t{line.Trim()}");
        }

        return null;
    }

    /// <summary>
    /// Creates a process for use with downloading files from the google bucket
    /// </summary>
    /// <returns>The process to use</returns>
    private Process CreateDownloadFilesProcess()
    {
        return new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                RedirectStandardInput = true,
                CreateNoWindow = true,
                UseShellExecute = false,
            }
        };
    }

    #endregion
}
