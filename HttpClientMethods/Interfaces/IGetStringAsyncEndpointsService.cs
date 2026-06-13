namespace HttpClientMethods.Interfaces
{
    public interface IGetStringAsyncEndpointsService
    {
        //GetStringAsync(String)
        Task<string?> GetRepositoryReadmeStringAsync(string owner, string repo);


        //GetStringAsync(Uri)
        Task<string?> GetRepositoryReadmeUriAsync(string owner, string repo);
    }
}
