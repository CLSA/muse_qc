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
    /// Helper to interact with DB
    /// </summary>
    private DbHelpers DbMethods { get; init; }

    #endregion

    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="configHelper">Helper to access configuration settings</param>
    /// <param name="logging">The logger to use</param>
    /// <param name="bucket">A module to use to interact with the google bucket</param>
    /// <param name="qualityRunner">A module for running the muse quality checks</param>
    /// <param name="museQuality">A module for determining is muse data quality is acceptable</param>
    /// <param name="qualityReport">A module for creating muse quality reports</param>
    public App(ConfigHelper configHelper, ILogger logging, IGoogleBucket bucket,IMuseQualityRunner qualityRunner, 
        IMuseQualityDecisions museQuality, IQualityReport qualityReport, MysqlDBData db)
    {
        ConfigHelper = configHelper;
        Logging = logging;
        Bucket = bucket;
        QualityRunner = qualityRunner;
        MuseQuality = museQuality;
        QualityReport = qualityReport;
        DbMethods = new(db, logging);
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Runs the quality checks application process
    /// </summary>
    public void Run(int maxFilesToDownloadAtOnce = 10, int maxFilesToDownload = -1)
    {
        string appName = AppDomain.CurrentDomain.FriendlyName;
        Logging.LogInformation($"{appName} started running");

        // Get list of files on google bucket
        List<GBDownloadInfoModel> pathsOnBucket = Bucket.GetFilePaths();

        int filesDownloaded = 0;
        while( filesDownloaded < maxFilesToDownload)
        {
            // decide how many files to download this loop execution
            // while ensuring the max total number is not exceeded
            int filesLeftToDownload = (maxFilesToDownload - filesDownloaded);
            int numFilesToDownload = filesLeftToDownload > maxFilesToDownloadAtOnce 
                ? maxFilesToDownloadAtOnce : filesLeftToDownload;
            
            // Download files that have not had quality checks run
            // and are not currently downloaded
            DownloadFiles(pathsOnBucket, numFilesToDownload);

            // Run quality checks
            RunQualityChecks();

            // increment the number of files that have been downloaded
            filesDownloaded+= numFilesToDownload;
        }
        

        // Create reports
        // TODO: Implement

        Logging.LogInformation($"{appName} done running");
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Download all files
    /// </summary>
    /// <param name="pathsOnBucket">The paths to all files that exist in the google bucket</param>
    /// <param name="maxFilesToDownload">The maximum number of files that should be downloaded</param>
    private void DownloadFiles(List<GBDownloadInfoModel> pathsOnBucket, int maxFilesToDownload = 10)
    {
        // Select all files that need to be downloaded
        List<GBDownloadInfoModel> pathsToDownload = DecideFilesToDownload(pathsOnBucket);

        // Reduce the amount of files to download to a max specified amount
        pathsToDownload = SelectXFiles(pathsToDownload, maxFilesToDownload); pathsToDownload = SelectXFiles(pathsToDownload, maxFilesToDownload);
        
        // Download files and update DB with paths
        string? edfStorageFolderPath = ConfigHelper.GetEdfStorageFolderPath();
        if (string.IsNullOrEmpty(edfStorageFolderPath) == false)
        {
            List<GBDownloadInfoModel> filesDownloadedSuccessfully = Bucket.DownloadFiles(pathsToDownload, edfStorageFolderPath);
            DbMethods.UpdateDbWithDownloadedFilePaths(filesDownloadedSuccessfully, edfStorageFolderPath);
        }
        else
        {
            Logging.LogInformation($"No edf storage folder found in configuration");
        }
    }

    /// <summary>
    /// Runs quality checks on files that require quality checks
    /// </summary>
    private void RunQualityChecks()
    {
        // Get output and jpg folder paths (return if either is null)
        string? outDirPath = ConfigHelper.GetOutputStorageFolderPath();
        string? jpgDirPath = ConfigHelper.GetJpgStorageFolderPath();
        if (outDirPath is null || jpgDirPath is null) return;

        // Format out dir path and Create out folder if it does not exist
        string outDir = $"{outDirPath.Replace("\\", "/")}";
        if (Directory.Exists(outDir) == false) 
        { 
            Directory.CreateDirectory(outDir);
        }

        // Determine files that need to have quality checks run
        IEnumerable<string> edfFiles = DbMethods.GetEdfFilesThatNeedQualityChecks();

        // Run quality checks
        foreach (string edf in edfFiles)
        {
            // Log startime and file size of file that is starting to run
            DateTime startTime = DateTime.Now;
            long fileSize = new FileInfo(edf).Length;
            Logging.LogInformation($"Started: {edf} at {startTime}. FileSize: {fileSize}");

            // Run quality check R script 
            MuseQualityOutputPathsModel? outputPaths = QualityRunner.RunMuseQualityCheck(edf, outDir);

            // Log error if any of the output files do not exist
            if (outputPaths is null || File.Exists(outputPaths.JpgPath) == false
                || File.Exists(outputPaths.CsvPath) == false || File.Exists(outputPaths.EdfPath) == false)
            {
                Logging.LogError($"Not all output files were created by the quality script for input file: {edf}");
                continue;
            }

            // Update Muse quality information in DB
            MuseQualityResultsModel? resultsPackage = CreateMuseQualityResultsModel(outputPaths, jpgDirPath);
            bool runSuccessfully = false;
            if(resultsPackage is not null)
            {
                runSuccessfully = DbMethods.UpdateDbMuseQuality(resultsPackage);
            }
            

            // Log error if unable to update the DB
            if (runSuccessfully == false)
            {
                Logging.LogError($"Unable to update db with output paths for input file: {edf}\n" +
                    $"\tJpg: {outputPaths.JpgPath}\n\tCsv: {outputPaths.CsvPath}\n\tEdf: {outputPaths.EdfPath}\n");
                continue;
            }

            // Log end time when completed running quality check for participant
            DateTime endTime = DateTime.Now;
            Logging.LogInformation($"Completed: {edf} at {endTime}. Duration: {endTime-startTime}");
            
            // Delete uneeded files
            File.Delete(outputPaths.CsvPath);
            File.Delete(outputPaths.EdfPath);
            File.Delete(edf);
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

    /// <summary>
    /// Creates a MuseQualityResultsModel with all of the information required by the data base
    /// </summary>
    /// <param name="outputPaths">The paths to the files output by the Muse Quality R script</param>
    /// <param name="jpgDirPath">The path to the directory where jpg files should be stored</param>
    /// <returns>a MuseQualityResultsModel with all of the information required by the data base</returns>
    private MuseQualityResultsModel? CreateMuseQualityResultsModel(MuseQualityOutputPathsModel outputPaths, string jpgDirPath)
    {
        // Read data from output csv
        QCStatsModel? qcStats = QualityRunner.ReadOutputCsv(outputPaths.CsvPath);

        // Interpret values from jpg filename
        // NOTE: Assumes jpg filename is the same as the google bucket name
        string fileName = Path.GetFileNameWithoutExtension(outputPaths.JpgPath);
        string? westonId = MuseGBFileName.GetWestonID(fileName);
        string? podSerial = MuseGBFileName.GetPodID(fileName);
        DateTime? startDate = MuseGBFileName.GetStartDateTime(fileName);

        // Log error if any of the data is missing
        if (qcStats is null || westonId is null || podSerial is null || startDate is null)
        {
            Logging.LogError($"Unable to read all info. qcStats: {(qcStats is null ? "NULL" : "Correct")}" +
                $" westonID: {westonId} podSerial: {podSerial} startDate: {startDate}");
            return null;
        }

        // Interpret the results
        bool realData = MuseQuality.IsActualNight(qcStats);
        bool durProblem = MuseQuality.HasDurationProblem(qcStats);
        bool qualityProblem = MuseQuality.HasQualityProblem(qcStats);

        // Move jpg file to jpg specific folder
        string newJpgPath = Path.Combine(jpgDirPath, Path.GetFileName(outputPaths.JpgPath));
        File.Move(outputPaths.JpgPath, newJpgPath);

        return new MuseQualityResultsModel(qcStats, fileName, westonId, podSerial, startDate.Value, realData, durProblem, qualityProblem, MuseQuality.GetVersionNumber(), newJpgPath);
    }

    /// <summary>
    /// Creates a list of the files that are not currently downloaded that need to have anlysis run
    /// </summary>
    /// <param name="downloadableFiles">A list of information on all edf files that exist in the google bucket</param>
    /// <returns>A list of the files that are not currently downloaded that need to have anlysis run</returns>
    public List<GBDownloadInfoModel> DecideFilesToDownload(List<GBDownloadInfoModel> downloadableFiles)
    {
        // Get the upload date time of the last file downloaded
        DateTime? lastTimeDownloaded = DbMethods.GetUploadDTLastFileDownloaded();

        // Check each file that can be downloaded and select those that need to be downloaded
        List<GBDownloadInfoModel> filesToDownload = new();
        foreach (GBDownloadInfoModel gbInfo in downloadableFiles)
        {
            try
            {
                // Ignore if missing information or if the upload date is earlier then the last file downloaded
                if (gbInfo.NoNullValues == false
                    || lastTimeDownloaded != null && gbInfo.UploadDateTime.CompareTo(lastTimeDownloaded) < 0)
                {
                    continue;
                }

                // Skip if file is less than 1 megabyte
                if (gbInfo.LessThan1mb)
                {
                    Logging.LogTrace($"File size too low. Not being added to files to download. size: {gbInfo.Size} {gbInfo.SizeUnits} path: {gbInfo.FullFilePath}");
                    continue;
                }

                bool dbUpToDate = DbMethods.InsertGBDataIntoDb(gbInfo);
                if (dbUpToDate)
                {
                    filesToDownload.Add(gbInfo);
                }
            }
            catch (Exception ex)
            {
                Logging.LogError($"Error while evaluating {gbInfo.FileNameWithExtension}. Msg: {ex.Message}");
            }
        }
        return filesToDownload;
    }

    #endregion
}
