using HttpClientMethods.Dtos;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;

namespace HttpClientMethods.Services
{
    public class SendAsyncEndpointsService(IHttpClientFactory httpClientFactory, ILogger<SendAsyncEndpointsService> logger) : ISendAsyncEndpointsService
    {
        #region Specific HTTP Methods

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

        //HEAD
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

        //OPTIONS
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

        #endregion


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
            HttpClient httpClient = httpClientFactory.CreateClient("Local");

            JsonSerializerOptions options = new(JsonSerializerDefaults.Web);

            Uri serverPosterUri = new($"sendasync/server/posters/{posterId}", UriKind.Relative);

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, serverPosterUri);

            httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                using HttpResponseMessage respone = await httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead);

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
            HttpClient httpClient = httpClientFactory.CreateClient("Local");

            JsonSerializerOptions options = new(JsonSerializerDefaults.Web);

            Uri uri = new($"sendasync/server/posters", UriKind.Relative);

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

                using HttpResponseMessage response = await httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError($"Failed to create poster. Status code: {response.StatusCode}");
                    return string.Empty;    
                }

                return response.Headers.Location?.ToString() ?? string.Empty;

            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, "An error occurred while preparing poster data for creation.");               
            }
            catch(UriFormatException ex)
            {
                logger.LogError(ex, "An error occurred while preparing poster data for creation.");
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "An error occurred while preparing poster data for creation.");
            }
            catch (OperationCanceledException ex)
            {
                logger.LogError(ex, "An error occurred while preparing poster data for creation.");
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "An error occurred while preparing poster data for creation.");
            }

            return string.Empty;
        }


        public async Task<PosterDto?> GetCompressedPosterServerAsync(string posterId)
        {
            var Random = new Random();

            byte[] generatedData = new byte[1024 * 1024 * 5]; // 5 MB of random data
            Random.NextBytes(generatedData);

            return new PosterDto(posterId, "Generated poster", generatedData);
        }

        public async Task<PosterDto?> GetCompressedPosterClientAsync(string posterId)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("Local");

            JsonSerializerOptions options = new(JsonSerializerDefaults.Web);

            Uri uri = new($"sendasync/server/posters/{posterId}", UriKind.Relative);

            try
            {
                HttpRequestMessage request = new(HttpMethod.Get, uri);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

                using HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError($"Failed to get compressed poster with id {posterId}. Status code: {response.StatusCode}");
                    return null;
                }

                using Stream responseStream = await response.Content.ReadAsStreamAsync();

                PosterDto? posterDto = await JsonSerializer.DeserializeAsync<PosterDto>(responseStream, options);

                return posterDto;
            }
            catch(InvalidOperationException ex) 
            {
                logger.LogError(ex, $"An error occurred while fetching compressed poster with id {posterId}.");
            }
            catch (UriFormatException ex)
            {
                logger.LogError(ex, $"An error occurred while fetching compressed poster with id {posterId}.");
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, $"An error occurred while fetching compressed poster with id {posterId}.");
            }
            catch (OperationCanceledException ex)
            {
                logger.LogError(ex, $"An error occurred while fetching compressed poster with id {posterId}.");
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, $"An error occurred while fetching compressed poster with id {posterId}.");
            }

            return null;
        }

        #endregion


        public async Task<(IEnumerable<string> Repos, double time)> GetRepositoriesAsync(string orgName, int page, int perPage, int totalPages, CancellationToken cancellationToken)
        {
            List<string> result = [];

            int currentPage = page;

            HttpClient httpClient = httpClientFactory.CreateClient("GitHub");

            var time = new Stopwatch();

            time.Start();

            while (currentPage <= totalPages)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Uri url = new($"orgs/{Uri.EscapeDataString(orgName)}/repos?page={currentPage}&per_page={perPage}", UriKind.Relative);

                HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);

                using HttpResponseMessage response = await httpClient.SendAsync(httpRequestMessage, cancellationToken);

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

            HttpClient httpClient = httpClientFactory.CreateClient("GitHub");

            var time = new Stopwatch();

            time.Start();

            List<Task<HttpResponseMessage>> tasks = [];

            while (currentPage <= totalPages)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Uri url = new($"orgs/{Uri.EscapeDataString(orgName)}/repos?page={currentPage}&per_page={perPage}", UriKind.Relative);

                HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);

                tasks.Add(httpClient.SendAsync(httpRequestMessage, cancellationToken));

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
