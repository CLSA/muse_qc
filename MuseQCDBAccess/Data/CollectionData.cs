﻿using MuseQCDBAccess.DbAccess;
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

    public Task<IEnumerable<bool>> CollectionExists(string WID, DateTime StartDT, string PodSerial) =>
        _db.LoadData<bool, dynamic>("collectionBasicInfo_exists", new { WID, StartDT, PodSerial });

    public Task<IEnumerable<bool>> JpgExists(string WID, DateTime StartDT, string PodSerial) =>
        _db.LoadData<bool, dynamic>("jpg_exists", new { WID, StartDT, PodSerial });

    public Task<IEnumerable<bool>> EdfExists(string WID, DateTime StartDT, string PodSerial) =>
        _db.LoadData<bool, dynamic>("edf_exists", new { WID, StartDT, PodSerial });

    public Task UpdateEdfPath(string WID, DateTime StartDT, string PodSerial, string edfFullPath) =>
        _db.SaveData<dynamic>("update_edfPath", new { WID, StartDT, PodSerial, edfFullPath});

    public Task InsertBasicInfo(string WID, DateTime StartDT, float TimeOffset, string PodSerial, DateTime UploadDT, DateTime BasicInfoAddedDT) =>
        _db.SaveData<dynamic>("insert_collectionBasicInfo", new { WID, StartDT, TimeOffset, PodSerial, UploadDT, BasicInfoAddedDT });

    public Task InsertQualityOutputs(string WID, DateTime StartDT, string PodSerial,
        QCStatsModel qc, string JpgPath, bool RealData, bool Problem, DateTime OutputsAddedDT
        ) =>
        _db.SaveData<dynamic>("insert_qualityOutputs", new { 
            WID, StartDT, OutputsAddedDT, PodSerial,
            qc.Dur, qc.Ch1, qc.Ch2, qc.Ch3, qc.Ch4,
            qc.Ch12, qc.Ch13, qc.Ch43, qc.Ch42,
            qc.FAny, qc.FBoth, qc.TAny, qc.TBoth,
            qc.FtAny, qc.EegAny, qc.EegAll,
            JpgPath, RealData, Problem
        });
}
