using System.Runtime.CompilerServices;

namespace HttpClientMethods.Services
{
    public interface IGetEndpointsService
    {
        Task<int> GetRepositoriesCountAsync();
        Task<IEnumerable<string>> GetAllRepositoriesAsync();
        Task<(List<(string commitMessage, DateTime commitDate)> commits, int total)> GetRepositoryCommits(string orgName, string repositoryName, int page, int perPage, int totalPages, CancellationToken cancellationToken);
    }
}
