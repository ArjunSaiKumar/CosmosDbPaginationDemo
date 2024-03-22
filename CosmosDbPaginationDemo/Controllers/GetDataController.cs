using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Container = Microsoft.Azure.Cosmos.Container;

namespace CosmosDbPaginationDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GetDataController : ControllerBase
    {
        private static readonly string EndpointUri = "https://localhost:8081";
        private static readonly string PrimaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        private static readonly string DatabaseName = "Test";
        private static readonly string ContainerName = "Cities";

        private static CosmosClient cosmosClient;
        private static Database database;
        private static Container container;

        public async Task<IActionResult> GetData(int age, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                CosmosDBManager cosmosDBManager = new CosmosDBManager(EndpointUri, PrimaryKey, DatabaseName, ContainerName);
                var data = await cosmosDBManager.GetStudentsAsync(age, pageNumber, pageSize);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
