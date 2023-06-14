using MuseQCApp.Interfaces;
using MuseQCApp.Models;

namespace MuseQCApp.Modules;

public class FileLocations : IFileLocations
{
    public void DecideFilesToDownload(Dictionary<string, object> possibleFilesDict)
    {
        throw new NotImplementedException();
    }

    public ReportInfoModel GetReportInfo(DateTime StartDate, DateTime EndDate)
    {
        throw new NotImplementedException();
    }

    public List<string> NeedsQualityCheck()
    {
        throw new NotImplementedException();
    }
}
