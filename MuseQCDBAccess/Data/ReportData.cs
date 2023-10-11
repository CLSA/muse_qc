using MuseQCDBAccess.DbAccess;
using MuseQCDBAccess.Models;

namespace MuseQCDBAccess.Data;
public class ReportData
{
    private readonly IDataAccess _db;

    public ReportData(IDataAccess db)
    {
        _db = db;
    }

    // NOTE: In order for the dynamic types to work with the stored procedures 
    //       in the database, the spelling/casing of the parameters must
    //       match in c# and in the stored procedure
    public Task<IEnumerable<EegQualityReportModel>> GetQualityReportData() =>
       _db.LoadData<EegQualityReportModel, dynamic>("get_qualityReportData", new { });
}
