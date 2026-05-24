using HttpClientMethods.Dtos;

namespace HttpClientMethods.Services
{
    public interface IGetAsyncEndpointsService
    {
        #region relativeUri

        //GetAsync(String)
        Task<int?> GetRepositoriesCountAsync(string orgName);

        //GetAsync(String, CancellationToken)
        Task<IEnumerable<string>> GetRepositoriesAsync(string orgName, int page, int perPage, int totalPages, CancellationToken cancellationToken);


        //GetAsync(String, HttpCompletionOption)
        Task<string?> GetRepositoriesReadmeAsync(string owner, string repo, string contentType);


        //GetAsync(String, HttpCompletionOption, CancellationToken)
        IAsyncEnumerable<GitHubRepositoryDto> GetRepositoriesStreamAsync(string orgName, int perPage, CancellationToken cancellationToken);

        #endregion


        #region Uri

        //GetAsync(Uri)
        Task<int?> GetCommitsCountAsync(string owner, string repo);

        //GetAsync(Uri, CancellationToken)
        Task<IEnumerable<(string commitMessage, DateTime commitDate)>> GetCommitsAsync(string orgName, string repositoryName, int page, int perPage, int totalPages, CancellationToken cancellationToken);

        //GetAsync(Uri, HttpCompletionOption)
        IAsyncEnumerable<(string commitMessage, DateTime commitDate)> GetCommitsStreamAsync(string orgName, string repositoryName, int page, int perPage, int totalPages);

        //GetAsync(Uri, HttpCompletionOption, CancellationToken)
        IAsyncEnumerable<(string commitMessage, DateTime commitDate)> GetCommitsStreamAsync(string orgName, string repositoryName, int page, int perPage, int totalPages, CancellationToken cancellationToken);

        #endregion
    }
}
