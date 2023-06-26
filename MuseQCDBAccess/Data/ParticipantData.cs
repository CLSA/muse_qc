using MuseQCDBAccess.DbAccess;

namespace MuseQCDBAccess.Data;
public class ParticipantData
{
    private readonly IDataAccess _db;

    public ParticipantData(IDataAccess db)
    {
        _db = db;
    }


    // NOTE: In order for the dynamic types to work with the stored procedures 
    //       in the database, the spelling/casing of the parameters must
    //       match in c# and in the stored procedure

    public Task InsertParticipant(string WID, string PSite) =>
        _db.SaveData<dynamic>("insert_participant", new { WID, PSite });

    public Task InsertWestonID(string WID) =>
        _db.SaveData<dynamic>("insert_westonID", new { WID });

    public Task UpdateSite(string WID, string PSite) =>
        _db.SaveData<dynamic>("update_site", new { WID, PSite });

    public Task<IEnumerable<bool>> WestonIDExists(string WID) =>
        _db.LoadData<bool, dynamic>("westonID_exists", new { WID });

    public Task<IEnumerable<string>> GetParticipantSite(string WID) =>
        _db.LoadData<string, dynamic>("get_participantSite", new { WID });
}
