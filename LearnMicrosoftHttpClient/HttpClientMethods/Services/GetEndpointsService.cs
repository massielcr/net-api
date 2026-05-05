using System.Text.Json;

namespace HttpClientMethods.Services
{
    public class GetEndpointsService(IHttpClientFactory clientFactory) : IGetEndpointsService
    {

        public async Task<int> GetRepositoriesCountAsync()
        {
            var client = clientFactory.CreateClient();

            client.BaseAddress = new Uri("https://api.github.com/");
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "MyTestService");

            string relativeUri = $"orgs/dotnet/repos";            

            using HttpResponseMessage response = await client.GetAsync(relativeUri);

            response.EnsureSuccessStatusCode();

            using Stream stream = await response.Content.ReadAsStreamAsync();

            IEnumerable<JsonElement>? repositories = await JsonSerializer.DeserializeAsync<IEnumerable<JsonElement>>(stream);

            return repositories?.Count() ?? 0;
        }

        public async Task<IEnumerable<string>> GetAllRepositoriesAsync()
        {
            var client = clientFactory.CreateClient();

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "MyTestService");

            Uri uri = new("https://api.github.com/orgs/dotnet/repos");

            using HttpResponseMessage response = await client.GetAsync(uri);

            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();

            var repositories = await JsonSerializer.DeserializeAsync<IEnumerable<JsonElement>>(stream);

            return repositories?
                    .Select(repo => repo.GetProperty("name").GetString() ?? string.Empty)
                    .OrderBy(name => name)
                    .Select((name, index) => $"{index + 1} -  {name}")
                    .ToList() ?? [];
        }
    }
}
