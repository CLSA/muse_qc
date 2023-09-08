using MuseQCApp.Interfaces;
using MuseQCDBAccess.Models;

namespace MuseQCApp.Modules;
public class MuseQualityDecisionsV1 : IMuseQualityDecisions
{
    public int GetVersionNumber()
    {
        return 1;
    }

    public bool HasDurationProblem(QCStatsModel stats)
    {
        // Assumptions:
        // 1. Must be an actual night of data in order to have a duration problem
        // 2. A duration of 6 hours or above is considered a good night
        // NOTE: Ft.Any of >= 0.8 is the threshold
        return IsActualNight(stats) && stats.Dur < 6;
    }

    public bool HasQualityProblem(QCStatsModel stats)
    {
        // Assumptions:
        // 1. Must be an actual night of data in order to have a duration problem
        // 2. A threshold of 0.8 or above is acceptable for FT.any
        // NOTE: Ft.Any of >= 0.8 is the threshold
        return IsActualNight(stats) && stats.FtAny < 0.8;
    }

    public bool IsActualNight(QCStatsModel stats)
    {
        // If the recording is less than 10 mins, then it is not an attempted night
        return stats.Dur > 1.0/6.0;
    }
}
