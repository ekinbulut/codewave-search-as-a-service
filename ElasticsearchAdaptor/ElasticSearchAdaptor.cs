using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Options;

namespace ElasticsearchAdaptor;

public class ElasticSearchResponse
{
    public int Code { get; set; }
    public string? Status { get; set; }
}

public interface IElasticSearchAdaptor
{
    event Action<object, ElasticSearchResponse>? AdaptorResponse;
    void ConfigureSettings(bool enableDebugMode = false, bool prettyJson = false, int minutes = 2);
    Task IndexAsync(object data, string index);
}

public class ElasticSearchAdaptor : IElasticSearchAdaptor
{
    private readonly IOptions<ElasticSearchOptions> _options;
    private readonly ElasticsearchClientSettings? _settings;

    private ElasticsearchClient? _client;

    public event Action<object, ElasticSearchResponse>? AdaptorResponse;

    public ElasticSearchAdaptor(IOptions<ElasticSearchOptions> options)
    {
        _options = options;
        if (_options.Value.Uri != null) _settings = new ElasticsearchClientSettings(new Uri(_options.Value.Uri));
    }

    private ElasticsearchClient? Client
    {
        get
        {
            if (_client == null)
            {
                if (_settings != null) _client = new ElasticsearchClient(_settings);
            }

            return _client;
        }
    }

    public void ConfigureSettings(bool enableDebugMode = false, bool prettyJson = false, int minutes = 2)
    {
        if (enableDebugMode)
        {
            _settings?.EnableDebugMode();
        }
        if (prettyJson)
        {
            _settings?.PrettyJson();
        }
        _settings?.RequestTimeout(TimeSpan.FromMinutes(minutes));
    }

    public async Task IndexAsync(object data, string index)
    {
        var response = await Client?.IndexAsync(data, index)!;
        if (response.IsValidResponse)
        {
            AdaptorResponse?.Invoke(this, new ElasticSearchResponse(){
                Code = Convert.ToInt32($"{response.ApiCallDetails.HttpStatusCode}"),
                Status = $"{index} completed"
            });
        }
        else
        {
            if (response.ElasticsearchServerError != null)
                AdaptorResponse?.Invoke(this, new ElasticSearchResponse()
                {
                    Code = Convert.ToInt32($"{response.ElasticsearchServerError.Status}"),
                    Status = $"{response.ElasticsearchServerError}"
                });
        }
    }

}