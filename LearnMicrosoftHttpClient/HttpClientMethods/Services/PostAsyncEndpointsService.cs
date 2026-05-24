using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

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

        public async Task<bool> CreatePersonalRepositoryIssueAsync(string owner, string repo, string title, byte[] imageBody)
        {
            if (imageBody == null || imageBody.Length == 0)
            {
                throw new ArgumentException("Image body cannot be null or empty.", nameof(imageBody));
            }

            HttpClient httpClient = clientFactory.CreateClient("GitHub");

            try
            {
                // Create the asset
                string base64Image = Convert.ToBase64String(imageBody);
                string assetName = $"issue_{Guid.NewGuid():N}.png";

                Uri assetUri = new($"repos/{owner}/{repo}/contents/assets/{assetName}", UriKind.Relative);

                var assetContent = new
                {
                    message = $"Add image for issue '{title}'",
                    content = base64Image
                };

                using HttpResponseMessage response = await httpClient.PutAsJsonAsync(assetUri, assetContent);

                if (!response.IsSuccessStatusCode)
                {
                    string errorBody = await response.Content.ReadAsStringAsync(CancellationToken.None);
                    logger.LogWarning("GitHub API returned status {StatusCode} for asset '{AssetName}'. Details: {Details}", response.StatusCode, assetName, errorBody);
                    return false;
                }

                // Create the Issue with the asset link
                Uri relativeUri = new($"repos/{owner}/{repo}/issues", UriKind.Relative);

                var issue = new
                {
                    title,
                    body = $"![{assetName}](https://raw.githubusercontent.com/{owner}/{repo}/main/assets/{assetName})"
                };

                string issueJson = JsonSerializer.Serialize(issue, JsonSerializerOptions.Web);

                using StringContent issueStringContent = new(issueJson, System.Text.Encoding.UTF8, "application/json");

                using HttpResponseMessage issueResponse = await httpClient.PostAsync(relativeUri, issueStringContent, CancellationToken.None);

                if (!issueResponse.IsSuccessStatusCode)
                {
                    string errorBody = await issueResponse.Content.ReadAsStringAsync(CancellationToken.None);
                    logger.LogWarning("GitHub API returned status {StatusCode} for issue '{IssueName}'. Details: {Details}", issueResponse.StatusCode, title, errorBody);
                    return false;
                }

                return true;
            }
            catch (HttpRequestException ex)
            {
                // Catches network dropout, DNS failures, or timeout exceptions
                logger.LogError(ex, "Network error occurred while communicating with the GitHub API for issue '{IssueName}'.", title);                
            }
            catch (JsonException ex)
            {
                // Catches any unexpected JSON parsing/serialization problems
                logger.LogError(ex, "JSON formatting or parsing failed while working with GitHub payload for issue '{IssueName}'.", title);
            }
            catch (Exception ex)
            {
                // Catch-all structural backup safety block
                logger.LogError(ex, "An unexpected error occurred while executing CreatePersonalRepositoryIssueAsync for issue '{IssueName}'.", title);
            }

            return false;

        }
    }
}
