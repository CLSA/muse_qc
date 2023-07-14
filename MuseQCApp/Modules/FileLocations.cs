using Microsoft.Extensions.Logging;
using MuseQCApp.Helpers;
using MuseQCApp.Interfaces;
using MuseQCApp.Models;
using MuseQCDBAccess.Data;

namespace MuseQCApp.Modules;

public class FileLocations : IFileLocations
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

    private readonly MysqlDBData Db;

    #endregion

    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="configHelper">Helper to access configuration settings</param>
    /// <param name="logging">The logger to use</param>
    public FileLocations(ConfigHelper configHelper, ILogger logging, MysqlDBData dbData)
    {
        ConfigHelper = configHelper;
        Logging = logging;
        Db = dbData;
    }

    #endregion

    #region Implemented interface methods

    public List<GBDownloadInfoModel> DecideFilesToDownload(List<GBDownloadInfoModel> downloadableFiles)
    {
        // Get the upload date time of the last file downloaded
        DateTime? lastTimeDownloaded = GetUploadDTLastFileDownloaded();

        // Check each file that can be downloaded and select those that need to be downloaded
        List<GBDownloadInfoModel> filesToDownload = new();
        foreach (GBDownloadInfoModel gbInfo in downloadableFiles)
        {
            try
            {
                // Ignore if missing information or if the upload date is earlier then the last file downloaded
                if (gbInfo.NoNullValues == false
                    || (lastTimeDownloaded != null && gbInfo.UploadDateTime.CompareTo(lastTimeDownloaded) < 0))
                {
                    continue;
                }

                bool dbUpToDate = InsertDataIntoDb(gbInfo);
                if (dbUpToDate)
                {
                    filesToDownload.Add(gbInfo);
                }
            }
            catch(Exception ex) 
            {
                Logging.LogError($"Error while evaluating {gbInfo.FileNameWithExtension}. Msg: {ex.Message}");
            }
        }
        return filesToDownload;
    }

    public ReportInfoModel GetReportInfo(DateTime StartDate, DateTime EndDate)
    {
        throw new NotImplementedException();
    }

    public List<string> NeedsQualityCheck()
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Get the upload date time of the last file downloaded
    /// </summary>
    /// <returns>The upload date time of the last file downloaded, or Null if there has no been any files downloaded</returns>
    public DateTime? GetUploadDTLastFileDownloaded()
    {
        DateTime? lastTimeDownloaded = Db.ConfigVals.GetLastDateDownloaded().Result.First();
        return lastTimeDownloaded;
    }

    /// <summary>
    /// Insert data from the google bucket info into the database if it doesn't exist already
    /// </summary>
    /// <param name="gbInfo">The google bucket info to use</param>
    /// <returns>True if there were not no problem, false if an error occurs</returns>
    private bool InsertDataIntoDb(GBDownloadInfoModel gbInfo)
    {
        // Do not attempt to enter values in db if any data is null (aka missing)
        if (gbInfo.NoNullValues == false) return false;
        string westonID = gbInfo.WestonID ?? "";
        DateTime startDateTime = gbInfo.CollectionDateTime ?? DateTime.Now;
        string podID = gbInfo.PodID ?? "";

        // Insert weston id into database if it doesn't exist
        // return false if there is a problem inserting into the db
        if (InsertWestonID(westonID) == false) return false;

        // Insert collection basic info into database if it doesn't exist
        // return true if there are no problems,
        // otherwise false if any errors occur
        return InsertCollectionBasicInfo(westonID, startDateTime, podID, gbInfo);
    }

    /// <summary>
    /// Insert the weston id into the db if it does not exists in the db
    /// </summary>
    /// <param name="westonID">The weston id to insert</param>
    /// <returns>True if there are no problems, false if any errors occur</returns>
    private bool InsertWestonID(string westonID)
    {
        // Add weston ID to DB if it does not already exist
        bool westonIdExists = Db.Participant.WestonIDExists(westonID).Result.First();
        if (westonIdExists == false)
        {
            bool completed = Db.Participant.InsertWestonID(westonID).Wait(5000);
            if (completed == false)
            {
                Logging.LogError($"Unable to insert WestonID: {westonID} into DB");
                return false;
            }
            Logging.LogInformation($"Inserted WestonID: {westonID} into DB");
        }

        return true;
    }

    private bool InsertCollectionBasicInfo(string westonID, DateTime startDateTime, string podID, GBDownloadInfoModel gbInfo)
    {
        // Log error if the pod id is not of length 14
        if (podID.Length != 14)
        {
            Logging.LogError($"Incorrectly formatted pod id. Expected length: 14 Actual: {podID} ({podID.Length})");
            return false;
        }

        // Add collection basic info to Db if it does not already exist
        bool collectionBasicInfoExists = Db.Collection.CollectionExists(westonID, startDateTime, podID).Result.First();
        if (collectionBasicInfoExists == false)
        {
            // None Will not be null because No Null check already preformed above
            float offset = gbInfo.TimeZoneOffset ?? 0;

            DateTime insertTime = DateTime.Now;
            bool completed = Db.Collection.InsertBasicInfo(westonID, startDateTime, offset, podID, gbInfo.UploadDateTime, insertTime).Wait(5000);
            if (completed == false)
            {
                Logging.LogError($"Unable to insert Collection Basic info into DB " +
                    $"WID: {westonID} Pod id: {podID} Offset: {offset} start: {startDateTime} Upload: {gbInfo.UploadDateTime}");
                return false;
            }
            Logging.LogInformation($"Inserted Basic info into DB at {insertTime}. WestonID: {westonID} Start: {startDateTime} ({offset}) Pod: {podID} Upload: {gbInfo.UploadDateTime}");
        }
        return true;
    }

    #endregion
}
