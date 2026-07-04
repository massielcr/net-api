using System.Text.Json;

namespace HttpClientMethods.Interfaces
{
    public interface ISendAsyncEndpointsService
    {
        #region Specific HTTP Methods

        Task<IEnumerable<string>> GetAvatarHeadersAsync(string username);

        Task<IEnumerable<string>> GetUserOptionsAsync(string username);

        #endregion



        #region BASIC TASKS


        //Task - create issue with cancellation token, timeout, and exception handling
        Task CreateIssueAsync(string owner, string repo, string title, string body, CancellationToken cancellationToken);


        //Task<T> - get repo info with cancellation token, timeout, and exception handling
        Task<JsonElement> GetRepoInfoAsync(string owner, string repo, CancellationToken cancellationToken);


        //Task.WhenAny - get the first completed repo info with cancellation token, timeout, and exception handling
        //Task.WhenAll - get multiple repo info in parallel with cancellation token, timeout, and exception handling
        Task<(string owner, IEnumerable<string> repos)?> GetReposInfoAsync(string owner, CancellationToken cancellationToken);   


        //Task.WhenEach - get repo info for each repo in a list with cancellation token, timeout, and exception handling
        IAsyncEnumerable<string> GetReposInfoEnumerableAsync(string owner, CancellationToken cancellationToken);


        #endregion



        #region OTHERS

        Task<(IEnumerable<string> Repos, double time)> GetRepositoriesAsync(string orgName, int page, int perPage, int totalPages, CancellationToken cancellationToken);

        Task<(IEnumerable<string> Repos, double time)> GetRepositoriesParallelAsync(string orgName, int page, int perPage, int totalPages, CancellationToken cancellationToken);

        #endregion
    }
}
