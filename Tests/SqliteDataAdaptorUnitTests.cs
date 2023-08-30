using DatabaseAdaptor;

namespace Tests;

public class SqliteDataAdaptorUnitTests
{
    private SqlLiteAdaptor _sut;

    public SqliteDataAdaptorUnitTests()
    {
        _sut = new SqlLiteAdaptor("Data Source=test.db");
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
}