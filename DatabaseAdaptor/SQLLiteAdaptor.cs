

using System.Data;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace DatabaseAdaptor;

public class SqlLiteAdaptor
{
    private readonly SqliteConnection _connection;
    
    public SqlLiteAdaptor(string connectionString)
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
        catch (Exception e)
        {
            throw new ConnectionException();
        }
        return result;
    }

    public string GetSchemaAndData()
    {
        List<Dictionary<string, object>> tablesData = new List<Dictionary<string, object>>();
        
        using (var schemaCmd = _connection.CreateCommand())
        {
            schemaCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table'";
            using (var reader = schemaCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string tableName = reader.GetString(0);
                    Dictionary<string, object> tableData = new Dictionary<string, object>
                    {
                        { "TableName", tableName },
                        { "Data", GetDataFromTable(tableName) }
                    };
                    tablesData.Add(tableData);
                }
            }
        }
        
        return JsonConvert.SerializeObject(tablesData, Formatting.Indented);
    }

    public List<Dictionary<string, object>> GetDataFromTable(string tableName)
    {
        List<Dictionary<string, object>> tableData = new List<Dictionary<string, object>>();

        using (var dataCmd = _connection.CreateCommand())
        {
            dataCmd.CommandText = $"SELECT * FROM {tableName}";
            using (var dataReader = dataCmd.ExecuteReader())
            {
                while (dataReader.Read())
                {
                    Dictionary<string, object> rowData = new Dictionary<string, object>();
                    for (int i = 0; i < dataReader.FieldCount; i++)
                    {
                        rowData[dataReader.GetName(i)] = dataReader.GetValue(i);
                    }
                    tableData.Add(rowData);
                }
            }
        }

        return tableData;
    }
}
