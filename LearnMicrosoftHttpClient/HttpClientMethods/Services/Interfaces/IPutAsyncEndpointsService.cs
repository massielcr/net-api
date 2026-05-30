namespace HttpClientMethods.Services
{
    internal interface IPutAsyncEndpointsService
    {
        Task<bool> ReplaceRepositoryTopicsAsync(string owner, string repo, List<string> names);
    }
}