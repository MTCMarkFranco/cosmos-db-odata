using Azure.Identity;
using CustomerApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerApi.Controllers
{
    public class CustomersController : ODataController
    {
        private readonly Container _container;

        public CustomersController(IConfiguration configuration)
        {
            var accountEndpoint = configuration["CosmosDb:AccountEndpoint"] ?? throw new ArgumentNullException("CosmosDb:AccountEndpoint");
            var databaseName = configuration["CosmosDb:DatabaseName"] ?? throw new ArgumentNullException("CosmosDb:DatabaseName");
            var containerName = configuration["CosmosDb:ContainerName"] ?? throw new ArgumentNullException("CosmosDb:ContainerName");

            var credential = new DefaultAzureCredential();
            var cosmosClient = new CosmosClient(accountEndpoint, credential);
            var database = cosmosClient.GetDatabase(databaseName);
            _container = database.GetContainer(containerName);
        }

        [EnableQuery]
        [HttpGet]
        public async Task<IActionResult> Get(ODataQueryOptions<Customer> queryOptions)
        {
            var query = _container.GetItemLinqQueryable<Customer>(true);

            // Apply OData query options
            var appliedQuery = (IQueryable<Customer>)queryOptions.ApplyTo(query);

            // Execute the query with Cosmos DB
            var iterator = appliedQuery.ToFeedIterator();
            var results = new List<Customer>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
            }

            return Ok(results);
        }
    }
}