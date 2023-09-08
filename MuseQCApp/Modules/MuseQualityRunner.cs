using Microsoft.Extensions.Logging;
using MuseQCApp.Helpers;
using MuseQCApp.Interfaces;
using MuseQCApp.Models;
using MuseQCDBAccess.Models;
using System.Diagnostics;

namespace MuseQCApp.Modules;

public class MuseQualityRunner : IMuseQualityRunner
{
    #region Private variable

    private const string EEGCh1 = "eegch1";
    private const string EEGCh2 = "eegch2";
    private const string EEGCh3 = "eegch3";
    private const string EEGCh4 = "eegch4";
    private const string EEGCh1_2 = "eeg_ch1-eeg_ch2";
    private const string EEGCh1_3 = "eeg_ch1-eeg_ch3";
    private const string EEGCh4_2 = "eeg_ch4-eeg_ch2";
    private const string EEGCh4_3 = "eeg_ch4-eeg_ch3";
    private const string FAny = "F.any";
    private const string FBoth = "F.both";
    private const string TAny = "T.any";
    private const string TBoth = "T.both";
    private const string FTAny = "FT.any";
    private const string EEGAny = "EEG.any";
    private const string EEGAll = "EEG.all";

    #endregion

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
    public MuseQualityRunner(ConfigHelper configHelper, ILogger logging)
    {
        ConfigHelper = configHelper;
        Logging = logging;
    }

    #endregion

    #region Implemented interface methods

    public MuseQualityOutputPathsModel? RunMuseQualityCheck(string edfPath, string outputPath)
    {
        // output path must have a "/" at the end in order to work with R script
        if (outputPath.EndsWith("/") == false)
        {
            outputPath = outputPath + "/";
        }

        // Return null 
        if(File.Exists(edfPath) == false)
        {
            return null;
        }

        edfPath = edfPath.Replace( "\\", "/");

        // Run quality script
        Process cmd = CreateMuseQCProcess(edfPath, outputPath);
        Logging.LogInformation($"Processing {edfPath}. Output files will be stored in {outputPath}");
        cmd.Start();
        cmd.WaitForExit();

        // Recreate the paths where the script stores output files
        string inFileName = Path.GetFileNameWithoutExtension(edfPath);
        string outPathWithoutExtension = $"{outputPath}{inFileName}";
        string jpgPath = $"{outPathWithoutExtension}.jpg";
        string csvPath = $"{outPathWithoutExtension}.csv";
        string outEdfPath = $"{outPathWithoutExtension}.filtered.edf";

        // Return the paths where files should now be stored
        return new MuseQualityOutputPathsModel(jpgPath, csvPath, outEdfPath);
    }

