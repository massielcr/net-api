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

        public async Task<bool> CreatePersonalRepositoryIssuesAsync(string owner, string repo, string title, string body, int count, CancellationToken cancellationToken)
        {
            int counter = 1;

            string relativeUri = $"repos/{owner}/{repo}/issues";

            HttpClient httpClient = clientFactory.CreateClient("GitHub");

            try
            {
                while (counter <= count)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var issue = new
                    {
                        title = $"{title} #{counter}",
                        body = $"{body} - Issue number {counter}"
                    };

                    string issueJson = JsonSerializer.Serialize(issue, JsonSerializerOptions.Web);

                    using StringContent content = new(issueJson, System.Text.Encoding.UTF8, "application/json");

                    using HttpResponseMessage response = await httpClient.PostAsync(relativeUri, content, cancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        string errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                        logger.LogWarning("GitHub API returned status {StatusCode} for issue #{IssueNumber}. Details: {Details}", response.StatusCode, counter, errorBody);
                        return false;
                    }

                    counter++;
                }

                return true;
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Issue creation was canceled by the user after creating {CreatedCount} issues.", counter - 1);
                throw;
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "An error occurred while sending the HTTP request to GitHub. This could be due to network issues, an invalid URL, or GitHub being unavailable.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while trying to create issues.");
            }

            return false;
        }
    }
}
