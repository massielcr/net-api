using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Text.Json;

namespace HttpClientMethods.Services
{
    public class SendAsyncEndpointsService(IHttpClientFactory httpClientFactory, ILogger<SendAsyncEndpointsService> logger) : ISendAsyncEndpointsService
    {
        private readonly static HttpClient _httpClient = new();

        private const string BaseUrl = "https://api.github.com/";
        private readonly string? _githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");


        public async Task<IEnumerable<string>> GetAvatarHeadersAsync(string username)
        {
            List<string> headers = [];

            string? avatarUrl = await GetAvatarUrl(username);

            if (string.IsNullOrWhiteSpace(avatarUrl))
            {
                logger.LogError($"Failed to get avatar_url for user {username}.");
                return headers;
            }

            HttpClient imageClient = httpClientFactory.CreateClient();

            HttpRequestMessage imageRequest = new(HttpMethod.Head, avatarUrl);

            HttpResponseMessage imageResponse = await imageClient.SendAsync(imageRequest);

            if (!imageResponse.IsSuccessStatusCode) 
            {
                logger.LogError($"Failed to get image headers for user {username}. Status code: {imageResponse.StatusCode}"); 
                return headers;
            }

            foreach (var header in imageResponse.Headers)
            {
                headers.Add($"{header.Key}: {string.Join(", ", header.Value)}");
            }

            return headers;
        }

        public async Task<IEnumerable<string>> GetUserOptionsAsync(string username)
        {
            HashSet<string> options = [];

            HttpClient httpClient = httpClientFactory.CreateClient("GitHub");

            Uri uri = new($"users/{username}", UriKind.Relative);

            HttpRequestMessage userRequest = new(HttpMethod.Options, uri);

            using HttpResponseMessage userResponse = await httpClient.SendAsync(userRequest);

            if (!userResponse.IsSuccessStatusCode)
            {
                logger.LogError($"Failed to get user options for user {username}. Status code: {userResponse.StatusCode}");
                return options;
            }

            if (userResponse.Headers.Contains("Access-Control-Allow-Methods"))
            {
                var allowHeaderValues = userResponse.Headers.GetValues("Access-Control-Allow-Methods");
                foreach (var val in allowHeaderValues)
                {
                    options.UnionWith(val.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
                }
            }

            return options;
        }

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

        public async Task<string?> GetAvatarUrl(string username)
        {
            string? avatarUrl = null;

            HttpClient httpClient = httpClientFactory.CreateClient("GitHub");

            Uri uri = new($"users/{username}", UriKind.Relative);

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, uri);

            using HttpResponseMessage response = await httpClient.SendAsync(httpRequestMessage);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError($"Failed to get avatar headers for user {username}. Status code: {response.StatusCode}");
                return avatarUrl;
            }

            JsonDocument jsonDocument = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());            

            if (jsonDocument.RootElement.TryGetProperty("avatar_url", out var avatar_url))
            {
                avatarUrl = avatar_url.GetString();
            }

            return avatarUrl;
        }
    }
}
