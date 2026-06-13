using HttpClientMethods.Models;
using HttpClientMethods.Services.Interfaces;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace HttpClientMethods.Services
{
    public class PatchAsyncEndpoinsService(IHttpClientFactory httpClientFactory, ILogger<PatchAsyncEndpoinsService> logger) : IPatchAsyncEndpoinsService
    {
        //PatchAsync(String, HttpContent)
        public async Task<bool> UpdateRepoVisibilityAsync(string owner, string repo, string visibility)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("GitHub");

            JsonSerializerOptions jsonSerializerOptions = new(JsonSerializerDefaults.Web);

            string uri = $"repos/{owner}/{repo}";

            var githubRepo = new { visibility };            

            try
            {
                JsonContent jsonContent = JsonContent.Create(githubRepo, options: jsonSerializerOptions);

                using HttpResponseMessage response = await httpClient.PatchAsync(uri, jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError("Failed to update repository visibility. Status Code: {StatusCode}", response.StatusCode);
                    return false;
                }

                return true;

            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, $"The requestUri is not an absolute URI and BaseAddress isn't set.");
            }
            catch (UriFormatException ex)
            {
                logger.LogError(ex, $"The provided request URI is not valid relative or absolute URI.");
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, $"The request failed due to an issue getting a valid HTTP response, such as network connectivity failure, DNS failure, server certificate validation error, or invalid server response");
                logger.LogError(ex, $".NET Framework only: the request timed out.");
            }
            catch (OperationCanceledException ex)
            {
                logger.LogError(ex, $".NET Core and .NET 5 and later only: The request failed due to timeout.");
            }

            return false;
        }

        //PatchAsync(String, HttpContent, CancellationToken)
        public async Task<bool> UpdateRepoIssuesAsync(string owner, string repo, IEnumerable<GitHubIssue> issues, CancellationToken cancellationToken)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("GitHub");

            JsonSerializerOptions jsonSerializerOptions = new(JsonSerializerDefaults.Web);

            foreach (GitHubIssue issue in issues)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string uri = $"repos/{owner}/{repo}/issues/{issue.IssueNumber}";

                var githubIssue = new
                {
                    state = issue.State,
                    state_reason = issue.StateReason,
                    labels = issue.Labels
                };

                JsonContent jsonContent = JsonContent.Create(githubIssue, options: jsonSerializerOptions);

                try
                {
                    using HttpResponseMessage response = await httpClient.PatchAsync(uri, jsonContent, cancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        logger.LogError("Failed to update issue #{IssueNumber}. Status Code: {StatusCode}", issue.IssueNumber, response.StatusCode);
                        return false;
                    }                    
                }
                catch (InvalidOperationException ex)
                {
                    logger.LogError(ex, $"The requestUri is not an absolute URI and BaseAddress isn't set.");
                    return false;
                }
                catch (UriFormatException ex)
                {
                    logger.LogError(ex, $"The provided request URI is not valid relative or absolute URI.");
                    return false;
                }
                catch (HttpRequestException ex)
                {
                    logger.LogError(ex, $"The request failed due to an issue getting a valid HTTP response, such as network connectivity failure, DNS failure, server certificate validation error, or invalid server response");
                    logger.LogError(ex, $".NET Framework only: the request timed out.");
                    return false;
                }
                catch (OperationCanceledException ex)
                {
                    logger.LogError(ex, $".NET Core and .NET 5 and later only: The request failed due to timeout.");
                    throw;
                }
            }

            return true;
        }


        //PatchAsync(Uri, HttpContent)
        public async Task<bool> UpdateRepoIssueAsync(string owner, string repo, GitHubIssue issue)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("GitHub");

            JsonSerializerOptions jsonSerializerOptions = new(JsonSerializerDefaults.Web);

            Uri uri = new($"repos/{owner}/{repo}/issues/{issue.IssueNumber}", UriKind.Relative);

            var githubIssue = new
            {
                state = issue.State,
                state_reason = issue.StateReason,
                labels = issue.Labels
            };

            JsonContent jsonContent = JsonContent.Create(githubIssue, options: jsonSerializerOptions);

            try
            {
                using HttpResponseMessage response = await httpClient.PatchAsync(uri, jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError("Failed to update issue #{IssueNumber}. Status Code: {StatusCode}", issue.IssueNumber, response.StatusCode);
                    return false;
                }

                return true;
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, $"The requestUri is not an absolute URI and BaseAddress isn't set.");
            }
            catch (UriFormatException ex)
            {
                logger.LogError(ex, $"The provided request URI is not valid relative or absolute URI.");
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, $"The request failed due to an issue getting a valid HTTP response, such as network connectivity failure, DNS failure, server certificate validation error, or invalid server response");
                logger.LogError(ex, $".NET Framework only: the request timed out.");
            }
            catch (OperationCanceledException ex)
            {
                logger.LogError(ex, ".NET Core and .NET 5 and later only: The request failed due to timeout.");
            }

            return false;
        }

        //PatchAsync(Uri, HttpContent, CancellationToken)
        public async IAsyncEnumerable<int> UpdateRepoIssuesStreamAsync(string owner, string repo, IEnumerable<GitHubIssue> issues, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("GitHub");

            JsonSerializerOptions jsonSerializerOptions = new(JsonSerializerDefaults.Web);

            await foreach(GitHubIssue issue in issues.ToAsyncEnumerable().WithCancellation(cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                bool isSuccess = false;

                Uri uri = new($"repos/{owner}/{repo}/issues/{issue.IssueNumber}", UriKind.Relative);

                var githubIssue = new
                {
                    state = issue.State,
                    state_reason = issue.StateReason,
                    labels = issue.Labels
                };

                JsonContent jsonContent = JsonContent.Create(githubIssue, options: jsonSerializerOptions);

                try
                {
                    using HttpResponseMessage response = await httpClient.PatchAsync(uri, jsonContent, cancellationToken);

                    isSuccess = response.IsSuccessStatusCode;
                    
                    if (!isSuccess)
                    {
                        logger.LogError("Failed to update issue #{IssueNumber}. Status Code: {StatusCode}", issue.IssueNumber, response.StatusCode);
                        yield break;
                    }                    
                }
                catch (InvalidOperationException ex)
                {
                    logger.LogError(ex, $"The requestUri is not an absolute URI and BaseAddress isn't set.");
                }
                catch (UriFormatException ex)
                {
                    logger.LogError(ex, $"The provided request URI is not valid relative or absolute URI.");
                }
                catch (HttpRequestException ex)
                {
                    logger.LogError(ex, $"The request failed due to an issue getting a valid HTTP response, such as network connectivity failure, DNS failure, server certificate validation error, or invalid server response");
                    logger.LogError(ex, $".NET Framework only: the request timed out.");
                }
                catch (OperationCanceledException ex)
                {
                    logger.LogError(ex, $".NET Core and .NET 5 and later only: The request failed due to timeout.");
                    throw;
                }

                if (isSuccess)
                {
                    yield return issue.IssueNumber;    
                }
            }

            yield break;
        }        
    }
}
