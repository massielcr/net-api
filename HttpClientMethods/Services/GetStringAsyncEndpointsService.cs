using HttpClientMethods.Interfaces;

namespace HttpClientMethods.Services
{
    public class GetStringAsyncEndpointsService(IHttpClientFactory httpClientFactory, ILogger<GetStringAsyncEndpointsService> logger) : IGetStringAsyncEndpointsService
    {
        //GetStringAsync(String)
        public async Task<string?> GetRepositoryReadmeStringAsync(string owner, string repo)
        {
            string relativeUri = $"repos/{owner}/{repo}/readme";

            try
            {
                HttpClient httpClient = httpClientFactory.CreateClient("GitHub");

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


        //GetStringAsync(Uri)
        public async Task<string?> GetRepositoryReadmeUriAsync(string owner, string repo)
        {
            Uri uri = new($"repos/{owner}/{repo}/readme", UriKind.Relative);

            try
            {
                HttpClient httpClient = httpClientFactory.CreateClient("GitHub");

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
