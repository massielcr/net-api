using System.Net.Http.Headers;
using System.Text.Json;

namespace HttpClientMethods.Services
{
    public class GetEndpointsService(IHttpClientFactory clientFactory) : IGetEndpointsService
    {
        private const string BaseUrl = "https://api.github.com/";
        private readonly string? _githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");

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
            catch (UriFormatException ex)
            {
                Console.WriteLine($"The provided request URI is not valid relative or absolute URI: {ex.Message}");
                return result;
            }
        }

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
            catch (UriFormatException ex)
            {
                Console.WriteLine("The provided request URI is not valid relative or absolute URI.");
                return [];
            }
        }

        public async Task<(List<(string commitMessage, DateTime commitDate)> commits, int total)> GetRepositoryCommits(string orgName, string repositoryName, int page, int perPage, int totalPages, CancellationToken cancellationToken)
        {
            HttpClient client = clientFactory.CreateClient();

            client.BaseAddress = new Uri(BaseUrl);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "MyTestService");    
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _githubToken);

            (List<(string commitMessage, DateTime commitDate)> commits, int total) result = (new List<(string commitMessage, DateTime commitDate)>(), 0);

            try
            {
                while (page <= totalPages)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string relativeUri = $"repos/{orgName}/{repositoryName}/commits?page={page}&per_page={perPage}";

                    using HttpResponseMessage response = await client.GetAsync(relativeUri, cancellationToken);

                    response.EnsureSuccessStatusCode();

                    using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);

                    IEnumerable<JsonElement>? commits = await JsonSerializer.DeserializeAsync<IEnumerable<JsonElement>>(stream, cancellationToken: cancellationToken);

                    if (commits == null || !commits.Any())
                    {
                        break;
                    }

                    foreach (JsonElement commit in commits)
                    {
                        string commitMessage = commit.GetProperty("commit").GetProperty("message").GetString() ?? string.Empty;
                        DateTime commitDate = commit.GetProperty("commit").GetProperty("committer").GetProperty("date").GetDateTime();

                        result.commits.Add((commitMessage, commitDate));
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
            catch (HttpRequestException ex)
            {
                Console.WriteLine("The request failed due to an issue getting a valid HTTP response, such as network connectivity failure, DNS failure, server certificate validation error, or invalid server response. ");
                Console.WriteLine(".NET Framework only: the request timed out.");
                return result;
            }
            catch (UriFormatException ex)
            {
                Console.WriteLine("The provided request URI is not valid relative or absolute URI.");
                return result;
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine("The cancellation token was canceled. This exception is stored into the returned task.");
                Console.WriteLine(".NET Core and .NET 5 and later only: The request failed due to timeout.");
                throw;
            }
        }
    }
}
