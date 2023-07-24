using MuseQCApp.Interfaces;
using MuseQCDBAccess.Models;

namespace MuseQCApp.Modules;
public class MuseQualityDecisionsV1 : IMuseQualityDecisions
{
    public int GetVersionNumber()
    {
        return 1;
    }

    public bool HasProblem(QCStatsModel stats)
    {
        // Assumptions:
        // 1. Must be an actual night of data in order to have a problem
        // 2. A threshold of 0.8 or above is acceptable for FT.any
        // 3. A duration of 6 hours or above is considered a good night
        // NOTE: Ft.Any of >= 0.8 is the threshold
        return IsActualNight(stats) && (stats.FtAny < 0.8 || stats.Dur < 6);
    }

    public bool IsActualNight(QCStatsModel stats)
    {
        return stats.Dur > 1;
    }
}
