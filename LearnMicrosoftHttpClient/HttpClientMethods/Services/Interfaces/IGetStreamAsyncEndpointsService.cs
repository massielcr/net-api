namespace HttpClientMethods.Services
{
    public interface IGetStreamAsyncEndpointsService
    {
        public Task<Stream?> DownloadAvatarStringAsync(string username);
        public Task<Stream?> DownloadAvatarUriAsync(string username);
    }
}
