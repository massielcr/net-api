namespace HttpClientMethods.Interfaces
{
    public interface ISendAsyncEndpointsService
    {
        #region Specific HTTP Methods

        Task<IEnumerable<string>> GetAvatarHeadersAsync(string username);

        Task<IEnumerable<string>> GetUserOptionsAsync(string username);

        #endregion


        #region OTHERS

        Task<(IEnumerable<string> Repos, double time)> GetRepositoriesAsync(string orgName, int page, int perPage, int totalPages, CancellationToken cancellationToken);

        Task<(IEnumerable<string> Repos, double time)> GetRepositoriesParallelAsync(string orgName, int page, int perPage, int totalPages, CancellationToken cancellationToken);

        #endregion
    }
}
