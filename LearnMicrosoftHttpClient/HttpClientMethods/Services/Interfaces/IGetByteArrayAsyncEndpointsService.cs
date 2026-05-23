using HttpClientMethods.Models;

namespace HttpClientMethods.Services
{
    public interface IGetByteArrayAsyncEndpointsService
    {
        public Task<GitHubAvatar?> DownloadAvatarStringAsync(string username);
        public Task<GitHubAvatar?> DownloadAvatarUriAsync(string username);
    }
}
