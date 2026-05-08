using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace HttpClientMethods.Services
{
    public class GetEndpointsService(IHttpClientFactory clientFactory) : IGetEndpointsService
    {
        private const string BaseUrl = "https://api.github.com/";
        private readonly string? _githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");

        private static readonly Regex _cleanMessageRegex = new Regex(@"[^a-zA-Z0-9\s]", RegexOptions.Compiled);

        #region relativeUri

        //GetAsync(String)
        public async Task<int> GetRepositoriesCountAsync()
        {
            int result = -1;

            HttpClient client = clientFactory.CreateClient();

            client.BaseAddress = new Uri(BaseUrl);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "MyTestService");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _githubToken);

            string relativeUri = $"orgs/dotnet/repos";

            try
            {
                using HttpResponseMessage response = await client.GetAsync(relativeUri);

                response.EnsureSuccessStatusCode();

                using Stream stream = await response.Content.ReadAsStreamAsync();

                IEnumerable<JsonElement>? repositories = await JsonSerializer.DeserializeAsync<IEnumerable<JsonElement>>(stream);

                return repositories?.Count() ?? result;

            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"The requestUri is not an absolute URI and BaseAddress isn't set.{ex.Message}");
                return result;
            }
            catch (UriFormatException ex)
            {
                Console.WriteLine($"The provided request URI is not valid relative or absolute URI: {ex.Message}");
                return result;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"The request failed due to an issue getting a valid HTTP response, such as network connectivity failure, DNS failure, server certificate validation error, or invalid server response: {ex.Message}");
                Console.WriteLine($".NET Framework only: the request timed out. {ex.Message}");
                return result;
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine($".NET Core and .NET 5 and later only: The request failed due to timeout. {ex.Message}");
                return result;
            }            
        }

        //GetAsync(String, HttpCompletionOption, CancellationToken)
        public async Task<(List<(string commitMessage, DateTime commitDate)> commits, int total)> GetRepositoryCommits(string orgName, string repositoryName, int page, int perPage, int totalPages, CancellationToken cancellationToken)
        {
            HttpClient client = clientFactory.CreateClient();

            client.BaseAddress = new Uri(BaseUrl);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("MyTestService");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _githubToken);

            (List<(string commitMessage, DateTime commitDate)> commits, int total) result = (new List<(string commitMessage, DateTime commitDate)>(), 0);

            try
            {
                while (page <= totalPages)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string relativeUri = $"repos/{orgName}/{repositoryName}/commits?page={page}&per_page={perPage}";

                    using HttpResponseMessage response = await client.GetAsync(relativeUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

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
                            result.commits.Add((cleanMessage, date));
                        }
                    }

                    result.total += commits.Count();

                    page++;
                }

                return result;
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine("The requestUri is not an absolute URI and BaseAddress isn't set.");
                return result;
            }
            catch (UriFormatException ex)
            {
                Console.WriteLine("The provided request URI is not valid relative or absolute URI.");
                return result;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("The request failed due to an issue getting a valid HTTP response, such as network connectivity failure, DNS failure, server certificate validation error, or invalid server response. ");
                Console.WriteLine(".NET Framework only: the request timed out.");
                return result;
            }            
            catch (OperationCanceledException ex)
            {
                Console.WriteLine("The cancellation token was canceled. This exception is stored into the returned task.");
                Console.WriteLine(".NET Core and .NET 5 and later only: The request failed due to timeout.");
                throw;
            }
        }

        #endregion

        #region Uri

        //GetAsync(Uri)
        public async Task<IEnumerable<string>> GetAllRepositoriesAsync()
        {
            HttpClient client = clientFactory.CreateClient();

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "MyTestService");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _githubToken);

            Uri uri = new($"{BaseUrl}orgs/dotnet/repos");

            try
            {
                using HttpResponseMessage response = await client.GetAsync(uri);

                response.EnsureSuccessStatusCode();

                using Stream stream = await response.Content.ReadAsStreamAsync();

                var repositories = await JsonSerializer.DeserializeAsync<IEnumerable<JsonElement>>(stream);

                return repositories?
                        .Select(repo => repo.GetProperty("name").GetString() ?? string.Empty)
                        .OrderBy(name => name)
                        .Select((name, index) => $"{index + 1} -  {name}")
                        .ToList() ?? [];

            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine("The requestUri is not an absolute URI and BaseAddress isn't set.");
                return [];
            }
            catch (UriFormatException ex)
            {
                Console.WriteLine("The provided request URI is not valid relative or absolute URI.");
                return [];
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("The request failed due to an issue getting a valid HTTP response, such as network connectivity failure, DNS failure, server certificate validation error, or invalid server response.");
                Console.WriteLine(".NET Framework only: the request timed out.");
                return [];
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine(".NET Core and .NET 5 and later only: The request failed due to timeout.");
                return [];
            }            
        }


        //GetAsync(Uri, HttpCompletionOption, CancellationToken)
        public async IAsyncEnumerable<(string commitMessage, DateTime commitDate)> GetRepositoryCommitsStreamAsync(string orgName, string repositoryName, int page, int perPage, int totalPages, [EnumeratorCancellation] CancellationToken cancellationToken = default)
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
                    Console.WriteLine("The requestUri is not an absolute URI and BaseAddress isn't set.");
                    shouldContinue = false;
                }
                catch (UriFormatException ex)
                {
                    Console.WriteLine("The provided request URI is not valid relative or absolute URI.");
                    shouldContinue = false;
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine("The request failed due to an issue getting a valid HTTP response, such as network connectivity failure, DNS failure, server certificate validation error, or invalid server response.");
                    Console.WriteLine(".NET Framework only: the request timed out.");
                    shouldContinue = false;
                }
                catch (OperationCanceledException ex)
                {
                    Console.WriteLine("The cancellation token was canceled. This exception is stored into the returned task.");
                    Console.WriteLine(".NET Core and .NET 5 and later only: The request failed due to timeout.");
                    throw;
                }

                if (!shouldContinue || response == null) yield break;

                bool hasCommits = false;

                using (response)
                {
                    IAsyncEnumerable<JsonElement> commitsStream = response.Content.ReadFromJsonAsAsyncEnumerable<JsonElement>(cancellationToken: cancellationToken);

                    await foreach (var commit in commitsStream.WithCancellation(cancellationToken))
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
    }
}
