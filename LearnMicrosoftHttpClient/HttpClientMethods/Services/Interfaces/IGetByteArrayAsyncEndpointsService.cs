using HttpClientMethods.Models;

namespace HttpClientMethods.Services
{
    public interface IGetByteArrayAsyncEndpointsService
    {
        public Task<GitHubAvatar?> DownloadLogoStringAsync(string username);
        public Task<GitHubAvatar?> DownloadLogoUriAsync(string username);
    }
}
