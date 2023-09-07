using ElasticsearchAdaptor;
using Microsoft.Extensions.Options;

namespace Tests;

public class ElasticSearchAdaptorTests
{
    private readonly ElasticSearchAdaptor _sut;

    public ElasticSearchAdaptorTests()
    {
        var options = Options.Create(new ElasticSearchOptions()
        {
            Uri = "http://localhost:9200"
        });
        _sut = new ElasticSearchAdaptor(options);
    }

    [Fact]
    public async Task Test_Index()
    {
        /*
         * {
           "DeliveryTag": 0,
           "Datas": [
                       {
                       "TableName": "DEPARTMENT",
                       "Data": [
                       {
                           "ID": 1,
                           "DEPT": "test",
                           "EMP_ID": 345
                       },
                       {
                           "ID": 2,
                           "DEPT": "test2",
                           "EMP_ID": 34
                       }
                    ]
           }
         */

        var datas = new List<Dictionary<string, object>>();
        var columns = new List<Dictionary<string, object>>();
        
        columns.Add(new Dictionary<string, object>()
        {
            {"ID", 1},
            {"DEPT","test"},
            {"EMP_ID",345}
        });
        
        columns.Add(new Dictionary<string, object>()
        {
            {"ID", 2},
            {"DEPT","test2"},
            {"EMP_ID",34}
        });
        
        datas.Add(new Dictionary<string, object>()
        {
            {"TableName","Department"},
            {"Data", columns}
        });

        var data = new
        {
            DeliveryTag = 0,
            Datas = datas
        };
        
        _sut.AdaptorResponse += (o, response) =>
        {
            Assert.Equal(201, response.Code);
        };
        
        await _sut.IndexAsync(new 
        {
            Id = 1,
            Value = data
        }, "idx_test");
    }

    class TestIndex
    {
        public int Id { get; set; }
        public string? Value { get; set; }
    }
}