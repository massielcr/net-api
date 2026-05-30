using HttpClientMethods.Models;

namespace HttpClientMethods.Services
{
    public interface IGetByteArrayAsyncEndpointsService
    {
        //GetByteArrayAsync(String)	
        public Task<GitHubAvatar?> GetAvatarStringAsync(string username);

        //GetByteArrayAsync(Uri)
        public Task<GitHubAvatar?> GetAvatarUriAsync(string username);
    }
}
