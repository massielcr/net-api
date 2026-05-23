namespace HttpClientMethods.Services
{
    public interface IGetStringAsyncEndpointsService
    {
        Task<string?> GetRepositoryReadmeStringAsync(string owner, string repo);
        Task<string?> GetRepositoryReadmeUriAsync(string owner, string repo);
    }
}
