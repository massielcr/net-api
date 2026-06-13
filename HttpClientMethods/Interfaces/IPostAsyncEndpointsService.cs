namespace HttpClientMethods.Interfaces
{
    public interface IPostAsyncEndpointsService
    {
        //PostAsync(String, HttpContent)
        public Task<bool> CreatePersonalRepositoryAsync(string name, string description, bool isPrivate, bool initialREADME, bool hasDownloads);

        //PostAsync(String, HttpContent, CancellationToken)
        public Task<bool> CreateRepositoryIssuesAsync(string owner, string repo, string title, string body, int count, CancellationToken cancellationToken);


        //PostAsync(Uri, HttpContent)
        public Task<bool> CreateRepositoryIssueAsync(string owner, string repo, string title, byte[] imageBody);

        //PostAsync(Uri, HttpContent, CancellationToken)
        public Task<bool> CreateRepositoryIssuesAsync(string owner, string repo, string title, byte[] imageBody, int count, CancellationToken cancellationToken);


        public Task<string?> CreateRepositoryContentAssetAsync(string owner, string repo, string title, byte[] imageBody, CancellationToken cancellationToken = default);
    }
}
