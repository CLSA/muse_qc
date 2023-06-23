namespace MuseQCDBAccess.DbAccess;

public interface IDataAccess
{
    Task<IEnumerable<T>> LoadData<T, U>(string storedProcedure, U parameters, string connectionID = "Default");
    Task SaveData<T>(string storedProcedure, T parameters, string connectionID = "Default");
}