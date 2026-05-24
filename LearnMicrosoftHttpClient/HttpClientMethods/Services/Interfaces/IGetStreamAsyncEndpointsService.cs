namespace HttpClientMethods.Services
{
    public interface IGetStreamAsyncEndpointsService
    {
        public Task<Stream?> GetAvatarStringAsync(string username);
        public Task<Stream?> GetAvatarUriAsync(string username);
    }
}
