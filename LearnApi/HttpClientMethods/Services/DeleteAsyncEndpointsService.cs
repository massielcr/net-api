using System.Runtime.CompilerServices;

namespace HttpClientMethods.Services
{
    public class DeleteAsyncEndpointsService(IHttpClientFactory httpClientFactory, ILogger<DeleteAsyncEndpointsService> logger) : IDeleteAsyncEndpointsService
    {
        //DeleteAsync(String)
        public async Task<bool> UnlockIssueAsync(string owner, string repo, int id)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("GitHub");

            string uri = $"repos/{owner}/{repo}/issues/{id}/lock";

            try
            {
                using HttpResponseMessage response = await httpClient.DeleteAsync(uri);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning("Failed to unlock issue {Owner}/{Repo}#{Id}. Status code: {StatusCode}", owner, repo, id, response.StatusCode);
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

        //DeleteAsync(String, CancellationToken)
        public async Task<bool> UnlockIssuesAsync(string owner, string repo, IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("GitHub");

            foreach (var id in ids)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string uri = $"repos/{owner}/{repo}/issues/{id}/lock";

                try
                {
                    using HttpResponseMessage response = await httpClient.DeleteAsync(uri, cancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        logger.LogWarning("Failed to unlock issue {Owner}/{Repo}#{Id}. Status code: {StatusCode}", owner, repo, id, response.StatusCode);
                        return false;
                    }
                }
                catch (InvalidOperationException ex)
                {
                    logger.LogError(ex, $"The requestUri is not an absolute URI and BaseAddress isn't set.");
                    return false;
                }
                catch (UriFormatException ex)
                {
                    logger.LogError(ex, $"The provided request URI is not valid relative or absolute URI.");
                    return false;
                }
                catch (HttpRequestException ex)
                {
                    logger.LogError(ex, $"The request failed due to an issue getting a valid HTTP response, such as network connectivity failure, DNS failure, server certificate validation error, or invalid server response");
                    logger.LogError(ex, $".NET Framework only: the request timed out.");
                    return false;
                }
                catch (OperationCanceledException ex)
                {
                    logger.LogError(ex, $".NET Core and .NET 5 and later only: The request failed due to timeout.");
                    throw;
                }
            }

            return true;
        }


        //DeleteAsync(Uri)
        public async Task<bool> UnlockIssueUriAsync(string owner, string repo, int id)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("GitHub");

            Uri uri = new($"repos/{owner}/{repo}/issues/{id}/lock", UriKind.Relative);

            try
            {
                using HttpResponseMessage response = await httpClient.DeleteAsync(uri);

                if(!response.IsSuccessStatusCode)
                {
                    logger.LogWarning("Failed to unlock issue {Owner}/{Repo}#{Id}. Status code: {StatusCode}", owner, repo, id, response.StatusCode);
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

        //DeleteAsync(Uri, CancellationToken)
        public async IAsyncEnumerable<int> UnlockIssuesStreamAsync(string owner, string repo, IEnumerable<int> ids, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("GitHub");

            await foreach (var id in ids.ToAsyncEnumerable().WithCancellation(cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                bool success = false;

                Uri uri = new($"repos/{owner}/{repo}/issues/{id}/lock", UriKind.Relative);

                try
                {
                    using HttpResponseMessage response = await httpClient.DeleteAsync(uri, cancellationToken);

                    success = response.IsSuccessStatusCode;

                    if(!success)
                    {
                        logger.LogWarning("Failed to unlock issue {Owner}/{Repo}#{Id}. Status code: {StatusCode}", owner, repo, id, response.StatusCode);
                    }
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

                if (success)
                {
                    yield return id;
                }
            }

            yield break;
        }        
    }
}
