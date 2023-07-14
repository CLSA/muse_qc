namespace MuseQCApp.Models;

/// <summary>
/// Information that is needed from Google Bucket downloads 
/// </summary>
public class GBDownloadInfoModel
{
    /// <summary>
    /// The full file path to where the file is stored in the google bucket
    /// </summary>
    public string FullFilePath { get; init; }

    /// <summary>
    /// The file name with extension
    /// </summary>
    public string FileNameWithExtension => Path.GetFileName(FullFilePath);

    public string DownloadFileNameWithExtension => FileNameWithExtension.Replace(":", "");

    /// <summary>
    /// The file name without extension
    /// </summary>
    public string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(FullFilePath);

    /// <summary>
    /// The Date and Time the file was uploaded into the google bucket
    /// </summary>
    public DateTime UploadDateTime { get; init; }

    /// <summary>
    /// The Date and Time the data collection started
    /// </summary>
    public DateTime? CollectionDateTime { get; init; }

    /// <summary>
    /// The time zone offset of the tablet collecting the data
    /// </summary>
    public float? TimeZoneOffset { get; init; }

    /// <summary>
    /// The serial number of the Muse pod used to collect data
    /// </summary>
    public string? PodID { get; init; }

    /// <summary>
    /// The weston ID of the participant
    /// </summary>
    public string? WestonID { get; init; }

    /// <summary>
    /// The type of data stored in the file
    /// </summary>
    public string? DataType { get; init; }

    /// <summary>
    /// True if none of the values inferred from the file name are null, false otherwise
    /// </summary>
    public bool NoNullValues => CollectionDateTime != null && TimeZoneOffset != null && PodID != null && WestonID != null && DataType != null;

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="fullFilePath">The full file path to where the file is stored in the google bucket</param>
    public GBDownloadInfoModel(string fullFilePath, DateTime uploadDateTime)
    {
        FullFilePath = fullFilePath;
        UploadDateTime = uploadDateTime;

        // Sample file name:
        //      2023-06-18T00:31:31-04:00_6002-CNZB-5F0A_ww75958498_acc
        //      [DateTime]       [Offset] [Pod ID]      [Weston ID] [Data type]
        CollectionDateTime = GetStartDateTimeFromFileName(FileNameWithoutExtension);
        TimeZoneOffset = GetTimezoneOffsetFromFileName(FileNameWithoutExtension);
        PodID = GetPodIDFromFileName(FileNameWithoutExtension);
        WestonID = GetWestonIDFromFileName(FileNameWithoutExtension);
        DataType = GetDataTypeFromFileName(FileNameWithoutExtension);
    }

    public string GetDownloadFilePath(string downloadDirectory)
    {
        return Path.Combine(downloadDirectory, DownloadFileNameWithExtension);
    }

    /// <summary>
    /// Get the start date time from the file name
    /// </summary>
    /// <param name="fileName">The file name</param>
    /// <returns>The start datetime if it could be parsed, otherwise null</returns>
    private DateTime? GetStartDateTimeFromFileName(string fileName)
    {
        try
        {
            string startDateTimeStr = fileName.Substring(0,19);
            bool dateParsed = DateTime.TryParse(startDateTimeStr, out DateTime startDateTime);
            if (dateParsed)
            {
                return startDateTime;
            }
        }
        catch { }
        return null;
    }

    /// <summary>
    /// Get the time zone offset from the file name
    /// </summary>
    /// <param name="fileName">The file name</param>
    /// <returns>The time zone offset if it could be parsed, otherwise null</returns>
    private float? GetTimezoneOffsetFromFileName(string fileName)
    {
        try
        {
            string hourStr = fileName.Substring(20,2);
            string minStr = fileName.Substring(23,2);
            bool hourParsed = int.TryParse(hourStr, out int hour);
            bool minParsed = int.TryParse(minStr, out int minute);
            if(hourParsed & minParsed)
            {
                float offset = (float)(hour + (minute / 60.0));
                return fileName[19].Equals('-') ? offset * -1 : offset;
            }
        }
        catch{}
        return null;
    }

    /// <summary>
    /// Get the pod id from the file name
    /// </summary>
    /// <param name="fileName">The file name</param>
    /// <returns>The pod id if it could be parsed, otherwise null</returns>
    private string? GetPodIDFromFileName(string fileName)
    {
        try
        {
            string podIdStr = fileName.Substring(26, 14);
            if (podIdStr[4] == '-' & podIdStr[9] == '-')
            {
                return podIdStr;
            }
        }
        catch { }
        return null;
    }

    /// <summary>
    /// Get the weston id from the file name
    /// </summary>
    /// <param name="fileName">The file name</param>
    /// <returns>The weston id if it could be parsed, otherwise null</returns>
    private string? GetWestonIDFromFileName(string fileName)
    {
        try
        {
            string westonIdStr = fileName.Substring(41, 10);
            if (westonIdStr.ToLower().StartsWith("ww") || westonIdStr.ToLower().StartsWith("tt"))
            {
                return westonIdStr;
            }
        }
        catch { }
        return null;
    }

    /// <summary>
    /// Get the data type from the file name
    /// </summary>
    /// <param name="fileName">The file name</param>
    /// <returns>The data type if it could be parsed, otherwise null</returns>
    private string? GetDataTypeFromFileName(string fileName)
    {
        try
        {
            return fileName.Split("_").Last();
        }
        catch { }
        return null;
    }
}
