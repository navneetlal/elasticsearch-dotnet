using Microsoft.AspNetCore.Mvc;
using ElasticSearch.Services;
using ElasticSearch.Models;

namespace ElasticSearch.Controllers
{

  [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly IElasticSearchService elasticSearchService;
        private Nest.ElasticClient client;

        public HomeController(IElasticSearchService _elasticSearchService)
        {
            elasticSearchService = _elasticSearchService;
            client = elasticSearchService.getClient();
        }

        [HttpGet()]
        public IActionResult Get([FromQuery] string key)
        {

            var searchResponse = client.Search<Person>(s => s
                .From(0)
                .Size(10)
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.FirstName)
                        .Query(key)
                    )
                )
            );

            var people = searchResponse.Documents;

            return Ok(people);
  
        }

        [HttpPost]
        public IActionResult Post(Person person)
        {
            var indexResponse = client.IndexDocument(person);
            return Ok(indexResponse);
        }
    }


}
