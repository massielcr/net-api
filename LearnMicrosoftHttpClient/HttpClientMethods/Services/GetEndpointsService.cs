using System.Text.Json;

namespace HttpClientMethods.Services
{
    public class GetEndpointsService(HttpClient httpClient) : IGetEndpointsService
    {
        public async Task<IEnumerable<string>> GetRepositoriesStringAsync(string username)
        {
            string relativeUri = $"users/{username}/repos";

            using HttpResponseMessage response = await httpClient.GetAsync(relativeUri);

            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();

            var repositories = await JsonSerializer.DeserializeAsync<IEnumerable<JsonElement>>(stream);

            return repositories?
                    .Select(repo => repo.GetProperty("name").GetString() ?? string.Empty)
                    .ToList() ?? [];
        }
    }
}
