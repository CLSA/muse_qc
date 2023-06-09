﻿using MuseQCDBAccess.DbAccess;

namespace MuseQCDBAccess.Data;
public class ConfigValsData
{
    private readonly IDataAccess _db;

    public ConfigValsData(IDataAccess db)
    {
        _db = db;
    }

    // NOTE: In order for the dynamic types to work with the stored procedures 
    //       in the database, the spelling/casing of the parameters must
    //       match in c# and in the stored procedure

    public Task<IEnumerable<DateTime?>> GetLastDateDownloaded() =>
        _db.LoadData<DateTime?, dynamic>("get_lastDateTimeDownloaded", new { });
}
