namespace HttpClientMethods.Interfaces
{
    public interface IDeleteAsyncEndpointsService
    {
        //DeleteAsync(String)
        Task<bool> UnlockIssueAsync(string owner, string repo, int id);

        //DeleteAsync(String, CancellationToken)
        Task<bool> UnlockIssuesAsync(string owner, string repo, IEnumerable<int> ids, CancellationToken cancellationToken);


        //DeleteAsync(Uri)
        Task<bool> UnlockIssueUriAsync(string owner, string repo, int id);

        //DeleteAsync(Uri, CancellationToken)
        IAsyncEnumerable<int> UnlockIssuesStreamAsync(string owner, string repo, IEnumerable<int> ids, CancellationToken cancellationToken);
    }
}
