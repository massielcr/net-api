using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace HttpClientMethods.Services
{
    public class GetEndpointsService(IHttpClientFactory clientFactory) : IGetEndpointsService
    {
        private const string BaseUrl = "https://api.github.com/";

        public async Task<int> GetRepositoriesCountAsync()
        {
            HttpClient client = clientFactory.CreateClient();

            client.BaseAddress = new Uri(BaseUrl);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "MyTestService");

            string relativeUri = $"orgs/dotnet/repos";            

            using HttpResponseMessage response = await client.GetAsync(relativeUri);

            response.EnsureSuccessStatusCode();

            using Stream stream = await response.Content.ReadAsStreamAsync();

            IEnumerable<JsonElement>? repositories = await JsonSerializer.DeserializeAsync<IEnumerable<JsonElement>>(stream);

            return repositories?.Count() ?? 0;
        }

        public async Task<IEnumerable<string>> GetAllRepositoriesAsync()
        {
            HttpClient client = clientFactory.CreateClient();

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "MyTestService");

            Uri uri = new($"{BaseUrl}orgs/dotnet/repos");

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

        public async IAsyncEnumerable<(string commitMessage, DateTime commitDate, int total)> GetRepositoryCommits(string orgName, string repositoryName, int page, int perPage, [EnumeratorCancellation]  CancellationToken cancellationToken)
        {
            HttpClient client = clientFactory.CreateClient();

            client.BaseAddress = new Uri(BaseUrl);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "MyTestService");

            int counter = 0;
            int pageCounter = 0;

            while (pageCounter < 10)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string relativeUri = $"repos/{orgName}/{repositoryName}/commits?page={page + pageCounter}&per_page={perPage}";

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
    
                    yield return (commitMessage, commitDate, ++counter);
                }

                pageCounter++;
            }
        }
    }
}
