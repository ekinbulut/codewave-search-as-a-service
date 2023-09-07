

using System.Data;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace DatabaseAdaptor;

public interface IAdaptor
{
    bool Connect();
    string GetSchemaAndData();
    DatabaseModel GetDatabaseModel();
}

public class SqlLiteAdaptor : IAdaptor
{
    private readonly SqliteConnection _connection;
    
    public SqlLiteAdaptor(string? connectionString)
    {
        _connection = new SqliteConnection(connectionString);
    }

    public bool Connect()
    {
        bool result;
        try
        {
            _connection.Open();
            result = true;
        }
        catch
        {
            throw new ConnectionException();
        }
        return result;
    }

    public string GetSchemaAndData()
    {
        var databaseModel = new DatabaseModel();
        using (var schemaCmd = _connection.CreateCommand())
        {
            schemaCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table'";
            using (var reader = schemaCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string tableName = reader.GetString(0);
                    
                    databaseModel.Tables.Add(new Table()
                    {
                        TableName =  tableName,
                        Datas = GetDataFromTable(tableName)
                    });
                }
            }
        }
        
        return JsonConvert.SerializeObject(databaseModel);
    }

    public DatabaseModel GetDatabaseModel()
    {
        var databaseModel = new DatabaseModel();
        using (var schemaCmd = _connection.CreateCommand())
        {
            schemaCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table'";
            using (var reader = schemaCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string tableName = reader.GetString(0);
                    
                    databaseModel.Tables.Add(new Table()
                    {
                        TableName =  tableName,
                        Datas = GetDataFromTable(tableName)
                    });
                }
            }
        }

        return databaseModel;
    }

    private List<Data> GetDataFromTable(string tableName)
    {
        List<Dictionary<string, object>> tableData = new List<Dictionary<string, object>>();

        var datas = new List<Data>();
        
        using (var dataCmd = _connection.CreateCommand())
        {
            dataCmd.CommandText = $"SELECT * FROM {tableName}";
            using (var dataReader = dataCmd.ExecuteReader())
            {
                while (dataReader.Read())
                {
                    for (int i = 0; i < dataReader.FieldCount; i++)
                    {
                        datas.Add(new Data(dataReader.GetName(i),dataReader.GetValue(i)));
                    }
                }
            }
        }

        return datas;
    }
}
