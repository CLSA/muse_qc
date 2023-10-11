using MuseQCDBAccess.DbAccess;

namespace MuseQCDBAccess.Data;
public class MysqlDBData
{
    public readonly ConfigValsData ConfigVals;
    public readonly ParticipantData Participant;
    public readonly CollectionData Collection;
    public readonly ReportData Report;

    public MysqlDBData(IDataAccess db)
    {
        ConfigVals = new ConfigValsData(db);
        Participant = new ParticipantData(db);
        Collection = new CollectionData(db);
        Report = new ReportData(db);
    }
}
