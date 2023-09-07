using Newtonsoft.Json;

namespace DatabaseAdaptor;

public class DatabaseModel
{
    public List<Table> Tables { get; set; }

    public DatabaseModel()
    {
        Tables = new List<Table>();
    }
}

public class Data
{
    public Data(string name, object? value)
    {
        Name = name;
        Value = value.ToString();
    }

    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("value")]
    public string? Value { get; set; }
}

public class Table
{
    public string TableName { get; set; }
    [JsonProperty("Data")]
    public List<Data> Datas { get; set; }

    public Table()
    {
        Datas = new List<Data>();
    }
}