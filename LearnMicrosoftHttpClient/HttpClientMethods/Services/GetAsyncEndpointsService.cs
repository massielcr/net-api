using HttpClientMethods.Dtos;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace HttpClientMethods.Services
{
    public class GetAsyncEndpointsService(IHttpClientFactory clientFactory, ILogger<GetAsyncEndpointsService> logger) : IGetAsyncEndpointsService
    {
        #region String

        //GetAsync(String)
        public async Task<int?> GetRepositoriesCountAsync(string orgName)
        {
            int? result = null;

            string relativeUri = $"orgs/{orgName}";

            try
            {
                HttpClient client = clientFactory.CreateClient("GitHub");

                using HttpResponseMessage response = await client.GetAsync(relativeUri).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                using Stream responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

                JsonElement? responseJson = await JsonSerializer.DeserializeAsync<JsonElement>(responseStream).ConfigureAwait(false);

                if (responseJson.HasValue && responseJson.Value.TryGetProperty("public_repos", out var count))
                {
                    result = count.GetInt32();
                    return result;
                }
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError($"The requestUri is not an absolute URI and BaseAddress isn't set.{ex.Message}");
            }
            catch (UriFormatException ex)
            {
                logger.LogError($"The provided request URI is not valid relative or absolute URI: {ex.Message}");
            }
            catch (HttpRequestException ex)
            {
                logger.LogError($"The request failed due to an issue getting a valid HTTP response, such as network connectivity failure, DNS failure, server certificate validation error, or invalid server response: {ex.Message}");
                logger.LogError($".NET Framework only: the request timed out. {ex.Message}");
            }
            catch (OperationCanceledException ex)
            {
                logger.LogError($".NET Core and .NET 5 and later only: The request failed due to timeout. {ex.Message}");                
            }

            return result;
        }


        //GetAsync(String, CancellationToken)
        public async Task<IEnumerable<string>> GetRepositoriesAsync(string orgName, int page, int perPage, int totalPages, CancellationToken cancellationToken)
        {
            List<string> result = [];

            string? relativeUri = $"orgs/{orgName}/repos?page={page}&per_page={perPage}";

            try
            {
                int counterPages = 0;
                bool shouldContinue = true;

                while (shouldContinue)
                {
                    HttpClient client = clientFactory.CreateClient("GitHub");

                    using HttpResponseMessage response = await client.GetAsync(relativeUri, cancellationToken).ConfigureAwait(false);

                    response.EnsureSuccessStatusCode();

                    using Stream responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

                    IEnumerable<JsonElement>? repositories = await JsonSerializer.DeserializeAsync<IEnumerable<JsonElement>>(responseStream, cancellationToken: cancellationToken).ConfigureAwait(false);

                    if (repositories != null && repositories.Any())
                    {
                        result.AddRange(repositories.Select(repo => repo.GetProperty("name").GetString() ?? string.Empty).ToList());
                    }

                    counterPages++;

                    relativeUri = GetNextPageUrl(response.Headers);

                    shouldContinue = counterPages < totalPages && !string.IsNullOrEmpty(relativeUri);
                }
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, "The requestUri is not an absolute URI and BaseAddress isn't set.");
            }
            catch (UriFormatException ex)
            {
                logger.LogError(ex, "The provided request URI is not valid relative or absolute URI.");
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "The request failed due to an issue getting a valid HTTP response, such as network connectivity failure, DNS failure, server certificate validation error, or invalid server response.");
                logger.LogError(ex, ".NET Framework only: the request timed out.");
            }
            catch (OperationCanceledException ex)
            {
                logger.LogError(ex, ".NET Core and .NET 5 and later only: The request failed due to timeout.");
                
            }

            return result.Any() ? result.OrderBy(name => name).Select((name, index) => $"{index + 1} -  {name}").ToList() : [];
        }


        //GetAsync(String, HttpCompletionOption)
        public async Task<string?> GetRepositoryReadmeAsync(string owner, string repo, string contentType)
        {
            try
            {
                string relativeUri = $"repos/{owner}/{repo}/readme";

                HttpClient httpClient = clientFactory.CreateClient("GitHub");

                HttpResponseMessage response = await httpClient.GetAsync(relativeUri).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode) { return null; }

                using Stream responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

                using JsonDocument? readmeJson = await JsonSerializer.DeserializeAsync<JsonDocument>(responseStream).ConfigureAwait(false);

                if (readmeJson == null || !readmeJson.RootElement.TryGetProperty("html_url", out var htmlUrlProp)) { return null; }

                string? htmlUrl = htmlUrlProp.GetString();

                HttpClient htmlUrlClient = clientFactory.CreateClient();

                HttpResponseMessage htmlUrlResponse = await htmlUrlClient.GetAsync(htmlUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

                if (htmlUrlResponse.Content.Headers.ContentType?.MediaType != contentType) { return null; }

                string fullPageHtml = await htmlUrlResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

                return fullPageHtml;
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, "The requestUri is not an absolute URI and BaseAddress isn't set.");
            }
            catch (UriFormatException ex)
            {
                logger.LogError(ex, "The provided request URI is not valid relative or absolute URI.");
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "The request failed due to an issue getting a valid HTTP response, such as network connectivity failure, DNS failure, server certificate validation error, or invalid server response.");
                logger.LogError(ex, ".NET Framework only: the request timed out.");
            }
            catch (OperationCanceledException ex)
            {
                logger.LogError(ex, ".NET Core and .NET 5 and later only: The request failed due to timeout.");

            }

            return null;
        }


        //GetAsync(String, HttpCompletionOption, CancellationToken)
        public async IAsyncEnumerable<GitHubRepositoryDto> GetRepositoriesStreamAsync(string orgName, int perPage, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            string? relativeUri = $"orgs/{orgName}/repos?per_page={perPage}";

            HttpClient httpClient = clientFactory.CreateClient("GitHub");

            JsonSerializerOptions jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

            bool shouldContinue = true;

            while (shouldContinue)
            {
                cancellationToken.ThrowIfCancellationRequested();

                HttpResponseMessage? response = null;

                try
                {
                    response = await httpClient.GetAsync(relativeUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        logger.LogError("Failed to fetch repositories for organization {OrgName}. Status code: {StatusCode}", orgName, response.StatusCode);
                        response.Dispose();
                        yield break;
                    }
                }
                catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested)
                {
                    logger.LogInformation(ex, "Repository stream cancelled for organization {OrgName}", orgName);
                    throw;
                }
                catch (InvalidOperationException ex)
                {
                    logger.LogError(ex, "The requestUri is not an absolute URI and BaseAddress isn't set while fetching repos for {OrgName}", orgName);
                    yield break;
                }
                catch (UriFormatException ex)
                {
                    logger.LogError(ex, "The provided request URI is not valid relative or absolute URI while fetching repos for {OrgName}", orgName);
                    yield break;
                }
                catch (HttpRequestException ex)
                {
                    logger.LogError(ex, "The request failed fetching repos for {OrgName}: {Message}", orgName, ex.Message);
                    yield break;
                }

                using (response)
                {
                    using (Stream responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
                    {
                        IAsyncEnumerable<GitHubRepositoryDto?> repositories = JsonSerializer.DeserializeAsyncEnumerable<GitHubRepositoryDto>(responseStream, jsonSerializerOptions, cancellationToken);

                        await foreach (GitHubRepositoryDto? repo in repositories.WithCancellation(cancellationToken))
                        {
                            if (repo != null)
                            {
                                yield return repo;
                            }
                        }
                    }

                    relativeUri = GetNextPageUrl(response.Headers);
                    shouldContinue = !string.IsNullOrWhiteSpace(relativeUri);

                }
            }

            yield break;
        }

        #endregion



        #region Uri

        //GetAsync(Uri)
        public async Task<int?> GetCommitsCountAsync(string owner, string repo)
        {
            try
            {
                Uri uri = new($"repos/{owner}/{repo}/stats/participation", UriKind.Relative);

                HttpClient client = clientFactory.CreateClient("GitHub");

                using HttpResponseMessage response = await client.GetAsync(uri);

                response.EnsureSuccessStatusCode();

                Stream content = await response.Content.ReadAsStreamAsync();

                if (content != null)
                {
                    JsonElement repository = await JsonSerializer.DeserializeAsync<JsonElement>(content);

                    if (repository.TryGetProperty("all", out var allCommits))
                    {
                        return allCommits.EnumerateArray().Sum(c => c.GetInt32());
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, "The requestUri is not an absolute URI and BaseAddress isn't set.");
            }
            catch (UriFormatException ex)
            {
                logger.LogError(ex,"The provided request URI is not valid relative or absolute URI.");
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "The request failed due to an issue getting a valid HTTP response, such as network connectivity failure, DNS failure, server certificate validation error, or invalid server response.");
                logger.LogError(ex, ".NET Framework only: the request timed out.");
            }
            catch(OperationCanceledException ex)
            {
                logger.LogError(ex, ".NET Core and .NET 5 and later only: The request failed due to timeout.");       
            }

            return null;
        }

        
        //GetAsync(Uri, CancellationToken)
        public async Task<IEnumerable<GitHubCommitDto>> GetCommitsAsync(string orgName, string repositoryName, int page, int perPage, int totalPages, CancellationToken cancellationToken)
        {
            List<GitHubCommitDto> result = [];

            try
            {
                HttpClient client = clientFactory.CreateClient("GitHub");

                while (page <= totalPages)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    Uri uri = new($"repos/{orgName}/{repositoryName}/commits?page={page}&per_page={perPage}", UriKind.Relative);

                    using HttpResponseMessage response = await client.GetAsync(uri, cancellationToken);

                    response.EnsureSuccessStatusCode();

                    using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);

                    IEnumerable<JsonElement>? commits = await JsonSerializer.DeserializeAsync<IEnumerable<JsonElement>>(stream, cancellationToken: cancellationToken);

                    if (commits == null || !commits.Any()) { break; }

                    foreach (JsonElement commit in commits)
                    {
                        if (commit.TryGetProperty("commit", out var commitDetail))
                        {
                            string message = commitDetail.GetProperty("message").GetString() ?? "";
                            DateTime date = commitDetail.GetProperty("committer").GetProperty("date").GetDateTime();

                            result.Add(new GitHubCommitDto(message, date));
                        }
                    }

                    page++;
                }
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, "The requestUri is not an absolute URI and BaseAddress isn't set.");
            }
            catch (UriFormatException ex)
            {
                logger.LogError(ex, "The provided request URI is not valid relative or absolute URI.");
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "The request failed due to an issue getting a valid HTTP response, such as network connectivity failure, DNS failure, server certificate validation error, or invalid server response. ");
                logger.LogError(ex, ".NET Framework only: the request timed out.");                
            }
            catch (OperationCanceledException ex)
            {
                logger.LogError(ex, "The cancellation token was canceled. This exception is stored into the returned task.");
                logger.LogError(ex, ".NET Core and .NET 5 and later only: The request failed due to timeout.");
                throw;
            }

            return result;
        }

        
        //GetAsync(Uri, HttpCompletionOption)
        public async Task<byte[]?> GetRepositoryArchiveAsync(string owner, string repo)
        {
            Uri uri = new($"repos/{owner}/{repo}/zipball/main", UriKind.Relative);

            try
            {
                HttpClient httpClient = clientFactory.CreateClient("GitHub");

                HttpResponseMessage response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                if (response.Content.Headers.ContentLength >= 1024) // 1 KB
                {
                    logger.LogWarning("Repository archive for {Owner}/{Repo} is too large.", owner, repo);
                    return null;
                }

                byte[] responseByteArray = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

                return responseByteArray;
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, "The requestUri is not an absolute URI and BaseAddress isn't set.");
            }
            catch (UriFormatException ex)
            {
                logger.LogError(ex, "The provided request URI is not valid relative or absolute URI.");
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "The request failed due to an issue getting a valid HTTP response, such as network connectivity failure, DNS failure, server certificate validation error, or invalid server response. ");
                logger.LogError(ex, ".NET Framework only: the request timed out.");
            }
            catch (OperationCanceledException ex)
            {
                logger.LogError(ex, "The cancellation token was canceled. This exception is stored into the returned task.");
                logger.LogError(ex, ".NET Core and .NET 5 and later only: The request failed due to timeout.");
                throw;
            }

            return null;
        }


        //GetAsync(Uri, HttpCompletionOption, CancellationToken)
        public async IAsyncEnumerable<GitHubCommitDto> GetCommitsStreamAsync(string orgName, string repositoryName, int page, int perPage, int totalPages, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            HttpClient client = clientFactory.CreateClient("GitHub");

            while (page <= totalPages)
            {
                cancellationToken.ThrowIfCancellationRequested();

                HttpResponseMessage? response = null;

                bool shouldContinue = true;

                try
                {
                    Uri uri = new($"repos/{orgName}/{repositoryName}/commits?page={page}&per_page={perPage}", UriKind.Relative);

                    response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        shouldContinue = false;
                    }
                }
                catch (InvalidOperationException ex)
                {
                    logger.LogError(ex, "The requestUri is not an absolute URI and BaseAddress isn't set.");
                    shouldContinue = false;
                }
                catch (UriFormatException ex)
                {
                    logger.LogError(ex, "The provided request URI is not valid relative or absolute URI.");
                    shouldContinue = false;
                }
                catch (HttpRequestException ex)
                {
                    logger.LogError(ex, "The request failed due to an issue getting a valid HTTP response, such as network connectivity failure, DNS failure, server certificate validation error, or invalid server response.");
                    logger.LogError(ex, ".NET Framework only: the request timed out.");
                    shouldContinue = false;
                }
                catch (OperationCanceledException ex)
                {
                    logger.LogError(ex, "The cancellation token was canceled. This exception is stored into the returned task.");
                    logger.LogError(ex, ".NET Core and .NET 5 and later only: The request failed due to timeout.");
                    throw;
                }

                if (!shouldContinue || response == null) yield break;

                bool hasCommits = false;

                using (response)
                {
                    using Stream commitsStream = await response.Content.ReadAsStreamAsync(cancellationToken);

                    IAsyncEnumerable<JsonElement> commits = JsonSerializer.DeserializeAsyncEnumerable<JsonElement>(commitsStream, cancellationToken: cancellationToken);

                    await foreach (var commit in commits)
                    {
                        hasCommits = true;

                        string message = commit.GetProperty("commit").GetProperty("message").GetString() ?? string.Empty;
                        DateTime commitDate = commit.GetProperty("commit").GetProperty("committer").GetProperty("date").GetDateTime();

                        yield return new GitHubCommitDto(message, commitDate);
                    }                    
                }

                if (!hasCommits) break;

                page++;
            }            
        }

        #endregion


        private static string? GetNextPageUrl(HttpResponseHeaders headers)
        {
            if (!headers.TryGetValues("Link", out var values)) return null;

            // The header looks like: <url1>; rel="next", <url2>; rel="last"
            var linkHeader = values.First();
            var links = linkHeader.Split(',');
            var nextLink = links.FirstOrDefault(l => l.Contains("rel=\"next\""));

            if (nextLink != null)
            {
                // Extract URL between < and >
                int start = nextLink.IndexOf("<") + 1;
                int end = nextLink.IndexOf(">");

                string nextUrl = nextLink.Substring(start, end - start);

                nextUrl = nextUrl.Replace("https://api.github.com", "", StringComparison.OrdinalIgnoreCase);

                return nextUrl;
            }

            return null;
        }
    }
}
