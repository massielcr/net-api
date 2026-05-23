using System.Text;
using System.Text.Json;

namespace HttpClientMethods.Services
{
    public class PostAsyncEndpointsService : IPostAsyncEndpointsService
    {
        private const string BaseUrl = "https://api.github.com/";
        private readonly string? _githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");

        private readonly static HttpClient _httpClient = new();

        
        public async Task<bool> CreatePersonalRepoAsync(string repoName, string description, bool isPrivate, bool initialCommit)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "MyTestService");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_githubToken}");

            Uri uri = new($"{BaseUrl}user/repos");

            var repoSetup = new
            {
                name = repoName,
                description,
                @private = isPrivate,
                auto_init = initialCommit
            };

            string repoSetupJason = JsonSerializer.Serialize(repoSetup);

            using StringContent content = new(repoSetupJason, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(uri, content);

            return response.IsSuccessStatusCode;
        }
    }
}
