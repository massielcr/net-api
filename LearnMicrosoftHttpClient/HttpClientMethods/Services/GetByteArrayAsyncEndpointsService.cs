using HttpClientMethods.Helpers;
using HttpClientMethods.Models;
using System.Text.Json;

namespace HttpClientMethods.Services
{
    public class GetByteArrayAsyncEndpointsService(IHttpClientFactory httpClientFactory, IFileService fileService, ILogger<GetByteArrayAsyncEndpointsService> logger) : IGetByteArrayAsyncEndpointsService
    {
        private const string BaseUrl = "https://api.github.com";

        public async Task<GitHubAvatar?> GetAvatarStringAsync(string username)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("GitHub");

            string relativeUri = $"users/{username}";

            try
            {
                using HttpResponseMessage response = await httpClient.GetAsync(relativeUri);

                if (!response.IsSuccessStatusCode) { return null; }

                using Stream responseStream = await response.Content.ReadAsStreamAsync();

                using JsonDocument responseJson = await JsonDocument.ParseAsync(responseStream);

                if (!responseJson.RootElement.TryGetProperty("avatar_url", out var avatarProp)) { return null; }

                string? avatarUrl = avatarProp.GetString();

                if (string.IsNullOrEmpty(avatarUrl)) { return null; }

                HttpClient imageClient = httpClientFactory.CreateClient();

                byte[] data = await imageClient.GetByteArrayAsync(avatarUrl);

                if (data == null || data.Length == 0) { return null; }

                return new GitHubAvatar
                {
                    Data = data,
                    ContentType = fileService.GetMimeTypeFromBytes(data),
                    ContentLength = data.Length
                };

            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, $"The requestUri is not an absolute URI and BaseAddress isn't set.");
            }
            catch (UriFormatException ex)
            {
                logger.LogError(ex, $"The provided request URI is not valid relative or absolute URI.");
            }
            catch(HttpRequestException ex)
            {
                logger.LogError(ex, $"The request failed due to an issue getting a valid HTTP response, such as network connectivity failure, DNS failure, server certificate validation error, or invalid server response");
                logger.LogError(ex, $".NET Framework only: the request timed out.");
            }
            catch(OperationCanceledException ex)
            {
                logger.LogError(ex, $".NET Core and .NET 5 and later only: The request failed due to timeout.");
            }

            return null;
        }

        public async Task<GitHubAvatar?> GetAvatarUriAsync(string username)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("GitHub");

            Uri uri = new($"{BaseUrl}/users/{username}");

            try
            {
                using HttpResponseMessage response = await httpClient.GetAsync(uri);

                if (!response.IsSuccessStatusCode) { return null; }

                using Stream responseStream = await response.Content.ReadAsStreamAsync();

                using JsonDocument responseJson = await JsonDocument.ParseAsync(responseStream);

                if (!responseJson.RootElement.TryGetProperty("avatar_url", out var avatarProp)) { return null; }

                string? avatarUrl = avatarProp.GetString();

                if (string.IsNullOrEmpty(avatarUrl)) { return null; }

                HttpClient imageClient = httpClientFactory.CreateClient();

                Uri imageUri = new(avatarUrl);

                byte[] data = await imageClient.GetByteArrayAsync(imageUri);

                if (data == null || data.Length == 0) { return null; }

                return new GitHubAvatar
                {
                    Data = data,
                    ContentType = fileService.GetMimeTypeFromBytes(data),
                    ContentLength = data.Length
                };
            }
            catch(InvalidOperationException ex)
            {
                logger.LogError(ex, $"The requestUri is not an absolute URI and BaseAddress isn't set.");
            }
            catch (UriFormatException ex)
            {
                logger.LogError(ex, $"The provided request URI is not valid relative or absolute URI.");
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, $"The request failed due to an issue getting a valid HTTP response, such as network connectivity failure, DNS failure, server certificate validation error, or invalid server response.");
                logger.LogError(ex, $".NET Framework only: the request timed out.");
            }
            catch (OperationCanceledException ex)
            {
                logger.LogError(ex, $".NET Core and .NET 5 and later only: The request failed due to timeout.");
            }

            return null;
        }
    }
}
