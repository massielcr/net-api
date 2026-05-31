namespace HttpClientMethods.Services
{
    public interface ISendAsyncEndpointsService
    {
        Task<IEnumerable<string>> GetAvatarHeadersAsync(string username);

        Task<IEnumerable<string>> GetUserOptionsAsync(string username);


        Task<(IEnumerable<string> Repos, double time)> GetRepositoriesAsync(string orgName, int page, int perPage, int totalPages, CancellationToken cancellationToken);

        Task<(IEnumerable<string> Repos, double time)> GetRepositoriesParallelAsync(string orgName, int page, int perPage, int totalPages, CancellationToken cancellationToken);
    }
}
