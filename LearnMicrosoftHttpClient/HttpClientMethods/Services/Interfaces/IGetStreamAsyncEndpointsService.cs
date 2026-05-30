namespace HttpClientMethods.Services
{
    public interface IGetStreamAsyncEndpointsService
    {
        //GetStreamAsync(String)
        public Task<Stream?> GetAvatarStringAsync(string username);

        //GetStreamAsync(Uri)
        public Task<Stream?> GetAvatarUriAsync(string username);
    }
}
