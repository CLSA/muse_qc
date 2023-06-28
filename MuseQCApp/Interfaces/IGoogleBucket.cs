using MuseQCApp.Models;

namespace MuseQCApp.Interfaces;

/// <summary>
/// An interface for google bucket interaction
/// </summary>
public interface IGoogleBucket
{
    /// <summary>
    /// Get all file paths stored in the google bucket
    /// </summary>
    /// <returns>A list of filenames for files that exist in the google bucket</returns>
    /// NOTE: Reads required information from configuration. 
    ///       Each implementation may need different values in configuration
    ///       The required config values should be noted in the comments when implmenting this method
    public List<GBDownloadInfoModel> GetFilePaths();

    /// <summary>
    /// Download the requested files from the google bucket
    /// </summary>
    /// <param name="filePathsDict">A list of filenames to download</param>
    /// <param name="storageDirPath">The folder to store the downloaded files in</param>
    /// <returns>True if all requested files were downloaded, otherwise false</returns>
    public bool DownloadFiles(List<GBDownloadInfoModel> filePaths);
}
