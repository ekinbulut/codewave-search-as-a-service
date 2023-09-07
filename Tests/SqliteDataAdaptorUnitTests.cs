using DatabaseAdaptor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tests;

public class SqliteDataAdaptorUnitTests
{
    private SqlLiteAdaptor _sut;

    public SqliteDataAdaptorUnitTests()
    {
        _sut = new SqlLiteAdaptor("Data Source=/Users/ekin/Developer/SaaS/test.db");
    }
    
    
    [Fact]
    public void Connect()
    {
        var actual = _sut.Connect();
        Assert.True(actual);
    }

    [Fact]
    public void Test_GetSchemaAndData()
    {
        if (_sut.Connect())
        {
            var actual = _sut.GetSchemaAndData();
            Assert.False(String.IsNullOrEmpty(actual));
        }
    }

    [Fact]
    public void Test_DataModelConversion()
    {
        if (_sut.Connect())
        {
            var dataStr = _sut.GetSchemaAndData();

          
            var actual = JsonConvert.DeserializeObject<DatabaseModel>(dataStr);

            Assert.IsType<DatabaseModel>(actual);
        }
    }
}