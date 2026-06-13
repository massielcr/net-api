using HttpClientMethods.Models;

namespace HttpClientMethods.Interfaces
{
    public interface IPatchAsyncEndpoinsService
    {
        //PatchAsync(String, HttpContent)
        Task<bool> UpdateRepoVisibilityAsync(string owner, string repo, string visibility);

        //PatchAsync(String, HttpContent, CancellationToken)
        Task<bool> UpdateRepoIssuesAsync(string owner, string repo, IEnumerable<GitHubIssue> issues, CancellationToken cancellationToken);


        //PatchAsync(Uri, HttpContent)
        Task<bool> UpdateRepoIssueAsync(string owner, string repo, GitHubIssue issue);

        //PatchAsync(Uri, HttpContent, CancellationToken)
        IAsyncEnumerable<int> UpdateRepoIssuesStreamAsync(string owner, string repo, IEnumerable<GitHubIssue> issues, CancellationToken cancellationToken);
    }
}
