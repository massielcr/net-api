using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace HttpClientMethods.Services
{
    public class GetAsyncEndpointsService(IHttpClientFactory clientFactory, ILogger<GetAsyncEndpointsService> logger) : IGetAsyncEndpointsService
    {
        private const string BaseUrl = "https://api.github.com/";
        private readonly string? _githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");

        private static readonly Regex _cleanMessageRegex = new Regex(@"[^a-zA-Z0-9\s]", RegexOptions.Compiled);

        #region relativeUri

        //GetAsync(String)
        public async Task<int?> GetRepositoriesCountAsync(string orgName)
        {
            int? result = null;

            HttpClient client = clientFactory.CreateClient();

            client.BaseAddress = new Uri(BaseUrl);

            client.DefaultRequestHeaders.Clear();
           
            client.DefaultRequestHeaders.Add("User-Agent", "MyTestService");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _githubToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

            string relativeUri = $"orgs/{orgName}";

            try
            {
                using HttpResponseMessage response = await client.GetAsync(relativeUri).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                using Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

                JsonElement? organization = await JsonSerializer.DeserializeAsync<JsonElement>(stream).ConfigureAwait(false);

                if (organization.HasValue && organization.Value.TryGetProperty("public_repos", out var count))
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
            HttpClient client = clientFactory.CreateClient();

            client.BaseAddress = new Uri(BaseUrl);

            client.DefaultRequestHeaders.Clear();

            client.DefaultRequestHeaders.Add("User-Agent", "MyTestService");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _githubToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

            List<string> result = [];

            try
            {
                int counter = 0;
                bool shouldContinue = true;

                while (shouldContinue)
                {
                    string? relativeUri = $"orgs/{orgName}/repos";

                    using HttpResponseMessage response = await client.GetAsync(relativeUri, cancellationToken).ConfigureAwait(false);

                    response.EnsureSuccessStatusCode();

                    using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

                    IEnumerable<JsonElement>? repositories = await JsonSerializer.DeserializeAsync<IEnumerable<JsonElement>>(stream, cancellationToken: cancellationToken).ConfigureAwait(false);

                    if (repositories != null && repositories.Any())
                    {
                        result.AddRange(repositories.Select(repo => repo.GetProperty("name").GetString() ?? string.Empty).ToList());
                    }

                    counter++;

                    relativeUri = GetNextPageUrl(response.Headers);

                    shouldContinue = counter < totalPages && !string.IsNullOrEmpty(relativeUri);
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
        public async IAsyncEnumerable<string> GetRepositoriesStreamAsync(string orgName, int page, int perPage, int totalPages)
        {
            yield break;
        }


        //GetAsync(String, HttpCompletionOption, CancellationToken)
        public async IAsyncEnumerable<string> GetRepositoriesStreamAsync(string orgName, int page, int perPage, int totalPages, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield break;
        }


        #endregion



        #region Uri

        //GetAsync(Uri)
        public async Task<int?> GetCommitsCountAsync(string owner, string repo)
        {
            HttpClient client = clientFactory.CreateClient();

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "MyTestService");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_githubToken}");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

            try
            {
                Uri uri = new($"{BaseUrl}repos/{owner}/{repo}/stats/participation");

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
        public async Task<IEnumerable<(string commitMessage, DateTime commitDate)>> GetCommitsAsync(string orgName, string repositoryName, int page, int perPage, int totalPages, CancellationToken cancellationToken)
        {
            HttpClient client = clientFactory.CreateClient();

            client.DefaultRequestHeaders.Clear();

            client.DefaultRequestHeaders.UserAgent.ParseAdd("MyTestService");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _githubToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

            List<(string commitMessage, DateTime commitDate)> result = [];

            try
            {
                while (page <= totalPages)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    Uri uri = new($"{BaseUrl}repos/{orgName}/{repositoryName}/commits?page={page}&per_page={perPage}");

                    using HttpResponseMessage response = await client.GetAsync(uri, cancellationToken);

                    response.EnsureSuccessStatusCode();

                    using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);

                    IEnumerable<JsonElement>? commits = await JsonSerializer.DeserializeAsync<IEnumerable<JsonElement>>(stream, cancellationToken: cancellationToken);

                    if (commits == null || !commits.Any())
                    {
                        break;
                    }

                    foreach (JsonElement commit in commits)
                    {
                        if (commit.TryGetProperty("commit", out var commitDetail))
                        {
                            string rawMsg = commitDetail.GetProperty("message").GetString() ?? "";
                            DateTime date = commitDetail.GetProperty("committer").GetProperty("date").GetDateTime();

                            var cleanMessage = _cleanMessageRegex.Replace(WebUtility.HtmlDecode(rawMsg).Replace("\n", " "), "").Trim();
                            result.Add((cleanMessage, date));
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
        public async IAsyncEnumerable<(string commitMessage, DateTime commitDate)> GetCommitsStreamAsync(string orgName, string repositoryName, int page, int perPage, int totalPages)
        {
            yield break;
        }


        //GetAsync(Uri, HttpCompletionOption, CancellationToken)
        public async IAsyncEnumerable<(string commitMessage, DateTime commitDate)> GetCommitsStreamAsync(string orgName, string repositoryName, int page, int perPage, int totalPages, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            HttpClient client = clientFactory.CreateClient();

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "MyTestService");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _githubToken);


            while (page <= totalPages)
            {
                cancellationToken.ThrowIfCancellationRequested();

                HttpResponseMessage? response = null;

                bool shouldContinue = true;

                try
                {
                    Uri uri = new($"{BaseUrl}repos/{orgName}/{repositoryName}/commits?page={page}&per_page={perPage}");

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

                        string rawCommitMessage = commit.GetProperty("commit").GetProperty("message").GetString() ?? string.Empty;
                        DateTime commitDate = commit.GetProperty("commit").GetProperty("committer").GetProperty("date").GetDateTime();

                        var decoded = WebUtility.HtmlDecode(rawCommitMessage);
                        var cleanMessage = _cleanMessageRegex.Replace(decoded.Replace("\n", " "), "").Trim();

                        yield return (cleanMessage, commitDate);                        
                    }                    
                }

                if (!hasCommits) break;

                page++;
            }            
        }

        #endregion


        private string? GetNextPageUrl(HttpResponseHeaders headers)
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
                return nextLink.Substring(start, end - start);
            }

            return null;
        }
    }
}
