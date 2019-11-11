using Nest;
using System;

namespace ElasticSearch.Services
{
  public class ElasticSearchService: IElasticSearchService
  {
    public static ElasticClient client { get; set; }
    public ElasticSearchService()
    {
      var settings = new ConnectionSettings(new Uri("http://localhost:9200")).DefaultIndex("people");
      client = new ElasticClient(settings);
    }
    public ElasticClient getClient()
    {
      return client;
    }
  }

  public interface IElasticSearchService 
  {
    ElasticClient getClient();
  }
}