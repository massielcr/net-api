using System.Runtime.CompilerServices;

namespace HttpClientMethods.Services
{
    public interface IGetEndpointsService
    {
        Task<int> GetRepositoriesCountAsync();
        Task<IEnumerable<string>> GetAllRepositoriesAsync();
        IAsyncEnumerable<(string commitMessage, DateTime commitDate, int total)> GetRepositoryCommits(string orgName, string repositoryName, int page, int perPage, CancellationToken cancellationToken)
    }
}
