namespace MuseQCApp.Models;

public class GBDownloadInfoModel
{
    /// <summary>
    /// The full file path to where the file is stored in the google bucket
    /// </summary>
    public string FullFilePath { get; init; }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="fullFilePath">The full file path to where the file is stored in the google bucket</param>
    public GBDownloadInfoModel(string fullFilePath)
    {
        FullFilePath = fullFilePath;
    }
}
