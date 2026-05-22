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

            try
            {
                using HttpResponseMessage response = await httpClient.GetAsync(relativeUri);

                if (!response.IsSuccessStatusCode) { return []; }

                using Stream responseStream = await response.Content.ReadAsStreamAsync();

                if (responseStream == null) { return []; }

                using JsonDocument responseJson = await JsonDocument.ParseAsync(responseStream);

                if (!responseJson.RootElement.TryGetProperty("avatar_url", out var avatarProp)) { return []; }

                string? avatarUrl = avatarProp.GetString();

                if (string.IsNullOrEmpty(avatarUrl)) { return []; }

                byte[] data = await httpClient.GetByteArrayAsync(avatarUrl);

                return data;

            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"The requestUri is not an absolute URI and BaseAddress isn't set.{ex.Message}");
            }
            catch (UriFormatException ex)
            {
                Console.WriteLine($"The provided request URI is not valid relative or absolute URI: {ex.Message}");
            }
            catch(HttpRequestException ex)
            {
                Console.WriteLine($"The request failed due to an issue getting a valid HTTP response, such as network connectivity failure, DNS failure, server certificate validation error, or invalid server response: {ex.Message}");
                Console.WriteLine($".NET Framework only: the request timed out. {ex.Message}");
            }
            catch(OperationCanceledException ex)
            {
                Console.WriteLine($".NET Core and .NET 5 and later only: The request failed due to timeout. {ex.Message}");
            }

            return [];
        }

        public Task<byte[]> DownloadLogoUriAsync(string username)
        {
            throw new NotImplementedException();
        }
    }
}
