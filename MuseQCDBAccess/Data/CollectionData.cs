using MuseQCDBAccess.DbAccess;
using MuseQCDBAccess.Models;

namespace MuseQCDBAccess.Data;
public class CollectionData
{
    private readonly IDataAccess _db;

    public CollectionData(IDataAccess db)
    {
        _db = db;
    }

    // NOTE: In order for the dynamic types to work with the stored procedures 
    //       in the database, the spelling/casing of the parameters must
    //       match in c# and in the stored procedure

    public Task<IEnumerable<QCStatsModel>> GetStats() =>
        _db.LoadData<QCStatsModel, dynamic>("insert_collectionBasicInfo", new { });

    public Task<IEnumerable<bool>> CoolectionExists(string WID, DateTime StartDateTime, string PodID) =>
        _db.LoadData<bool, dynamic>("collectionBasicInfo_exists", new { WID, StartDateTime, PodID });

    public Task InsertBasicInfo(string WID, DateTime StartDateTime, float TimezoneOffset, string PodID, DateTime UploadDate) =>
        _db.SaveData<dynamic>("insert_collectionBasicInfo", new { WID, StartDateTime, TimezoneOffset, PodID, UploadDate });

    public Task InsertQualityOutputs(string WID, DateTime StartDateTime, string PodID,
        QCStatsModel qc, string JpgPath, bool IsRealData, bool HasProblem
        ) =>
        _db.SaveData<dynamic>("insert_qualityOutputs", new { 
            WID, StartDateTime, PodID,
            qc.Dur, qc.Ch1, qc.Ch2, qc.Ch3, qc.Ch4,
            qc.Ch12, qc.Ch13, qc.Ch43, qc.Ch42,
            qc.FAny, qc.FBoth, qc.TAny, qc.TBoth,
            qc.FtAny, qc.EegAny, qc.EegAll,
            JpgPath, IsRealData, HasProblem
        });
}
