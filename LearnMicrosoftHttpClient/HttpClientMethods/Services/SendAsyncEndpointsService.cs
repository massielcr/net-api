using HttpClientMethods.Dtos;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
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

            try
            {
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
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"An error occurred while fetching image headers for user {username}.");
            }

            return headers;
        }

        public async Task<IEnumerable<string>> GetUserOptionsAsync(string username)
        {
            HashSet<string> options = [];

            HttpClient httpClient = httpClientFactory.CreateClient("GitHub");

            Uri uri = new($"users/{username}", UriKind.Relative);

            HttpRequestMessage userRequest = new(HttpMethod.Options, uri);

            try
            {
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
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"An error occurred while fetching user options for user {username}.");
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

            try
            {
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
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"An error occurred while fetching avatar headers for user {username}.");
            }

            return avatarUrl;
        }


        #region Streams

        public async Task<PosterDto> GetPosterServerAsync(string posterId)
        {
            var Random = new Random();

            var generatedData = new byte[1024 * 1024 * 5]; // 5 MB of random data
            Random.NextBytes(generatedData);

            return new PosterDto(posterId, "Generated poster", generatedData);
        }

        public async Task<PosterDto?> GetPosterClientAsync(string posterId)
        {
            JsonSerializerOptions options = new(JsonSerializerDefaults.Web);

            Uri serverPosterUri = new($"http://localhost:5099/sendasync/server/posters/{posterId}", UriKind.Absolute);

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, serverPosterUri);

            httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                using HttpResponseMessage respone = await _httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead);

                if (!respone.IsSuccessStatusCode)
                {
                    logger.LogError($"Failed to get poster with id {posterId}. Status code: {respone.StatusCode}");
                    return null;
                }

                using Stream responseStream = await respone.Content.ReadAsStreamAsync();

                PosterDto? poster = await JsonSerializer.DeserializeAsync<PosterDto>(responseStream, options);

                return poster;
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, $"An error occurred while fetching poster with id {posterId}.");

            }
            catch (UriFormatException ex)
            {
                logger.LogError(ex, $"An error occurred while fetching poster with id {posterId}.");
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, $"An error occurred while fetching poster with id {posterId}.");
            }
            catch (OperationCanceledException ex)
            {
                logger.LogError(ex, $"An error occurred while fetching poster with id {posterId}.");
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, $"An error occurred while fetching poster with id {posterId}.");
            }

            return null;
        }

        public async Task<bool> CreatePosterServerAsync(PosterDto poster)
        {
            if (poster.Data == null)
            {
                logger.LogError("Invalid poster data provided for creation.");
                return false;
            }

            poster.Id = Guid.NewGuid().ToString();

            return true;
        }

        public async Task<string> CreatePosterClientAsync(PosterDto poster)
        {
            JsonSerializerOptions options = new(JsonSerializerDefaults.Web);

            Uri uri = new($"http://localhost:5099/sendasync/server/posters", UriKind.Absolute);

            try
            {
                using MemoryStream memoryStream = new();
                await JsonSerializer.SerializeAsync(memoryStream, poster, options);

                memoryStream.Seek(0, SeekOrigin.Begin);

                using StreamContent streamContent = new(memoryStream);
                streamContent.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

                using HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, uri);
                httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpRequestMessage.Content = streamContent;

                using HttpResponseMessage response = await _httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError($"Failed to create poster. Status code: {response.StatusCode}");
                    return string.Empty;    
                }

                return response.Headers.Location?.ToString() ?? string.Empty;

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while preparing poster data for creation.");
                return string.Empty;

            }
        }

        #endregion


    }
}
