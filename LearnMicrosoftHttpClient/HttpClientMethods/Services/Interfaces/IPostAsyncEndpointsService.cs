namespace HttpClientMethods.Services
{
    public interface IPostAsyncEndpointsService
    {
        public Task<bool> CreatePersonalRepoAsync(string repoName, string description, bool isPrivate, bool initialCommit);
    }
}
