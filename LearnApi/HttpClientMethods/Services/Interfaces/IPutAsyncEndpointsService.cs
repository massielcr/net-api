using HttpClientMethods.Models;

namespace HttpClientMethods.Services
{
    internal interface IPutAsyncEndpointsService
    {
        //PutAsync(String, HttpContent)
        Task<bool> UpdateRepositoryTopicsAsync(string owner, string repo, List<string> names);


        //PutAsync(String, HttpContent, CancellationToken)
        Task<bool> LockRepositoryIssuesAsync(string owner, string repo, List<GitHubIssue> githubIssues, CancellationToken cancellationToken);


        //PutAsync(Uri, HttpContent)
        Task<bool> LockRepositoryIssueAsync(string owner, string repo, GitHubIssue githubIssue);


        //PutAsync(Uri, HttpContent, CancellationToken)
        IAsyncEnumerable<int> LockRepositoryIssuesStreamAsync(string owner, string repo, List<GitHubIssue> githubIssues, CancellationToken cancellationToken);
    }
}