using System.Text.Json;

namespace HttpClientMethods.Services
{
    public class GetByteArrayAsyncEndpointsService(IHttpClientFactory httpClientFactory) : IGetByteArrayAsyncEndpointsService
    {
        private const string BaseUrl = "https://api.github.com/";
        private readonly string? _githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");

        public async Task<byte[]> DownloadLogoStringAsync(string username)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("GitHub");

            httpClient.BaseAddress = new Uri(BaseUrl);

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "MyTestService");
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_githubToken}");

            string relativeUri = $"users/{username}";

            using HttpResponseMessage response = await httpClient.GetAsync(relativeUri);

            if (!response.IsSuccessStatusCode) { return []; }

            string responseContent = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseContent);

            if (!doc.RootElement.TryGetProperty("avatar_url", out var avatarProp)) { return []; }

            string? avatarUrl = avatarProp.GetString();

            if (string.IsNullOrEmpty(avatarUrl)) { return []; }  

            byte[] data = await httpClient.GetByteArrayAsync(avatarUrl);

            return data;
        }

        public Task<byte[]> DownloadLogoUriAsync(string username)
        {
            throw new NotImplementedException();
        }
    }
}
