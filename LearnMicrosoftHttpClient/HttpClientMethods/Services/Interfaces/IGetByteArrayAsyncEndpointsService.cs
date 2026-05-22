namespace HttpClientMethods.Services
{
    public interface IGetByteArrayAsyncEndpointsService
    {
        public Task<byte[]> DownloadLogoStringAsync(string username);
        public Task<byte[]> DownloadLogoUriAsync(string username);
    }
}
