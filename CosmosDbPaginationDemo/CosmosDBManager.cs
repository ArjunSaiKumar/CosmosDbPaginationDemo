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
        public async Task<object> GetStudents2Async(int age, int pageNumber = 1, int pageSize = 10)
        {
            var students = new List<Student>();
            try
            {
                string continuationToken = null;
                int size = pageNumber * pageSize;
                QueryDefinition query = new QueryDefinition($"SELECT VALUE count(1) FROM root WHERE root.Age = {age}");

                var iterator = _container.GetItemQueryIterator<int>(
                    query,
                    requestOptions: new QueryRequestOptions { MaxItemCount = 1, MaxConcurrency = -1 });

                if (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    if (response.Count > 0)
                    {
                        // Check if the response is a valid integer
                        if (response.FirstOrDefault() is int countValue)
                        {
                            return countValue;
                        }
                        else
                        {
                            // Response is not in the expected format
                            throw new Exception("Unexpected response format: Response is not a valid integer.");
                        }
                    }
                    else
                    {
                        // No results returned by the query
                        return 0;
                    }
                }
                else
                {
                    // No more results available
                    return 0;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                // Handle the exception as needed
            }

            return students;
        }

    }
}
