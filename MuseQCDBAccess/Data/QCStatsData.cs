using MuseQCDBAccess.DbAccess;
using MuseQCDBAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuseQCDBAccess.Data;
public class QCStatsData
{
    private readonly IDataAccess _db;

    public QCStatsData(IDataAccess db)
    {
        _db = db;
    }

    public Task<IEnumerable<QCStatsModel>> GetStats() =>
        _db.LoadData<QCStatsModel, dynamic>("getQCStats", new { });
}
