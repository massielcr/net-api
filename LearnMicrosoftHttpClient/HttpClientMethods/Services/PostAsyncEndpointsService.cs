using System.Text.Json;

namespace HttpClientMethods.Services
{
    public class PostAsyncEndpointsService(IHttpClientFactory clientFactory, ILogger<PostAsyncEndpointsService> logger) : IPostAsyncEndpointsService
    {       
        public async Task<bool> CreatePersonalRepositoryAsync(string name, string description, bool isPrivate, bool initialREADME, bool hasDownloads)
        {
            string uri = $"user/repos";

            try
            {
                HttpClient httpClient = clientFactory.CreateClient("GitHub");

                var repoSetup = new
                {
                    name,
                    description,
                    @private = isPrivate,
                    auto_init = initialREADME,
                    has_downloads = hasDownloads
                };

                using JsonContent content = JsonContent.Create(repoSetup, options: JsonSerializerOptions.Web);

                HttpResponseMessage response = await httpClient.PostAsync(uri, content);

                if (!response.IsSuccessStatusCode)
                {
                    string errorBody = await response.Content.ReadAsStringAsync();
                    logger.LogWarning("GitHub API returned status {StatusCode}. Details: {Details}", response.StatusCode, errorBody);
                    return false;
                }

                return true;
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "An error occurred while sending the HTTP request to GitHub. This could be due to network issues, an invalid URL, or GitHub being unavailable.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while trying to create a repository.");
            }

            return false;            
        }
    }
}
