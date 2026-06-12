using HttpClientMethods.Dtos;

namespace HttpClientMethods.Services
{
    public interface ISendAsyncEndpointsService
    {
        #region Specific HTTP Methods

        Task<IEnumerable<string>> GetAvatarHeadersAsync(string username);

        Task<IEnumerable<string>> GetUserOptionsAsync(string username);

        #endregion


        #region Streams

        Task<PosterDto> GetPosterServerAsync(string posterId);
        Task<PosterDto?> GetPosterClientAsync(string posterId);

        Task<bool> CreatePosterServerAsync(PosterDto poster);
        Task<string> CreatePosterClientAsync(PosterDto poster);

        Task<MemoryStream?> GetCompressedPosterServerAsync(string posterId);
        Task<PosterDto?> GetCompressedPosterClientAsync(string posterId);

        Task<bool> CreateCompressedPosterServerAsync(PosterDto poster);
        Task<string> CreateCompressedPosterClientAsync(PosterDto poster);

        #endregion


        Task<(IEnumerable<string> Repos, double time)> GetRepositoriesAsync(string orgName, int page, int perPage, int totalPages, CancellationToken cancellationToken);

        Task<(IEnumerable<string> Repos, double time)> GetRepositoriesParallelAsync(string orgName, int page, int perPage, int totalPages, CancellationToken cancellationToken);
    }
}
