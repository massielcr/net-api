using System.Text.Json;

namespace HttpClientMethods.Services
{
    public class PutAsyncEndpointsService(IHttpClientFactory httpClientFactory, ILogger<PutAsyncEndpointsService> logger) : IPutAsyncEndpointsService
    {
        public async Task<bool> ReplaceRepositoryTopicsAsync(string owner, string repo, List<string> names)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("GitHub");

            string relativeUri = $"repos/{owner}/{repo}/topics";

            JsonContent content = JsonContent.Create(new { names });

            try
            {
                using HttpResponseMessage response = await httpClient.PutAsync(relativeUri, content);

                if (!response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    logger.LogError("Failed to replace repository topics. Status Code: {StatusCode}, Response: {ResponseContent}", response.StatusCode, responseContent);
                    return false;
                }

                return true;
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

            return false;
        }
    }
}
