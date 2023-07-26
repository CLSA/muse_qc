using Microsoft.Extensions.Logging;
using MuseQCApp.Helpers;
using MuseQCApp.Interfaces;
using MuseQCApp.Logic;
using MuseQCApp.Models;
using MuseQCDBAccess.Data;
using MuseQCDBAccess.Models;

namespace MuseQCApp;

public class App
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

    /// <summary>
    /// A module to use to interact with the google bucket
    /// </summary>
    private IGoogleBucket Bucket {  get; init; }

    /// <summary>
    /// A module to use for making decisions on file locations
    /// </summary>
    private IFileLocations FilePaths { get; init; }

    /// <summary>
    /// A module for running the muse quality checks
    /// </summary>
    private IMuseQualityRunner QualityRunner { get; init; }

    /// <summary>
    /// A module for determining is muse data quality is acceptable
    /// </summary>
    private IMuseQualityDecisions MuseQuality { get; init; }

    /// <summary>
    /// A module for creating muse quality reports
    /// </summary>
    private IQualityReport QualityReport { get; init; }

    /// <summary>
    /// A module for cleaning up the unnecessary files in the file system
    /// </summary>
    private ICleanUp Clean { get; init; }

    private MysqlDBData Db { get; init; }

    #endregion

    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="configHelper">Helper to access configuration settings</param>
    /// <param name="logging">The logger to use</param>
    /// <param name="bucket">A module to use to interact with the google bucket</param>
    /// <param name="filePaths">A module to use for making decisions on file locations</param>
    /// <param name="qualityRunner">A module for running the muse quality checks</param>
    /// <param name="museQuality">A module for determining is muse data quality is acceptable</param>
    /// <param name="qualityReport">A module for creating muse quality reports</param>
    /// <param name="clean">A module for cleaning up the unnecessary files in the file system</param>
    public App(ConfigHelper configHelper, ILogger logging, IGoogleBucket bucket, IFileLocations filePaths,
        IMuseQualityRunner qualityRunner, IMuseQualityDecisions museQuality, IQualityReport qualityReport, ICleanUp clean, MysqlDBData db)
    {
        ConfigHelper = configHelper;
        Logging = logging;
        Bucket = bucket;
        FilePaths = filePaths;
        QualityRunner = qualityRunner;
        MuseQuality = museQuality;
        QualityReport = qualityReport;
        Clean = clean;
        Db = db;
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Runs the quality checks application process
    /// </summary>
    public void Run()
    {
        string appName = AppDomain.CurrentDomain.FriendlyName;
        Logging.LogInformation($"{appName} started running");

        // Determine files that need to be downloaded from the bucket
        List<GBDownloadInfoModel> pathsOnBucket = Bucket.GetFilePaths();
        List<GBDownloadInfoModel> pathsToDownload = FilePaths.DecideFilesToDownload(pathsOnBucket);

        // TODO: remove when done testing
        pathsToDownload = SelectXFiles(pathsToDownload, 10); 

        // Download files and update DB with paths
        string? edfStorageFolderPath = ConfigHelper.GetEdfStorageFolderPath();
        if(string.IsNullOrEmpty(edfStorageFolderPath) == false)
        {
            List<GBDownloadInfoModel> filesDownloadedSuccessfully = Bucket.DownloadFiles(pathsToDownload, edfStorageFolderPath);
            UpdateDbWithDownloadedFilePaths(filesDownloadedSuccessfully, edfStorageFolderPath);
        }

        Logging.LogInformation($"{appName} done running");
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Adds the paths that were downloaded to the database
    /// </summary>
    /// <param name="pathsThatWereDownload">The information for files that were downloaded</param>
    /// <param name="edfStorageFolderPath">The path to were edf files are stored</param>
    private void UpdateDbWithDownloadedFilePaths(List<GBDownloadInfoModel> pathsThatWereDownload, string edfStorageFolderPath)
    {
        foreach (GBDownloadInfoModel gbInfo in pathsThatWereDownload)
        {
            string fullFilePath = gbInfo.GetDownloadFilePath(edfStorageFolderPath);
            if (gbInfo.NoNullValues == false) continue;
            Db.Collection.UpdateEdfPath(gbInfo.WestonID, gbInfo.CollectionDateTime.Value, gbInfo.PodID, fullFilePath).Wait();
        }
    }

    /// <summary>
    /// Selects files from oldest upload date to newest
    /// </summary>
    /// <param name="files">The information of files that can be selected</param>
    /// <param name="numToSelect">The number of files to be selected</param>
    /// <returns>Information for the selected files</returns>
    private List<GBDownloadInfoModel> SelectXFiles(List<GBDownloadInfoModel> files, int numToSelect)
    {
        files = files.OrderBy(x => x.UploadDateTime).ToList();
        int maxINt = numToSelect < files.Count ? numToSelect : files.Count;
        List<GBDownloadInfoModel> smallerPathsToDownload = new();
        for (int i = 0; i < maxINt; i++)
        {
            smallerPathsToDownload.Add(files[i]);
        }
        return smallerPathsToDownload;
    }

    private void RunQualityChecks(List<string> edfFiles)
    {
        // Get output and jpg folder paths (return if either is null)
        string? outDirPath = ConfigHelper.GetOutputStorageFolderPath();
        string? jpgDirPath = ConfigHelper.GetJpgStorageFolderPath();
        if (outDirPath is null || jpgDirPath is null) return;

        // Create out folder if it does not exist
        string outDir = $"{outDirPath.Replace("\\", "/")}";

        // Run quality checks
        foreach (string edf in edfFiles)
        {
            Logging.LogInformation($"Started: {edf}");
            // Run quality check R script 
            MuseQualityOutputPaths outputPaths = QualityRunner.RunMuseQualityCheck(edf, outDir);

            // Check if files exist
            bool outputFilesExist = outputPaths is not null &&
                File.Exists(outputPaths.JpgPath) 
                && File.Exists(outputPaths.CsvPath) 
                && File.Exists(outputPaths.EdfPath);
            if (outputFilesExist == false)
            {
                Logging.LogError($"Not all output files were created by the quality script for input file: {edf}");
                continue;
            }

            bool runSuccessfully = UpdateDb(outputPaths, jpgDirPath);

            if (runSuccessfully == false)
            {
                continue;
            }

            Logging.LogInformation($"Completed: {edf}");
            //File.Delete(outputPaths.CsvPath);
            File.Delete(outputPaths.EdfPath);
            //File.Delete(edf);
        }
    }

    private bool UpdateDb(MuseQualityOutputPaths outputPaths, string jpgDirPath)
    {
        // delete output csv, output edf and input edf
        QCStatsModel? qcStats = QualityRunner.ReadOutputCsv(outputPaths.CsvPath);
        // NOTE: It is assumed that the jpg filename is the same
        // as the google bucket name
        string fileName = Path.GetFileNameWithoutExtension(outputPaths.JpgPath);
        string? westonId = MuseGBFileName.GetWestonID(fileName);
        string? podSerial = MuseGBFileName.GetPodID(fileName);
        DateTime? startDate = MuseGBFileName.GetStartDateTime(fileName);
        if (qcStats is null || westonId is null || podSerial is null || startDate is null)
        {
            Logging.LogError($"Unable to read all info. qcStats: {(qcStats is null ? "NULL" : "Correct")}" +
                $" westonID: {westonId} podSerial: {podSerial} startDate: {startDate}");
            return false;
        }

        bool realData = MuseQuality.IsActualNight(qcStats);
        bool problem = MuseQuality.HasProblem(qcStats);

        // Move jpg file to jpg specific folder
        string newJpgPath = Path.Combine(jpgDirPath, Path.GetFileName(outputPaths.JpgPath));
        File.Move(outputPaths.JpgPath, newJpgPath);

        // Update db with jpg path and data from csv
        Db.Collection.InsertQualityOutputs(westonId, startDate.Value, podSerial, qcStats, newJpgPath, realData, problem);

        // Remove edf path from db
        bool edfExists = Db.Collection.EdfExists(westonId, startDate.Value, podSerial).Result.First();
        if (edfExists)
        {
            Db.Collection.UpdateEdfPath(westonId, startDate.Value, podSerial, "");
        }
        return true;
    }
    #endregion
}
