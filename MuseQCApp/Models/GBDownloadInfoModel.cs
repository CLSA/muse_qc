using MuseQCApp.Logic;

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

    public string DownloadFileNameWithExtension => MuseGBFileName.GbFileFileNameToWindowsFileName(FileNameWithExtension);

    /// <summary>
    /// The file name without extension
    /// </summary>
    public string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(FullFilePath);

    /// <summary>
    /// The Date and Time the file was uploaded into the google bucket
    /// </summary>
    public DateTime UploadDateTime { get; init; }

    /// <summary>
    /// The Size of the file
    /// </summary>
    public double Size { get; init; }

    /// <summary>
    /// The units for the size of the file
    /// </summary>
    public string SizeUnits { get; init; }

    /// <summary>
    /// True if the size of the file is less than 1 megabyte, false otherwise
    /// </summary>
    public bool LessThan1mb => SizeUnits.ToLower().Equals("kib") && Size < 1000;

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
    public GBDownloadInfoModel(string fullFilePath, DateTime uploadDateTime, double size, string sizeUnits)
    {
        FullFilePath = fullFilePath;
        UploadDateTime = uploadDateTime;
        Size = size;
        SizeUnits = sizeUnits;

        // Sample file name:
        //      2023-06-18T00:31:31-04:00_6002-CNZB-5F0A_ww75958498_acc
        //      [DateTime]       [Offset] [Pod ID]      [Weston ID] [Data type]
        CollectionDateTime = MuseGBFileName.GetStartDateTime(FileNameWithoutExtension);
        TimeZoneOffset = MuseGBFileName.GetTimezoneOffset(FileNameWithoutExtension);
        PodID = MuseGBFileName.GetPodID(FileNameWithoutExtension);
        WestonID = MuseGBFileName.GetWestonID(FileNameWithoutExtension);
        DataType = MuseGBFileName.GetDataType(FileNameWithoutExtension);
    }

    public string GetDownloadFilePath(string downloadDirectory)
    {
        return Path.Combine(downloadDirectory, DownloadFileNameWithExtension);
    }
}
