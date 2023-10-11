namespace MuseQCApp.Models;

/// <summary>
/// A model for storing summary statistics for a group of participants
/// </summary>
public class QualitySummaryModel
{
    /// <summary>
    /// The number of participants with 0 days of data
    /// </summary>
    public int Days0 { get; set; }

    /// <summary>
    /// The number of participants with 1 day of data
    /// </summary>
    public int Days1 { get; set; }

    /// <summary>
    /// The number of participants with 2 days of data
    /// </summary>
    public int Days2 { get; set; }

    /// <summary>
    /// The number of participants with 3 days of data
    /// </summary>
    public int Days3 { get; set; }

    /// <summary>
    /// The number of participants with 4+ days of data
    /// </summary>
    public int Days4Plus { get; set; }

    /// <summary>
    /// The number of participants that have a duration problem 
    /// NOTE: the same participant can be counted up to once for each of the problem types
    /// </summary>
    public int DurationProblem { get; set; }

    /// <summary>
    /// The number of participants that have a signal quality problem 
    /// NOTE: the same participant can be counted up to once for each of the problem types
    /// </summary>
    public int QualityProblem { get; set; }

    /// <summary>
    /// The number of participants that have fewer than 3 days of collected data
    /// NOTE: the same participant can be counted up to once for each of the problem types
    /// </summary>
    public int LowFilesProblem { get; set; }

}
