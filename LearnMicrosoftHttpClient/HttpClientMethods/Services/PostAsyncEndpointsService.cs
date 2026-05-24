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

        public async Task<bool> CreateRepositoryIssuesAsync(string owner, string repo, string title, string body, int count, CancellationToken cancellationToken)
        {
            HttpClient httpClient = clientFactory.CreateClient("GitHub");

            string relativeUri = $"repos/{owner}/{repo}/issues";

            int counter = 1;

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

        public async Task<bool> CreateRepositoryIssueAsync(string owner, string repo, string title, byte[] imageBody)
        {
            string? assetName = await CreateRepositoryContentAssetAsync(owner, repo, title, imageBody);

            if (string.IsNullOrWhiteSpace(assetName))
            {
                logger.LogWarning("Failed to create content asset for issue '{IssueName}'.", title);
                return false;
            }

            HttpClient httpClient = clientFactory.CreateClient("GitHub");

            try
            {
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
                logger.LogError(ex, "Network error occurred while communicating with the GitHub API for issue '{IssueName}'.", title);                
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "JSON formatting or parsing failed while working with GitHub payload for issue '{IssueName}'.", title);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while executing CreatePersonalRepositoryIssueAsync for issue '{IssueName}'.", title);
            }

            return false;

        }

        public async Task<bool> CreateRepositoryIssuesAsync(string owner, string repo, string title, byte[] imageBody, int count, CancellationToken cancellationToken)
        {
            string? assetName = await CreateRepositoryContentAssetAsync(owner, repo, title, imageBody, cancellationToken);

            if (string.IsNullOrWhiteSpace(assetName))
            {
                logger.LogWarning("Failed to create content asset for issue '{IssueName}'.", title);
                return false;
            }

            HttpClient httpClient = clientFactory.CreateClient("GitHub");

            int counter = 1;

            try
            {
                Uri relativeUri = new($"repos/{owner}/{repo}/issues", UriKind.Relative);

                while(counter <= count)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var issue = new
                    {
                        title = $"{title} {counter}",
                        body = $"![{assetName}](https://raw.githubusercontent.com/{owner}/{repo}/main/assets/{assetName})"
                    };

                    string issueJson = JsonSerializer.Serialize(issue, JsonSerializerOptions.Web);

                    using StringContent issueStringContent = new(issueJson, System.Text.Encoding.UTF8, "application/json");

                    using HttpResponseMessage issueResponse = await httpClient.PostAsync(relativeUri, issueStringContent, cancellationToken);

                    if (!issueResponse.IsSuccessStatusCode)
                    {
                        string errorBody = await issueResponse.Content.ReadAsStringAsync(cancellationToken);
                        logger.LogWarning("GitHub API returned status {StatusCode} for issue '{IssueName}'. Details: {Details}", issueResponse.StatusCode, title, errorBody);
                        return false;
                    }

                    counter++;
                }

                return true;
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Issue creation was canceled by the user after creating {CreatedCount} issues.", counter);
                throw;
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Network error occurred while communicating with the GitHub API for issue '{IssueName}'.", title);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "JSON formatting or parsing failed while working with GitHub payload for issue '{IssueName}'.", title);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while executing CreatePersonalRepositoryIssueAsync for issue '{IssueName}'.", title);
            }

            return false;
        }

        public async Task<string?> CreateRepositoryContentAssetAsync(string owner, string repo, string title, byte[] imageBody, CancellationToken cancellationToken = default)
        {
            if (imageBody == null || imageBody.Length == 0)
            {
                throw new ArgumentException("Image body cannot be null or empty.", nameof(imageBody));
            }

            HttpClient httpClient = clientFactory.CreateClient("GitHub");

            try
            {
                string base64Image = Convert.ToBase64String(imageBody);
                string assetName = $"issue_{Guid.NewGuid():N}.png";

                Uri assetUri = new($"repos/{owner}/{repo}/contents/assets/{assetName}", UriKind.Relative);

                var assetContent = new
                {
                    message = $"Add image for issue '{title}'",
                    content = base64Image
                };

                using HttpResponseMessage response = await httpClient.PutAsJsonAsync(assetUri, assetContent, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    string errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    logger.LogWarning("GitHub API returned status {StatusCode} for asset '{AssetName}'. Details: {Details}", response.StatusCode, assetName, errorBody);
                    return null;
                }

                return assetName;
            }
            catch (OperationCanceledException ex)
            {
                logger.LogInformation(ex, "Issue creation was canceled by the user.");
                throw;
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Network error occurred while communicating with the GitHub API for asset '{AssetName}'.", title);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "JSON formatting or parsing failed while working with GitHub payload for asset '{AssetName}'.", title);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while executing CreatePersonalRepositoryIssueAsync for asset '{AssetName}'.", title);
            }

            return null;
        }
    }
}