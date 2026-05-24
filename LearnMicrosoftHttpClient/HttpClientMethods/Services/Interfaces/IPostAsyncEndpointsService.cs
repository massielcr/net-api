namespace HttpClientMethods.Services
{
    public interface IPostAsyncEndpointsService
    {
        public Task<bool> CreatePersonalRepositoryAsync(string name, string description, bool isPrivate, bool initialREADME, bool hasDownloads);

        public Task<bool> CreatePersonalRepositoryIssuesAsync(string owner, string repo, string title, string body, int count, CancellationToken cancellationToken);
    }
}
