using HttpClientMethods.Models;
using System.Text.Json;

namespace HttpClientMethods.Services
{
    public class GetByteArrayAsyncEndpointsService(IHttpClientFactory httpClientFactory, ILogger<GetByteArrayAsyncEndpointsService> logger) : IGetByteArrayAsyncEndpointsService
    {
        private const string BaseUrl = "https://api.github.com";

        public async Task<GitHubAvatar?> DownloadAvatarStringAsync(string username)
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
                    ContentType = GetMimeTypeFromBytes(data),
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

        public async Task<GitHubAvatar?> DownloadAvatarUriAsync(string username)
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
                    ContentType = GetMimeTypeFromBytes(data),
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

        private string GetMimeTypeFromBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 4) return "application/octet-stream";

            // JPEG: Starts with FF D8 FF
            if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
            {
                return "image/jpeg";
            }

            // PNG: Starts with 89 50 4E 47
            if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
            {
                return "image/png";
            }

            // GIF: Starts with 47 49 46 38 ("GIF8")
            if (bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x38)
            {
                return "image/gif";
            }

            // Default fallback if unknown binary pattern
            return "image/jpeg";
        }
    }
}
