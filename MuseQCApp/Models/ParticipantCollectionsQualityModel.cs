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
    public int NumberGoodDays => EegQualityReportModels.Where(x => x.HasProblem == false).Count();

    /// <summary>
    /// The number of days of data total good or bad quality
    /// </summary>
    public int NumberCollections => EegQualityReportModels.Count;

    public int NumberDurationIssues => EegQualityReportModels.Where(x => x.LessThan5HoursDuration == true).Count();
    public int NumberFrontalIssues => EegQualityReportModels.Where(x => x.FrontalProblem == true).Count();
    public int NumberTemporalIssues => EegQualityReportModels.Where(x => x.TemporalProblem == true).Count();

    /// <summary>
    /// True if there are less than 3 days of collected data for this participant, false otherwise
    /// </summary>
    public bool HasLessThan3Days => NumberCollections < 3;

    public bool HasDataProblem => EegQualityReportModels.Where(x => x.LessThan5HoursDuration || x.FrontalProblem || x.TemporalProblem).Count() > 0;

    /// <summary>
    /// True if this participant has atleast one file with a duration or 
    /// signal quality problem or if the participant has less than 3 total 
    /// collections. False otherwise
    /// </summary>
    public bool HasAtleastOneProblem => NumberGoodDays < 3
        && (HasLessThan3Days || HasDataProblem);

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
