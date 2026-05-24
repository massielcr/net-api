namespace HttpClientMethods.Services
{
    public interface IPostAsyncEndpointsService
    {
        public Task<bool> CreatePersonalRepositoryAsync(string name, string description, bool isPrivate, bool initialREADME, bool hasDownloads);
    }
}
