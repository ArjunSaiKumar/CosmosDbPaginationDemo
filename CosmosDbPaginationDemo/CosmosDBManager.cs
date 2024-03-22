using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace CosmosDbPaginationDemo
{
    public class CosmosDBManager
    {
        private readonly Container _container;

        public CosmosDBManager(string endpoint,string connectionString, string databaseName, string containerName)
        {
            var client = new CosmosClient(endpoint, connectionString);
            _container = client.GetContainer(databaseName, containerName);
        }

        public async Task<List<Student>> GetStudentsAsync(int age, int pageNumber = 1, int pageSize = 10)
        {
            var students = new List<Student>();
            try
            {
                int size = pageNumber * pageSize;
                QueryDefinition query = new QueryDefinition($"SELECT * FROM root WHERE (root.Age = @id) OFFSET {size} LIMIT {pageSize}")
               .WithParameter("@id", age);

                var iterator = _container.GetItemQueryIterator<Student>(
                    query,
                    requestOptions: new QueryRequestOptions { MaxItemCount = pageSize });

                FeedResponse<Student> response = null;

                if (iterator.HasMoreResults)
                    response = await iterator.ReadNextAsync();

                if (response != null)
                    students.AddRange(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                // Handle the exception as needed
            }

            return students;
        }
        public async Task<List<Student>> GetStudents2Async(int pageNumber, int pageSize)
        {
            var students = new List<Student>();
            try
            {
                var continuationToken = CalculateContinuationToken(pageNumber, pageSize);
                if (string.IsNullOrEmpty(continuationToken))
                {
                    Console.WriteLine("Invalid page number or page size.");
                    return students;
                }

                var iterator = _container.GetItemQueryIterator<Student>(
                    "SELECT * FROM c",
                    continuationToken,
                    new QueryRequestOptions { MaxItemCount = pageSize });

                var response = await iterator.ReadNextAsync();
                students.AddRange(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                // Handle the exception as needed
            }

            return students;
        }

        private string CalculateContinuationToken(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                return null;

            int skip = (pageNumber - 1) * pageSize;

            return ToContinuationToken(skip);
        }

        private string ToContinuationToken(int skip)
        {
            return JsonConvert.SerializeObject(new { skip });
        }
    }
}
