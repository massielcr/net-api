using HttpClientMethods.Interfaces;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
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


        #region BASIC TASKS


        //Task - create issue with cancellation token, timeout, and exception handling
        public async Task CreateIssueAsync(string owner, string repo, string title, string body, CancellationToken cancellationToken)
        {
            JsonSerializerOptions options = new(JsonSerializerDefaults.Web);

            HttpClient httpClient = httpClientFactory.CreateClient("GitHub");

            Uri uri = new($"repos/{owner}/{repo}/issues", UriKind.Relative);

            var issue = new
            {
                title,
                body
            };

            try
            {
                JsonContent issueContent = JsonContent.Create(issue, options: options);

                using HttpRequestMessage request = new(HttpMethod.Post, uri);
                request.Content = issueContent;

                using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError("Failed to create issue. Status code: {StatusCode}", response.StatusCode);

                    return;
                }

                using Stream responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);

                JsonElement responseElement = await JsonSerializer.DeserializeAsync<JsonElement>(responseStream, cancellationToken: cancellationToken);

                long? issueId = null;

                if (responseElement.TryGetProperty("id", out var id))
                {
                    issueId = id.GetInt64();
                }

                logger.LogInformation($"Issue {issueId} created successfully in {owner}/{repo}");
            }
            catch(InvalidOperationException ex)
            {
                logger.LogError(ex, $"The requestUri is not an absolute URI and BaseAddress isn't set.");
            }
            catch(UriFormatException ex) 
            { 
                logger.LogError(ex, $"The requestUri is not a valid URI.");
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, $"An error occurred while sending the request.");
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "JSON formatting or parsing failed");
            }
            catch (OperationCanceledException ex) 
            { 
                logger.LogError(ex, $"The operation was canceled.");
                throw;
            }
        }

        
        //Task<int> - get repo info with cancellation token, timeout, and exception handling
        public async Task<JsonElement> GetRepoInfoAsync(string owner, string repo, CancellationToken cancellationToken)
        {
            JsonSerializerOptions options = new(JsonSerializerDefaults.Web);

            HttpClient httpClient = httpClientFactory.CreateClient("GitHub");

            string safeOwner = Uri.EscapeDataString(owner);
            string safeRepo = Uri.EscapeDataString(repo);

            Uri uri = new($"repos/{safeOwner}/{safeRepo}", UriKind.Relative);

            try
            {
                using HttpRequestMessage request = new(HttpMethod.Get, uri);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));

                using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError("Failed to get repo info for {owner}/{repo}. Status code: {StatusCode}", owner, repo, response.StatusCode);
                    return default;
                }

                using Stream responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);

                JsonElement? responseObject = await JsonSerializer.DeserializeAsync<JsonElement>(responseStream, options: options, cancellationToken: cancellationToken);

                if (responseObject != null)
                {
                    return responseObject.Value;
                }
            }
            catch(InvalidOperationException ex)
            {
                logger.LogError(ex, $"The requestUri is not an absolute URI and BaseAddress isn't set.");
            }
            catch(UriFormatException ex)
            {
                logger.LogError(ex, $"The requestUri is not a valid URI.");
            }
            catch(HttpRequestException ex)
            {
                logger.LogError(ex, $"An error occurred while sending the request.");
            }
            catch(JsonException ex)
            {
                logger.LogError(ex, "JSON formatting or parsing failed");
            }
            catch(OperationCanceledException ex)
            {
                logger.LogError(ex, $"The operation was canceled.");
                throw;
            }            

            return default;
        }

        //Task.WhenAny - get the first completed repo info with cancellation token, timeout, and exception handling
        public async Task<JsonElement> GetAnyRepoInfoAsync(string owner, CancellationToken cancellationToken)
        {
            return default;
        }

        //Task.WhenAll - get multiple repo info in parallel with cancellation token, timeout, and exception handling
        public async Task<IEnumerable<JsonElement>> GetReposInfoAsync(string owner, CancellationToken cancellationToken)
        {
            return [];
        }

        //Task.WhenEach - get repo info for each repo in a list with cancellation token, timeout, and exception handling
        public async IAsyncEnumerable<JsonElement> GetReposInfoEnumerableAsync(string owner, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            yield return default;
        }

        #endregion


        #region OTHERS

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

        #endregion
    }
}
