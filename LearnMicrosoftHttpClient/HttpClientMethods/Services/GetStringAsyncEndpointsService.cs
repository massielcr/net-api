namespace HttpClientMethods.Services
{
    public class GetStringAsyncEndpointsService(IHttpClientFactory httpClientFactory, ILogger<GetStringAsyncEndpointsService> logger) : IGetStringAsyncEndpointsService
    {
        private const string BaseUrl = "https://api.github.com";

        public async Task<string?> GetRepositoryReadmeStringAsync(string owner, string repo)
        {
            try
            {
                HttpClient httpClient = httpClientFactory.CreateClient("GitHub");

                string relativeUri = $"repos/{owner}/{repo}/readme";

                string? responseString = await httpClient.GetStringAsync(relativeUri);

                if (string.IsNullOrWhiteSpace(responseString)) { return null; }

                return responseString;
            }
            catch(InvalidOperationException ex)
            {
                logger.LogError(ex, $"The requestUri is not an absolute URI and BaseAddress isn't set.");
            }
            catch (UriFormatException ex)
            {
                logger.LogError(ex, $"The provided request URI is not valid relative or absolute URI.");
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, $"The request failed due to an issue getting a valid HTTP response, such as network connectivity failure, DNS failure, server certificate validation error, or invalid server response");
                logger.LogError(ex, $".NET Framework only: the request timed out.");
            }
            catch (OperationCanceledException ex)
            {
                logger.LogError(ex, $".NET Core and .NET 5 and later only: The request failed due to timeout.");
            }

            return null;            
        }

        public async Task<string?> GetRepositoryReadmeUriAsync(string owner, string repo)
        {
            try
            {
                HttpClient httpClient = httpClientFactory.CreateClient("GitHub");

                Uri uri = new($"{BaseUrl}/repos/{owner}/{repo}/readme");

                string? responseString = await httpClient.GetStringAsync(uri);

                if (string.IsNullOrWhiteSpace(responseString)) { return null; }

                return responseString;
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, $"The requestUri is not an absolute URI and BaseAddress isn't set.");
            }
            catch (UriFormatException ex)
            {
                logger.LogError(ex, $"The provided request URI is not valid relative or absolute URI.");
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, $"The request failed due to an issue getting a valid HTTP response, such as network connectivity failure, DNS failure, server certificate validation error, or invalid server response");
                logger.LogError(ex, $".NET Framework only: the request timed out.");
            }
            catch (OperationCanceledException ex)
            {
                logger.LogError(ex, $".NET Core and .NET 5 and later only: The request failed due to timeout.");
            }

            return null;
        }
    }
}
