using MuseQCDBAccess.Models;

namespace MuseQCApp.Models;

/// <summary>
/// A model to store all of the eeg report quality data for a single participant
/// </summary>
public class ParticipantCollectionsQualityModel
{
    /// <summary>
    /// A list of the eeg report quality for each collection from this participant
    /// </summary>
    public List<EegQualityReportModel> EegQualityReportModels { get; init; }

    /// <summary>
    /// The WestonID of the participant
    /// </summary>
    public string WestonID
    {
        get
        {
            if (EegQualityReportModels.Count() == 0)
            {
                return string.Empty;
            }

            return EegQualityReportModels[0].WestonID.ToLower();
        }
    }

    /// <summary>
    /// The participants site where they went for in person assessments to recieve device
    /// </summary>
    public string Site 
    { 
        get
        {
            if(EegQualityReportModels.Count() == 0)
            {
                return string.Empty;
            }

            return EegQualityReportModels[0].Site;
        } 
    }

    /// <summary>
    /// The number of days of good quality data where no issues occured
    /// </summary>
    public int NumberGoodDays => EegQualityReportModels.Where(x => x.HasDurationProblem == false && x.HasQualityProblem == false).Count();

    /// <summary>
    /// The number of days of data total good or bad quality
    /// </summary>
    public int NumberDays => EegQualityReportModels.Count;

    /// <summary>
    /// True if at least one of the days of data has a duration issue, false otherwise
    /// </summary>
    public bool HasAtLeast1DurationIssue => EegQualityReportModels.Where(x => x.LessThan5HoursDuration && NumberGoodDays < 3).Count() > 0;

    /// <summary>
    /// True if at least one of the days of data has a signal quality issue, false otherwise
    /// </summary>
    public bool HasAtLeast1QualityIssue => EegQualityReportModels.Where(x => x.HasQualityProblem && NumberGoodDays < 3).Count() > 0;

    /// <summary>
    /// True if there are less than 3 days of collected data for this participant, false otherwise
    /// </summary>
    public bool HasLessThan3Days => NumberDays < 3;

    /// <summary>
    /// True if this participant has atleast one file with a duration or 
    /// signal quality problem or if the participant has less than 3 total 
    /// collections. False otherwise
    /// </summary>
    public bool HasAtleastOneProblem => 
        HasAtLeast1DurationIssue || HasAtLeast1QualityIssue || HasLessThan3Days;

    /// <summary>
    /// True if any file has a start date 10 days later than the previously collected file
    /// </summary>
    public bool HasFileWithQuestionableDate
    {
        get
        {
            if (EegQualityReportModels.Count() == 0)
            {
                return true;
            }

            DateTime previousDate = EegQualityReportModels[0].StartDateTime;
            foreach (var qualityReport in EegQualityReportModels.Skip(1))
            {
                DateTime curDate = qualityReport.StartDateTime;
                double daysBetween = (curDate - previousDate).TotalDays;
                if (daysBetween > 10)
                {
                    return true;
                }
                previousDate = curDate;
            }

            return false;
        }
    }

    /// <summary>
    /// The last day of data collection as a DateOnly, 
    /// or Null if there is a file with a questionable date
    /// </summary>
    private DateOnly? LastCollectionDay => HasFileWithQuestionableDate ? null
        : DateOnly.FromDateTime(EegQualityReportModels.Last().StartDateTime);

    /// <summary>
    /// The collection month this data should be included in, otherwise min value
    /// </summary>
    public DateOnly CollectionMonth => LastCollectionDay is null ? DateOnly.MinValue 
        : new DateOnly(LastCollectionDay.Value.Year, LastCollectionDay.Value.Month, 1);

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="eegQualityModels">All of the eeg models for the participant</param>
    public ParticipantCollectionsQualityModel(List<EegQualityReportModel> eegQualityModels)
    {
        eegQualityModels.OrderBy(x => x.StartDateTime);
        EegQualityReportModels = eegQualityModels;
    }
}
