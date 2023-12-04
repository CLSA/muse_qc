using Microsoft.Extensions.Logging;
using MuseQCApp.Models;
using MuseQCDBAccess.Models;
using System.Diagnostics;

namespace MuseQCApp.Logic;
public class ReportWriter
{
    #region private properties

    /// <summary>
    /// The logger to use
    /// </summary>
    private ILogger Logging { get; init; }

    #endregion

    public ReportWriter(ILogger logging)
    {
        Logging = logging;
    }

    #region public methods

    /// <summary>
    /// Create all report csv files
    /// </summary>
    /// <param name="participants">Report quality data for each participant</param>
    /// <param name="reportFolderPath">The top level output folder to write output folders/files to</param>
    /// <param name="pythonExePath">The path to the python exe to use when running python files</param>
    public void CreateReports(List<ParticipantCollectionsQualityModel> participants, string reportFolderPath, string pythonExePath)
    {
        // Create Number of days report
        string numDaysFolder = Path.Combine(reportFolderPath, "NumDays");
        if (File.Exists(numDaysFolder) == false) Directory.CreateDirectory(numDaysFolder);
        CreateNumberOfDaysByWestonIdReport(participants, numDaysFolder);

        // Create summary report
        string summaryFolder = Path.Combine(reportFolderPath, "Summary");
        if (File.Exists(reportFolderPath) == false) Directory.CreateDirectory(reportFolderPath);
        string summaryCsvFolder = Path.Combine(summaryFolder, "csv");
        if (File.Exists(summaryCsvFolder) == false) Directory.CreateDirectory(summaryCsvFolder);
        string summaryPdfFolder = Path.Combine(summaryFolder, "pdf");
        if (File.Exists(summaryPdfFolder) == false) Directory.CreateDirectory(summaryPdfFolder);
        CreateSummaryReport(participants, summaryCsvFolder, summaryPdfFolder, pythonExePath);
        
        // Create in depth report
        string inDepthReportsFolder = Path.Combine(reportFolderPath, "InDepth");
        if (File.Exists(inDepthReportsFolder) == false) Directory.CreateDirectory(inDepthReportsFolder);
        string inDepthCsvFolder = Path.Combine(inDepthReportsFolder, "csv");
        if (File.Exists(inDepthCsvFolder) == false) Directory.CreateDirectory(inDepthCsvFolder);
        string inDepthPdfFolder = Path.Combine(inDepthReportsFolder, "pdf");
        if (File.Exists(inDepthPdfFolder) == false) Directory.CreateDirectory(inDepthPdfFolder);
        CreateInDepthSiteReports(participants, inDepthCsvFolder, inDepthPdfFolder, pythonExePath);        
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Creates a csv report for the Muse Number of days data
    /// </summary>
    /// <param name="participants">Report quality data for each participant</param>
    /// <param name="outputFolder">The output folder to store the report in</param>
    private void CreateNumberOfDaysByWestonIdReport(List<ParticipantCollectionsQualityModel> participants, string outputFolder)
    {
        // CODE TO CREATE FILES BASED ON SITE

        //List<string> sites = new();
        //foreach (var participant in participants)
        //{
        //    if(sites.Contains(participant.Site) == false)
        //    {
        //        sites.Add(participant.Site);
        //    }
        //}

        //string date = DateTime.Now.ToString("yyyy_MM_dd");
        //foreach (var site in sites)
        //{
        //    string csvPath = Path.Combine(outputFolder, $"MuseNumberOfDaysByWestonId_{site}_{date}.csv");
        //    using (StreamWriter sw = new(csvPath))
        //    {
        //        sw.WriteLine("WestonID,Days of good data,Total collections recorded (> 30 mins)");
        //        foreach(var participant in participants)
        //        {
        //            // skip if participant is from a different site
        //            if (participant.Site != site) continue;
        //            sw.WriteLine($"{participant.WestonID},{participant.NumberGoodDays},{participant.NumberCollections}");
        //        }
        //    }
        //}

        string date = DateTime.Now.ToString("yyyy_MM_dd");
        string csvPath = Path.Combine(outputFolder, $"MuseNumberOfDaysByWestonId_{date}.csv");
        using (StreamWriter sw = new(csvPath))
        {
            sw.WriteLine("WestonID,Days of good data,Total collections recorded (> 30 mins)");
            foreach (var participant in participants)
            {
                sw.WriteLine($"{participant.WestonID},{participant.NumberGoodDays},{participant.NumberCollections}");
            }
        }
    }

    /// <summary>
    /// Create the in depth reports (Both the csv and pdf)
    /// </summary>
    /// <param name="participants">The participant info</param>
    /// <param name="outputCsvFolder">The output folder where csv site folders should be stored</param>
    /// <param name="outputPdfFolder">he output folder where pdf site folders should be stored</param>
    /// <param name="pythonExePath">The path to the python exe to use</param>
    private void CreateInDepthSiteReports(
        List<ParticipantCollectionsQualityModel> participants, string outputCsvFolder, string outputPdfFolder, string pythonExePath)
    {
        Dictionary<string, Dictionary<DateOnly, Dictionary<bool, List<ParticipantCollectionsQualityModel>>>> dict
            = CreateInDepthParticipantQualityDict(participants);

        foreach (string site in dict.Keys)
        {
            foreach (DateOnly dateOnly in dict[site].Keys)
            {
                string monthStr = dateOnly.ToString("MMMM_yyyy");
                string fileName = $"InDepthQualityReport_{site}_{monthStr}";
                string outCsvSiteFolder = Path.Combine(outputCsvFolder, site);
                if (File.Exists(outCsvSiteFolder) == false) Directory.CreateDirectory(outCsvSiteFolder);
                string outPdfSiteFolder = Path.Combine(outputPdfFolder, site);
                if (File.Exists(outPdfSiteFolder) == false) Directory.CreateDirectory(outPdfSiteFolder);
                string csvPath = Path.Combine(outCsvSiteFolder, $"{fileName}.csv");

                // Create csv
                Dictionary<bool, List<ParticipantCollectionsQualityModel>> csvInfoDict = dict[site][dateOnly];
                using (StreamWriter sw = new(csvPath))
                {
                    sw.WriteLine("Section Header,Weston ID,Jpg Path,Start Date,Problem");

                    sw.WriteLine("Problems");
                    List<ParticipantCollectionsQualityModel> lessThan3DaysList = new();
                    foreach (ParticipantCollectionsQualityModel participant in csvInfoDict[true])
                    {
                        if (participant.HasLessThan3Days
                            && participant.HasDataProblem == false)
                        {
                            lessThan3DaysList.Add(participant);
                        }
                        else
                        {
                            WriteInDepthInfo(sw, participant);
                        }
                    }

                    sw.WriteLine("Problems: Less than 3 data collections");
                    foreach (ParticipantCollectionsQualityModel participant in lessThan3DaysList)
                    {
                        WriteInDepthInfo(sw, participant);
                    }

                    sw.WriteLine("Good collections");
                    foreach (ParticipantCollectionsQualityModel participant in csvInfoDict[false])
                    {
                        WriteInDepthInfo(sw, participant);
                    }
                }

                // Create PDF
                Process inDepthPdfCreation = CreatePythonReportProcess(pythonExePath, "jpg", csvPath, outPdfSiteFolder);
                inDepthPdfCreation.Start();
                inDepthPdfCreation.WaitForExit();
            }
        }
    }

    /// <summary>
    /// Writes participant quality data to in depth quality report
    /// </summary>
    /// <param name="sw">The stream writer to write to</param>
    /// <param name="p">The participant collections quality models to be written</param>
    private void WriteInDepthInfo(StreamWriter sw, ParticipantCollectionsQualityModel p)
    {
        p.EegQualityReportModels.OrderBy(x => x.StartDateTime);
        foreach (EegQualityReportModel eeg in p.EegQualityReportModels)
        {
            sw.WriteLine($",{p.WestonID},{eeg.JpgPath},{eeg.StartDateTime},{eeg.ProblemsStr}");
        }
    }

    /// <summary>
    /// Organizes participant Muse Quality data by site and month into a dictionary
    /// </summary>
    /// <param name="participants">Report quality data for each participant</param>
    /// <returns>The dictionairy created</returns>
    private Dictionary<string, Dictionary<DateOnly, Dictionary<bool, List<ParticipantCollectionsQualityModel>>>> CreateInDepthParticipantQualityDict
        (List<ParticipantCollectionsQualityModel> participants)
    {
        Dictionary<string, Dictionary<DateOnly, Dictionary<bool, List<ParticipantCollectionsQualityModel>>>> dict = new();
        foreach (ParticipantCollectionsQualityModel p in participants)
        {
            // Add site to dict if not added already
            if (dict.ContainsKey(p.Site) == false)
                dict.Add(p.Site, new());

            // Add month to dict if not added already
            if (dict[p.Site].ContainsKey(p.CollectionMonth) == false)
                dict[p.Site].Add(p.CollectionMonth, new() { { true, new() }, { false, new() } });

            dict[p.Site][p.CollectionMonth][p.HasAtleastOneProblem].Add(p);
        }
        return dict;
    }

    /// <summary>
    /// Creates summary reports (csv and pdf)
    /// </summary>
    /// <param name="participants">Report quality data for each participant</param>
    /// <param name="summaryCsvFolder">The folder to store summary csv files in</param>
    /// <param name="summaryPdfFolder">The folder to store summary pdf files in</param>
    /// <param name="pythonExePath">The path to the python exe to use</param>
    private void CreateSummaryReport(List<ParticipantCollectionsQualityModel> participants, string summaryCsvFolder, string summaryPdfFolder, string pythonExePath)
    {
        DateOnly lastMonth = DateOnly.FromDateTime(DateTime.Now.AddMonths(-1));
        lastMonth = lastMonth.AddDays(1 - lastMonth.Day).AddMonths(1).AddDays(-1);
        string lastMonthStr = lastMonth.ToString("MMMM_yyyy");
        string fileName = $"QualityReport_{lastMonthStr}";
        string csvPath = Path.Combine(summaryCsvFolder, $"{fileName}.csv");

        if (File.Exists(csvPath)) return;

        Dictionary<string, Dictionary<DateOnly, QualitySummaryModel>> summariesDict = GetSummaryStats(participants, lastMonth);

        using(StreamWriter sw = new(csvPath))
        {
            // Print Header
            sw.WriteLine("Summary Table Title,Category,0 Days,1 Days,2 Days,3 Days,4+ Days,Duration,Frontal,Temporal,< 3 Collected");

            PrintAllTime(sw, summariesDict);
            PrintByMonth(sw, summariesDict);
            PrintBySite(sw, summariesDict);
        }

        // Create the pdf files
        Process summaryCreation = CreatePythonReportProcess(pythonExePath, "summary", csvPath, summaryPdfFolder);
        summaryCreation.Start();
        summaryCreation.WaitForExit();
    }

    /// <summary>
    /// Adds summary tables containing information for each individual site to a summary report
    /// </summary>
    /// <param name="sw">The stream writer to write to</param>
    /// <param name="summariesDict">A dictionary containing the information to read from</param>
    private void PrintBySite(StreamWriter sw, Dictionary<string, Dictionary<DateOnly, QualitySummaryModel>> summariesDict)
    {
        // Print values in csv
        foreach (string key in summariesDict.Keys)
        {
            List<DateOnly> dates = new();
            foreach (DateOnly date in summariesDict[key].Keys)
            {
                dates.Add(date);
            }

            dates.Sort();
            dates.Reverse();

            foreach (DateOnly date in dates)
            {
                QualitySummaryModel cur = summariesDict[key][date];
                string toWrite = "";
                if (date.Equals(dates[0]))
                {
                    toWrite += key;
                }
                string dateStr = date.ToString("MMM yy");
                toWrite += $",{dateStr},{cur.Days0},{cur.Days1},{cur.Days2},{cur.Days3},{cur.Days4Plus}," +
                    $"{cur.DurationProblem},{cur.QualityFrontalProblem},{cur.QualityTemporalProblem},{cur.LowFilesProblem}";
                sw.WriteLine(toWrite);
            }
        }
    }

    /// <summary>
    /// Adds summary tables containing information for each individual month to a summary report
    /// </summary>
    /// <param name="sw">The stream writer to write to</param>
    /// <param name="summariesDict">A dictionary containing the information to read from</param>
    private void PrintByMonth(StreamWriter sw, Dictionary<string, Dictionary<DateOnly, QualitySummaryModel>> summariesDict)
    {
        // Dates list
        List<DateOnly> dates = new();

        foreach (string key in summariesDict.Keys)
        {
            foreach (DateOnly date in summariesDict[key].Keys)
            {
                if (dates.Contains(date) == false)
                {
                    dates.Add(date);
                }
            }
        }

        // sort dates list in reverse order
        dates.Sort();
        dates.Reverse();

        // Print values in csv
        foreach (DateOnly printDate in dates)
        {
            bool firstPrinted = false;
            foreach (string key in summariesDict.Keys)
            {
                foreach (DateOnly date in summariesDict[key].Keys)
                {
                    if (date.Equals(printDate) == false) continue;
                    QualitySummaryModel cur = summariesDict[key][date];
                    string toWrite = "";
                    if (firstPrinted == false)
                    {
                        toWrite += printDate.ToDateTime(TimeOnly.MinValue).ToString("MMMM yyyy");
                    }
                    toWrite += $",{key},{cur.Days0},{cur.Days1},{cur.Days2},{cur.Days3},{cur.Days4Plus}," +
                        $"{cur.DurationProblem},{cur.QualityFrontalProblem},{cur.QualityTemporalProblem},{cur.LowFilesProblem}";
                    sw.WriteLine(toWrite);
                    firstPrinted = true;
                }
            }
        }

    }

    /// <summary>
    /// Adds a summary table summarizing all data collections to a summary report
    /// </summary>
    /// <param name="sw">The stream writer to write to</param>
    /// <param name="summariesDict">A dictionary containing the information to read from</param>
    private void PrintAllTime(StreamWriter sw, Dictionary<string, Dictionary<DateOnly, QualitySummaryModel>> summariesDict)
    {
        // Get all time values for each site
        Dictionary<string, QualitySummaryModel> allTimeDict = new();
        foreach (string key in summariesDict.Keys)
        {
            var dict = summariesDict[key];
            allTimeDict.Add(key, new());
            foreach (var reportModel in dict.Values)
            {
                allTimeDict[key].Days0 += reportModel.Days0;
                allTimeDict[key].Days1 += reportModel.Days1;
                allTimeDict[key].Days2 += reportModel.Days2;
                allTimeDict[key].Days3 += reportModel.Days3;
                allTimeDict[key].Days4Plus += reportModel.Days4Plus;
                allTimeDict[key].DurationProblem += reportModel.DurationProblem;
                allTimeDict[key].QualityFrontalProblem += reportModel.QualityFrontalProblem;
                allTimeDict[key].QualityTemporalProblem += reportModel.QualityTemporalProblem;
                allTimeDict[key].LowFilesProblem += reportModel.LowFilesProblem;
            }
        }

        // Print values in csv
        if (allTimeDict.Count > 0)
        {
            QualitySummaryModel first = allTimeDict.Values.First();
            sw.WriteLine($"All FUP3 Collections Summary,{allTimeDict.Keys.First()},{first.Days0},{first.Days1},{first.Days2},{first.Days3},{first.Days4Plus}," +
                $"{first.DurationProblem},{first.QualityFrontalProblem},{first.QualityTemporalProblem},{first.LowFilesProblem}");
        }
        foreach (string key in allTimeDict.Keys.Skip(1))
        {
            QualitySummaryModel cur = allTimeDict[key];
            sw.WriteLine($",{key},{cur.Days0},{cur.Days1},{cur.Days2},{cur.Days3},{cur.Days4Plus}," +
                $"{cur.DurationProblem},{cur.QualityFrontalProblem},{cur.QualityTemporalProblem},{cur.LowFilesProblem}");
        }
    }

    #endregion

    /// <summary>
    /// Creates the summaries dict used to create summary tables
    /// </summary>
    /// <param name="participants">Report quality data for each participant</param>
    /// <param name="lastMonth">The month prior to the current month</param>
    /// <returns>the summaries dict used to create summary tables</returns>
    private Dictionary<string, Dictionary<DateOnly, QualitySummaryModel>> GetSummaryStats(
        List<ParticipantCollectionsQualityModel> participants, DateOnly lastMonth)
    {
        Dictionary<string, Dictionary<DateOnly, QualitySummaryModel>> summariesDict = new();
        foreach (var participant in participants)
        {
            // Skip participant if collection is after the last month being checked
            if (participant.CollectionMonth.CompareTo(lastMonth) >= 0) continue;

            // Set participant site string to their site or "" if null
            string participantSite = participant.Site is not null ? participant.Site  : "";

            // Add site if doesn't exist
            if (summariesDict.Keys is null || summariesDict.Keys.Contains(participantSite) == false)
                summariesDict.Add(participantSite, new());

            // Add Month if doesn't exist
            if (summariesDict[participantSite].Keys is null ||
                summariesDict[participantSite].Keys.Contains(participant.CollectionMonth) == false)
                summariesDict[participantSite].Add(participant.CollectionMonth, new());

            // Update summary data
            QualitySummaryModel summary = summariesDict[participantSite][participant.CollectionMonth];
            if (participant.NumberGoodDays >= 4) summary.Days4Plus++;
            else if (participant.NumberGoodDays == 3) summary.Days3++;
            else if (participant.NumberGoodDays == 2) summary.Days2++;
            else if (participant.NumberGoodDays == 1) summary.Days1++;
            else if (participant.NumberGoodDays == 0) summary.Days0++;
            summary.DurationProblem+= participant.NumberDurationIssues;
            summary.QualityFrontalProblem+= participant.NumberFrontalIssues;
            summary.QualityTemporalProblem+= participant.NumberTemporalIssues;
            if (participant.HasLessThan3Days) summary.LowFilesProblem++;
        }

        return summariesDict;
    }

    /// <summary>
    /// Creates a process to run the report creation python script
    /// </summary>
    /// <param name="pyExePath">The path to the python exe to use</param>
    /// <param name="type">The type (summary or jpg) of report to create. Used as a flag to the py script</param>
    /// <param name="csvPath">The path of the csv file with the infomration needed to create the report</param>
    /// <param name="outputDirPath">The output directory for the pdf file</param>
    /// <returns>A Process that can be run to Create a pdf report (Summary or InDepth)</returns>
    private Process CreatePythonReportProcess(string pyExePath, string type, string csvPath, string outputDirPath)
    {
        string pyFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Python", "MuseQualityReportPdfCreation.py");
        return new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                CreateNoWindow = true,
                UseShellExecute = false,
                Arguments = $"/c {pyExePath} {pyFilePath} \"{type}\" \"{csvPath}\" \"{outputDirPath}\""
            }
        };
    }
}