    public QCStatsModel? ReadOutputCsv(string csvPath)
    {
        using StreamReader sr = new StreamReader(csvPath);

        // Read first line (should be "qc.stats")
        int lineNum = 1;
        int totalLines = 17;
        QCStatsModel qcStats = new();
        string errorString = "";
        while (sr.EndOfStream == false && lineNum < (totalLines + 1))
        {
            string? curLine = sr.ReadLine();

            // Log an error if the line is null
            if (curLine is null)
            {
                Logging.LogError($"Unexpected Null line while reading qc stats csv file. Line number: {lineNum} Path: {csvPath}");
                return null;
            }

            errorString = InterpretLine(curLine, lineNum, qcStats);

            // Log any errors that occur while processing the csv
            if(errorString != "")
            {
                Logging.LogError($"A problem occured while processing line {lineNum} of the QC stats csv: {errorString}. Path: {csvPath}");
                return null;
            }
            lineNum++;
        }

        // Log an error if the file did not contain all the expected lines
        if(lineNum < (totalLines + 1))
        {
            Logging.LogError($"The QC stats csv had less lines ({lineNum}) of data than the expected 17: . Path: {csvPath}");
        }

        // Return the stats if there were no problems
        return qcStats;
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Creates a process to run the muse QC script
    /// </summary>
    /// <param name="bucketPath">The path ot the google bucket</param>
    /// <param name="outputTxtPath">The path to the folder to output data to. NOTE: The path must end with a "/"</param>
    /// <returns>The process</returns>
    private Process CreateMuseQCProcess(string edfPath, string outputPath)
    {
        string currentDir = Directory.GetCurrentDirectory();
        Logging.LogTrace($"Current Dir: {currentDir}");
        string workingDir = Path.Combine( currentDir, "R");
        Logging.LogTrace($"Working Dir: {workingDir}");
        string args = $"/c Rscript --vanilla MUSE_clean_QC.R \"{edfPath}\" \"{outputPath}\"";
        Logging.LogTrace($"args: {args}");

        return new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                RedirectStandardInput = true,
                CreateNoWindow = false,
                UseShellExecute = false,
                WorkingDirectory = workingDir,
                Arguments = args
            }
        };
    }

    /// <summary>
    /// Interprets the passed in line from the edf file and stores any value from 
    /// the csv in the qcStats object
    /// </summary>
    /// <param name="curLine">The current line to interpret</param>
    /// <param name="lineNum">The line number of the line to interpret</param>
    /// <param name="qcStats">The qc stats to update</param>
    /// <returns></returns>
    private string InterpretLine(string curLine, int lineNum, QCStatsModel qcStats)
    {
        curLine = curLine.Replace("\"", "").Trim();
        switch (lineNum)
        {
            case 1:
                return InterpretFirstLine(curLine);
            case 2:
                return InterpretDuration(curLine, qcStats);
            case 3:
                return InterpretGeneralLine(curLine, EEGCh1, qcStats);
            case 4:
                return InterpretGeneralLine(curLine, EEGCh2, qcStats);
            case 5:
                return InterpretGeneralLine(curLine, EEGCh3, qcStats);
            case 6:
                return InterpretGeneralLine(curLine, EEGCh4, qcStats);
            case 7:
                return InterpretGeneralLine(curLine, EEGCh1_2, qcStats);
            case 8:
                return InterpretGeneralLine(curLine, EEGCh1_3, qcStats);
            case 9:
                return InterpretGeneralLine(curLine, EEGCh4_3, qcStats);
            case 10:
                return InterpretGeneralLine(curLine, EEGCh4_2, qcStats);
            case 11:
                return InterpretGeneralLine(curLine, FAny, qcStats);
            case 12:
                return InterpretGeneralLine(curLine, FBoth, qcStats);
            case 13:
                return InterpretGeneralLine(curLine, TAny, qcStats);
            case 14:
                return InterpretGeneralLine(curLine, TBoth, qcStats);
            case 15:
                return InterpretGeneralLine(curLine, FTAny, qcStats);
            case 16:
                return InterpretGeneralLine(curLine, EEGAny, qcStats);
            case 17:
                return InterpretGeneralLine(curLine, EEGAll, qcStats);
            default:
                // NOTE: This line should never be reachable
                return "There is no interpretation for this line";
        }
    }

    /// <summary>
    /// Intprets the first line of the edf file
    /// </summary>
    /// <param name="line">The line to interpret from the csv</param>
    /// <returns>An error message string if there is a problem. Otherwise returns an empty string</returns>
    private string InterpretFirstLine(string line)
    {
        if(line.Contains("qc.stats") == false)
        {
            return "The first line does not contain qc.stats";
        }
        return "";
    }

    /// <summary>
    /// Intprets the duration of the edf file
    /// </summary>
    /// <param name="line">The line to interpret from the csv</param>
    /// <param name="qcStats">The QC stats model to update with the value read in</param>
    /// <returns>An error message string if there is a problem. Otherwise updates the QCStats passed 
    /// in with the read in value and returns an empty string</returns>
    private string InterpretDuration(string line, QCStatsModel qcStats)
    {
        // parse value
        bool parsed = double.TryParse(line, out double result);

        // Return an error if value could not be parsed
        if (parsed == false)
        {
            return $"Line formatted incorrectly. Could not parse the numeric value of {line} for the duration. ";
        }
        qcStats.Dur = result;
        return "";
    }

    /// <summary>
    /// Intprets the lines of the edf file that have a data value after text and 
    /// stores the value from the csv in the qcStats object
    /// </summary>
    /// <param name="line">The line to interpret from the csv</param>
    /// <param name="expectedText">The expected text that should be before the value</param>
    /// <param name="qcStats">The QC stats model to update with the value read in</param>
    /// <returns>An error message string if there is a problem. Otherwise updates the QCStats passed 
    /// in with the read in value and returns an empty string</returns>
    private string InterpretGeneralLine(string line, string expectedText, QCStatsModel qcStats)
    {
        // split line and report error if the line is formatted incorrectly
        string[] lineSplit = line.Split(' ');
        if (lineSplit.Length < 2 || lineSplit[0].Contains(expectedText) == false)
        {
            return $"Line formatted incorrectly. Expected: \"{expectedText} [Value]\" Actual: {line}";
        }

        // parse value
        bool parsed = double.TryParse(lineSplit[1], out double result);

        // Return an error if value could not be parsed
        if (parsed == false)
        {
            return $"Line formatted incorrectly. Could not parse the numeric value of {lineSplit[1]} for {expectedText}. ";
        }

        // store value
        if (expectedText.Equals(EEGCh1))
        {
            qcStats.Ch1 = result;
        }
        else if (expectedText.Equals(EEGCh2))
        {
            qcStats.Ch2 = result;
        }
        else if (expectedText.Equals(EEGCh3))
        {
            qcStats.Ch3 = result;
        }
        else if (expectedText.Equals(EEGCh4))
        {
            qcStats.Ch4 = result;
        }
        else if (expectedText.Equals(EEGCh1_2))
        {
            qcStats.Ch12 = result;
        }
        else if (expectedText.Equals(EEGCh1_3))
        {
            qcStats.Ch13 = result;
        }
        else if (expectedText.Equals(EEGCh4_2))
        {
            qcStats.Ch42 = result;
        }
        else if (expectedText.Equals(EEGCh4_3))
        {
            qcStats.Ch43 = result;
        }
        else if (expectedText.Equals(FAny))
        {
            qcStats.FAny = result;
        }
        else if (expectedText.Equals(FBoth))
        {
            qcStats.FBoth = result;
        }
        else if (expectedText.Equals(TAny))
        {
            qcStats.TAny = result;
        }
        else if (expectedText.Equals(TBoth))
        {
            qcStats.TBoth = result;
        }
        else if (expectedText.Equals(FTAny))
        {
            qcStats.FtAny = result;
        }
        else if (expectedText.Equals(EEGAny))
        {
            qcStats.EegAny = result;
        }
        else if (expectedText.Equals(EEGAll))
        {
            qcStats.EegAll = result;
        }
        return "";
    }
    #endregion
}
