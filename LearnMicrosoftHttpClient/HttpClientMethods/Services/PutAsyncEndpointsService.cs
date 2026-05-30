using HttpClientMethods.Models;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace HttpClientMethods.Services
{
    public class PutAsyncEndpointsService(IHttpClientFactory httpClientFactory, ILogger<PutAsyncEndpointsService> logger) : IPutAsyncEndpointsService
    {
        //PutAsync(String, HttpContent)
        public async Task<bool> UpdateRepositoryTopicsAsync(string owner, string repo, List<string> names)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("GitHub");

            string relativeUri = $"repos/{owner}/{repo}/topics";

            JsonContent content = JsonContent.Create(new { names });

            try
            {
                using HttpResponseMessage response = await httpClient.PutAsync(relativeUri, content);

                if (!response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    logger.LogError("Failed to replace repository topics. Status Code: {StatusCode}, Response: {ResponseContent}", response.StatusCode, responseContent);
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

        //PutAsync(String, HttpContent, CancellationToken)
        public async Task<bool> LockRepositoryIssuesAsync(string owner, string repo, List<GitHubIssue> githubIssues, CancellationToken cancellationToken)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("GitHub");

            JsonSerializerOptions options = new(options: JsonSerializerOptions.Web);

            foreach (var item in githubIssues)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string relativeUri = $"repos/{owner}/{repo}/issues/{item.IssueNumber}/lock";                

                var issue = new { lock_reason = item.LockReason};

                StringContent content = new(JsonSerializer.Serialize(issue, options), System.Text.Encoding.UTF8, "application/json");

                try
                {
                    using HttpResponseMessage response = await httpClient.PutAsync(relativeUri, content, cancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                        logger.LogError("Failed to update repository issue. Status Code: {StatusCode}, Response: {ResponseContent}", response.StatusCode, responseContent);
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
                    return false;
                }
            }

            return true;
        }

        //PutAsync(Uri, HttpContent)
        public async Task<bool> LockRepositoryIssueAsync(string owner, string repo, GitHubIssue githubIssue)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("GitHub");

            Uri uri = new($"repos/{owner}/{repo}/issues/{githubIssue.IssueNumber}/lock", UriKind.Relative);

            JsonSerializerOptions options = new(options: JsonSerializerOptions.Web);

            var issue = new { lock_reason = githubIssue.LockReason };

            StringContent content = new(JsonSerializer.Serialize(issue, options), System.Text.Encoding.UTF8, "application/json");

            try
            {
                using HttpResponseMessage response = await httpClient.PutAsync(uri, content);

                if (!response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    logger.LogError("Failed to update repository issue. Status Code: {StatusCode}, Response: {ResponseContent}", response.StatusCode, responseContent);
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

        //PutAsync(Uri, HttpContent, CancellationToken)
        public async IAsyncEnumerable<int> LockRepositoryIssuesStreamAsync(string owner, string repo, List<GitHubIssue> githubIssues, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            yield break;
        }
    }
}
