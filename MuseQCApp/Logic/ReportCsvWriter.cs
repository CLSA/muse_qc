using Microsoft.Extensions.Logging;
using MuseQCApp.Models;

namespace MuseQCApp.Logic;
public class ReportCsvWriter
{
    #region private properties

    /// <summary>
    /// The logger to use
    /// </summary>
    private ILogger Logging { get; init; }

    #endregion

    public ReportCsvWriter(ILogger logging)
    {
        Logging = logging;
    }

    /// <summary>
    /// Create all report csv files
    /// </summary>
    /// <param name="participants">Report quality data for each participant</param>
    /// <param name="outputFolder">The folder to write output csv files to</param>
    public void CreateReportCsvs(List<ParticipantCollectionsQualityModel> participants, string outputFolder)
    {
        string month = DateTime.Now.AddMonths(-1).ToString("MMMM_yyyy");
        string csvPath = Path.Combine(outputFolder, $"QualityReport_{month}.csv");

        if (File.Exists(csvPath)) return;

        Dictionary<string, Dictionary<DateOnly, QualitySummaryModel>> summariesDict = GetSummaryStats(participants);

        using StreamWriter sw = new(csvPath);

        // Print Header
        sw.WriteLine("Summary Table Title,Category,0 Days,1 Days,2 Days,3 Days,4+ Days,Duration,Signal Quality,>3 Collected");

        PrintAllTime(sw, summariesDict);
        PrintByMonth(sw, summariesDict);
        PrintBySite(sw, summariesDict);
    }

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
                toWrite += $",{date},{cur.Days0},{cur.Days1},{cur.Days2},{cur.Days3},{cur.Days4Plus}," +
                    $"{cur.DurationProblem},{cur.QualityProblem},{cur.LowFilesProblem}";
                sw.WriteLine(toWrite);
            }
        }
    }

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
        foreach(DateOnly printDate in dates)
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
                        toWrite += printDate;
                    }
                    toWrite += $",{key},{cur.Days0},{cur.Days1},{cur.Days2},{cur.Days3},{cur.Days4Plus}," +
                        $"{cur.DurationProblem},{cur.QualityProblem},{cur.LowFilesProblem}";
                    sw.WriteLine(toWrite);
                    firstPrinted = true;
                }
            }
        }
        
    }

    private void PrintAllTime(StreamWriter sw, Dictionary<string, Dictionary<DateOnly, QualitySummaryModel>> summariesDict)
    {
        // Get all time values for each site
        Dictionary<string, QualitySummaryModel> allTimeDict = new();
        foreach(string key in summariesDict.Keys)
        {
            var dict = summariesDict[key];
            allTimeDict.Add(key, new());
            foreach(var reportModel in dict.Values)
            {
                allTimeDict[key].Days0 += reportModel.Days0;
                allTimeDict[key].Days1 += reportModel.Days1;
                allTimeDict[key].Days2 += reportModel.Days2;
                allTimeDict[key].Days3 += reportModel.Days3;
                allTimeDict[key].Days4Plus += reportModel.Days4Plus;
                allTimeDict[key].DurationProblem += reportModel.DurationProblem;
                allTimeDict[key].QualityProblem += reportModel.QualityProblem;
                allTimeDict[key].LowFilesProblem += reportModel.LowFilesProblem;
            }
        }

        // Print values in csv
        if (allTimeDict.Count > 0)
        {
            QualitySummaryModel first = allTimeDict.Values.First();
            sw.WriteLine($"AllTime,{allTimeDict.Keys.First()},{first.Days0},{first.Days1},{first.Days2},{first.Days3},{first.Days4Plus}," +
                $"{first.DurationProblem},{first.QualityProblem},{first.LowFilesProblem}");
        }
        foreach(string key in allTimeDict.Keys.Skip(1))
        {
            QualitySummaryModel cur = allTimeDict[key];
            sw.WriteLine($",{key},{cur.Days0},{cur.Days1},{cur.Days2},{cur.Days3},{cur.Days4Plus}," +
                $"{cur.DurationProblem},{cur.QualityProblem},{cur.LowFilesProblem}");
        }
    }

    private Dictionary<string, Dictionary<DateOnly, QualitySummaryModel>> GetSummaryStats(List<ParticipantCollectionsQualityModel> participants)
    {
        Dictionary<string, Dictionary<DateOnly, QualitySummaryModel>> summariesDict = new();
        foreach (var participant in participants)
        {
            // Set participant site string to their site or "" if null
            string participantSite = participant.Site is not null ? participant.Site  : "";

            // Add site if doesn't exist
            // TODO: Why is this throwing null???????????????????????????????????
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
            if (participant.HasAtLeast1DurationIssue) summary.DurationProblem++;
            if (participant.HasAtLeast1QualityIssue) summary.QualityProblem++;
            if (participant.HasLessThan3Days) summary.LowFilesProblem++;
        }

        return summariesDict;
    }
}
