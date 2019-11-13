using Microsoft.AspNetCore.Mvc;
using System;

using ElasticSearch.Services;
using ElasticSearch.Models;

namespace ElasticSearch.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly IElasticSearchService elasticSearchService;
        private Nest.ElasticClient client;

        private IRabbitManager manager;

        public SearchController(IElasticSearchService _elasticSearchService, IRabbitManager _manager)
        {
            elasticSearchService = _elasticSearchService;
            client = elasticSearchService.getClient();

            manager = _manager;
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
            Console.WriteLine("message: ");
            var indexResponse = client.IndexDocument(person);
            manager.Publish(person, "Boards_Exchange", "topic", "kibin");
            return Ok(indexResponse);
        }
    }


}
