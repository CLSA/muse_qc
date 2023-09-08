using MuseQCDBAccess.Models;

namespace MuseQCApp.Models;
public class MuseQualityResultsModel
{
    public QCStatsModel QcStats { get; init; }
    public string FileName { get; init; }
    public string WestonId { get; init; }
    public string PodSerial { get; init; }
    public DateTime StartDate { get; init; }
    public bool RealData { get; init; }
    public bool DurProblem { get; init; }
    public bool QualityProblem { get; init; }
    public int MuseQualityVersion { get; init; }
    public string NewJpgPath { get; init; }


    public MuseQualityResultsModel(QCStatsModel qc, string filename, string westonID, string podSerial, 
        DateTime start, bool real, bool durProblem, bool qualityProblem, int museQualityVersion, string newJpgPath)
    {
        QcStats = qc;
        FileName = filename;
        WestonId = westonID;
        PodSerial = podSerial;
        StartDate = start;
        RealData = real;
        DurProblem = durProblem;
        QualityProblem = qualityProblem;
        MuseQualityVersion = museQualityVersion;
        NewJpgPath = newJpgPath;
    }
}
