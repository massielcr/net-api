using HttpClientMethods.Models;

namespace HttpClientMethods.Services
{
    public interface IGetByteArrayAsyncEndpointsService
    {
        public Task<GitHubAvatar?> GetAvatarStringAsync(string username);
        public Task<GitHubAvatar?> GetAvatarUriAsync(string username);
    }
}
