using System.Diagnostics;
using System.Text.Json;

namespace HttpClientMethods.Services
{
    public class SendAsyncEndpointsService : ISendAsyncEndpointsService
    {
        private readonly static HttpClient _httpClient = new();

        private const string BaseUrl = "https://api.github.com/";
        private readonly string? _githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");

        public async Task<(IEnumerable<string> Repos, double time)> GetRepositoriesAsync(string orgName, int page, int perPage, int totalPages, CancellationToken cancellationToken)
        {
            List<string> result = [];

            int currentPage = page;

            var time = new Stopwatch();

            time.Start();

            while (currentPage <= totalPages)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Uri url = new($"{BaseUrl}orgs/{Uri.EscapeDataString(orgName)}/repos?page={currentPage}&per_page={perPage}");

                HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);

                httpRequestMessage.Headers.Add("Accept", "application/json");
                httpRequestMessage.Headers.Add("User-Agent", "MyTestService");
                httpRequestMessage.Headers.Add("Authorization", $"Bearer {_githubToken}");

                using HttpResponseMessage response = await _httpClient.SendAsync(httpRequestMessage, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);

                    IEnumerable<JsonElement>? data = await JsonSerializer.DeserializeAsync<IEnumerable<JsonElement>>(stream, cancellationToken: cancellationToken);

                    foreach (JsonElement element in data ?? [])
                    {
                        if (element.TryGetProperty("name", out var name))
                        {
                            string repoName = name.GetString() ?? string.Empty;

                            result.Add(repoName);
                        }
                    }
                }

                currentPage++;
            }

            time.Stop();

            return (result, time.Elapsed.TotalMilliseconds);
        }

        public async Task<(IEnumerable<string> Repos, double time)> GetRepositoriesParallelAsync(string orgName, int page, int perPage, int totalPages, CancellationToken cancellationToken)
        {
            List<string> result = [];

            int currentPage = page;

            var time = new Stopwatch();

            time.Start();

            List<Task<HttpResponseMessage>> tasks = [];

            while (currentPage <= totalPages)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Uri url = new($"{BaseUrl}orgs/{Uri.EscapeDataString(orgName)}/repos?page={currentPage}&per_page={perPage}");

                HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);

                httpRequestMessage.Headers.Add("Accept", "application/json");
                httpRequestMessage.Headers.Add("User-Agent", "MyTestService");
                httpRequestMessage.Headers.Add("Authorization", $"Bearer {_githubToken}");

                tasks.Add(_httpClient.SendAsync(httpRequestMessage, cancellationToken));

                currentPage++;
            }

            try
            {
                HttpResponseMessage[] responses = await Task.WhenAll(tasks);
                foreach (HttpResponseMessage response in responses)
                {
                    using (response)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);

                            IEnumerable<JsonElement>? data = await JsonSerializer.DeserializeAsync<IEnumerable<JsonElement>>(stream, cancellationToken: cancellationToken);

                            foreach (JsonElement element in data ?? [])
                            {
                                if (element.TryGetProperty("name", out var name))
                                {
                                    string repoName = name.GetString() ?? string.Empty;

                                    result.Add(repoName);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                foreach (var task in tasks)
                {
                    if (task.IsCompletedSuccessfully)
                    {
                        task.Result.Dispose();
                    }
                }
                throw;
            }

            time.Stop();

            return (result, time.Elapsed.TotalMilliseconds);
        }
    }
}
