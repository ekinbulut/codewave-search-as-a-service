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
        _sut.AdaptorResponse += (o, response) =>
        {
            Assert.Equal(200, response.Code);
        };
        
        await _sut.IndexAsync(new TestIndex()
        {
            Id = 100,
            Value = "Test data value"
        }, "idx_test");
    }

    class TestIndex
    {
        public int Id { get; set; }
        public string? Value { get; set; }
    }
}