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

        public async Task<(List<(string commitMessage, DateTime commitDate)> commits, int total)> GetRepositoryCommits(string orgName, string repositoryName, int page, int perPage, int totalPages, CancellationToken cancellationToken)
        {
            HttpClient client = clientFactory.CreateClient();

            client.BaseAddress = new Uri(BaseUrl);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "MyTestService");

            (List<(string commitMessage, DateTime commitDate)> commits, int total) result = (new List<(string commitMessage, DateTime commitDate)>(), 0);

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
    }
}
