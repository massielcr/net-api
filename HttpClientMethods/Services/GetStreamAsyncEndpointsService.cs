using HttpClientMethods.Interfaces;
using System.Text.Json;

namespace HttpClientMethods.Services
{
    public class GetStreamAsyncEndpointsService(IHttpClientFactory httpClientFactory, ILogger<GetStreamAsyncEndpointsService> logger) : IGetStreamAsyncEndpointsService
    {
        //GetStreamAsync(String)
        public async Task<Stream?> GetAvatarStringAsync(string username)
        {
            string relativeUri = $"users/{username}";
            string? avatarUrl = null;

            try
            {
                HttpClient httpClient = httpClientFactory.CreateClient("GitHub");

                using (HttpResponseMessage response = await httpClient.GetAsync(relativeUri))
                {
                    if (!response.IsSuccessStatusCode) { return null; }

                    using Stream contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

                    using JsonDocument jsonContent = await JsonDocument.ParseAsync(contentStream).ConfigureAwait(false);

                    if (jsonContent == null || !jsonContent.RootElement.TryGetProperty("avatar_url", out var avatarProp)) { return null; }

                    avatarUrl = avatarProp.GetString();
                }                

                if (string.IsNullOrWhiteSpace(avatarUrl)) { return null; }


                HttpClient imageClient = httpClientFactory.CreateClient();

                Stream avatarStream = await imageClient.GetStreamAsync(avatarUrl);

                return avatarStream;
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, "An error occurred while trying to create an HttpClient instance. Please check the configuration of the named HttpClient 'GitHub' and ensure it is registered properly in the dependency injection container.");
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "An error occurred while sending the HTTP request to GitHub. This could be due to network issues, an invalid URL, or GitHub being unavailable.");
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "An error occurred while parsing the JSON response from GitHub. The response format may have changed or there may be unexpected data.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while trying to download the avatar image.");                
            }

            return null;
        }


        //GetStreamAsync(Uri)
        public async Task<Stream?> GetAvatarUriAsync(string username)
        {
            Uri uri = new($"users/{username}", UriKind.Relative);

            string? avatarUrl = null;

            try
            {
                HttpClient httpClient = httpClientFactory.CreateClient("GitHub");

                using (HttpResponseMessage response = await httpClient.GetAsync(uri))
                {
                    if (!response.IsSuccessStatusCode) { return null; }

                    using Stream responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

                    using JsonDocument jsonContent = await JsonDocument.ParseAsync(responseStream).ConfigureAwait(false);

                    if (jsonContent == null || !jsonContent.RootElement.TryGetProperty("avatar_url", out var avatarProp)) { return null; }

                    avatarUrl = avatarProp.GetString();
                }

                if (string.IsNullOrWhiteSpace(avatarUrl)) { return null; }

                HttpClient imageClient = httpClientFactory.CreateClient();

                Stream avatarStream = await imageClient.GetStreamAsync(avatarUrl);

                return avatarStream;
            }
            catch(InvalidOperationException ex)
            {
                logger.LogError(ex, "An error occurred while trying to create an HttpClient instance. Please check the configuration of the named HttpClient 'GitHub' and ensure it is registered properly in the dependency injection container.");
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "An error occurred while sending the HTTP request to GitHub. This could be due to network issues, an invalid URL, or GitHub being unavailable.");
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "An error occurred while parsing the JSON response from GitHub. The response format may have changed or there may be unexpected data.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while trying to download the avatar image.");
            } 

            return null;
        }
    }
}
