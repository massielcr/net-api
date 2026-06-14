using Microsoft.AspNetCore.Mvc;
using PluralsightKDStreams.Dtos;

namespace PluralsightKDStreams.Interfaces
{
    public interface IStreamerService
    {
        Task<PosterDto> GetPosterServerAsync(string posterId);
        Task<PosterDto?> GetPosterClientAsync(string posterId);

        Task<bool> CreatePosterServerAsync(PosterDto poster);
        Task<string> CreatePosterClientAsync(PosterDto poster);

        Task<MemoryStream?> GetCompressedPosterServerAsync(string posterId);
        Task<PosterDto?> GetCompressedPosterClientAsync(string posterId);

        Task<bool> CreateCompressedPosterServerAsync(PosterDto poster);
        Task<string> CreateCompressedPosterClientAsync(PosterDto poster);

        Task<bool> GetServerTrailerAsync(string trailerId, CancellationToken cancellationToken);
        Task<bool> GetClientTrailerAsync(string trailerId, int httptimeout, CancellationToken cancellationToken);


        Task<MemoryStream?> GetCompressedPosterExceptionDetailsServerAsync(string posterId);
        Task<(PosterDto? poster, ValidationProblemDetails? errors)> GetCompressedPosterExceptionDetailsClientAsync(string posterId);
    }
}
