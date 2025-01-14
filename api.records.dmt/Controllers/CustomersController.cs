using Azure.Identity;
using CustomerApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
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
        public async Task<IActionResult> Get()
        {
            var query = _container.GetItemLinqQueryable<Customer>(true);

            // Execute the query with Cosmos DB
            var iterator = query.ToFeedIterator();
            var results = new List<Customer>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
            }

            return Ok(results);
        }
        
        [HttpPut,HttpPatch]
        [Route("odata/customers/update")]
        public async Task<IActionResult> Update([FromBody] List<Customer> customers)
        {
            if (customers == null || customers.Count == 0)
            {
                return BadRequest("Invalid customer data.");
            }

            var responses = new List<Customer>();

            foreach (var customer in customers)
            {
                if (customer == null || string.IsNullOrEmpty(customer.id))
                {
                    return BadRequest("Invalid customer data.");
                }

                try
                {
                    var response = await _container.UpsertItemAsync(customer, new PartitionKey(customer.id));
                    responses.Add(response.Resource);

                    return  StatusCode((int)response.StatusCode);
                }
                catch (CosmosException ex)
                {
                    return StatusCode((int)ex.StatusCode, ex.Message);
                }
            }

           return new OkResult();
        }
    }
}