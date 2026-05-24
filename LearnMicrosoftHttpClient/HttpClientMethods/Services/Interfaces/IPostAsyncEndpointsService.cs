namespace HttpClientMethods.Services
{
    public interface IPostAsyncEndpointsService
    {
        public Task<bool> CreatePersonalRepositoryAsync(string name, string description, bool isPrivate, bool initialREADME, bool hasDownloads);

        public Task<bool> CreateRepositoryIssuesAsync(string owner, string repo, string title, string body, int count, CancellationToken cancellationToken);

        public Task<bool> CreateRepositoryIssueAsync(string owner, string repo, string title, byte[] imageBody);

        public Task<bool> CreateRepositoryIssuesAsync(string owner, string repo, string title, byte[] imageBody, int count, CancellationToken cancellationToken);

        public Task<string?> CreateRepositoryContentAssetAsync(string owner, string repo, string title, byte[] imageBody, CancellationToken cancellationToken = default);
    }
}
