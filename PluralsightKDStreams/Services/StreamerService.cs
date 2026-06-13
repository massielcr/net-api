using Microsoft.AspNetCore.Mvc;
using PluralsightKDStreams.Dtos;
using PluralsightKDStreams.Interfaces;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace PluralsightKDStreams.Services
{
    public class StreamerService(IHttpClientFactory httpClientFactory, ILogger<StreamerService> logger) : IStreamerService
    {
        public async Task<PosterDto> GetPosterServerAsync(string posterId)
        {
            var Random = new Random();

            var generatedData = new byte[1024 * 1024 * 5]; // 5 MB of random data
            Random.NextBytes(generatedData);

            return new PosterDto(posterId, "Generated poster", generatedData);
        }

        public async Task<PosterDto?> GetPosterClientAsync(string posterId)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("Local");

            JsonSerializerOptions options = new(JsonSerializerDefaults.Web);

            Uri serverPosterUri = new($"streamsapi/server/posters/{posterId}", UriKind.Relative);

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, serverPosterUri);

            httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                using HttpResponseMessage respone = await httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead);

                if (!respone.IsSuccessStatusCode)
                {
                    logger.LogError($"Failed to get poster with id {posterId}. Status code: {respone.StatusCode}");
                    return null;
                }

                using Stream responseStream = await respone.Content.ReadAsStreamAsync();

                PosterDto? poster = await JsonSerializer.DeserializeAsync<PosterDto>(responseStream, options);

                return poster;
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, $"An error occurred while fetching poster with id {posterId}.");

            }
            catch (UriFormatException ex)
            {
                logger.LogError(ex, $"An error occurred while fetching poster with id {posterId}.");
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, $"An error occurred while fetching poster with id {posterId}.");
            }
            catch (OperationCanceledException ex)
            {
                logger.LogError(ex, $"An error occurred while fetching poster with id {posterId}.");
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, $"An error occurred while fetching poster with id {posterId}.");
            }

            return null;
        }


        public async Task<bool> CreatePosterServerAsync(PosterDto poster)
        {
            if (poster.Data == null)
            {
                logger.LogError("Invalid poster data provided for creation.");
                return false;
            }

            poster.Id = Guid.NewGuid().ToString();

            return true;
        }

        public async Task<string> CreatePosterClientAsync(PosterDto poster)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("Local");

            JsonSerializerOptions options = new(JsonSerializerDefaults.Web);

            Uri uri = new($"streamsapi/server/posters", UriKind.Relative);

            try
            {
                using MemoryStream memoryStream = new();
                await JsonSerializer.SerializeAsync(memoryStream, poster, options);
                memoryStream.Seek(0, SeekOrigin.Begin);

                using StreamContent streamContent = new(memoryStream);
                streamContent.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

                using HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, uri);
                httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpRequestMessage.Content = streamContent;

                using HttpResponseMessage response = await httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError($"Failed to create poster. Status code: {response.StatusCode}");
                    return string.Empty;
                }

                return response.Headers.Location?.ToString() ?? string.Empty;

            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, "An error occurred while preparing poster data for creation.");
            }
            catch (UriFormatException ex)
            {
                logger.LogError(ex, "An error occurred while preparing poster data for creation.");
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "An error occurred while preparing poster data for creation.");
            }
            catch (OperationCanceledException ex)
            {
                logger.LogError(ex, "An error occurred while preparing poster data for creation.");
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "An error occurred while preparing poster data for creation.");
            }

            return string.Empty;
        }


        public async Task<MemoryStream?> GetCompressedPosterServerAsync(string posterId)
        {
            var Random = new Random();

            byte[] generatedData = new byte[1024 * 1024 * 5]; // 5 MB of random data
            Random.NextBytes(generatedData);

            PosterDto posterDto = new(posterId, "Generated poster", generatedData);
            JsonSerializerOptions options = new(JsonSerializerDefaults.Web);

            using MemoryStream memoryStream = new();
            await JsonSerializer.SerializeAsync(memoryStream, posterDto, options);
            memoryStream.Position = 0;

            MemoryStream compressedStream = new();

            await using (GZipStream gZipStream = new(compressedStream, CompressionMode.Compress, leaveOpen: true))
            {
                await memoryStream.CopyToAsync(gZipStream);
                await gZipStream.FlushAsync();
            }

            compressedStream.Position = 0;

            return compressedStream;
        }

        public async Task<PosterDto?> GetCompressedPosterClientAsync(string posterId)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("Local");

            JsonSerializerOptions options = new(JsonSerializerDefaults.Web);

            Uri uri = new($"streamsapi/server/posters/{posterId}/compression", UriKind.Relative);

            try
            {
                HttpRequestMessage request = new(HttpMethod.Get, uri);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError($"Failed to get compressed poster with id {posterId}. Status code: {response.StatusCode}");
                    return null;
                }

                using Stream responseStream = await response.Content.ReadAsStreamAsync();

                PosterDto? posterDto = await JsonSerializer.DeserializeAsync<PosterDto>(responseStream, options);

                return posterDto;
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, $"An error occurred while fetching compressed poster with id {posterId}.");
            }
            catch (UriFormatException ex)
            {
                logger.LogError(ex, $"An error occurred while fetching compressed poster with id {posterId}.");
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, $"An error occurred while fetching compressed poster with id {posterId}.");
            }
            catch (OperationCanceledException ex)
            {
                logger.LogError(ex, $"An error occurred while fetching compressed poster with id {posterId}.");
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, $"An error occurred while fetching compressed poster with id {posterId}.");
            }

            return null;
        }


        public async Task<bool> CreateCompressedPosterServerAsync(PosterDto poster)
        {
            if (poster.Data == null)
            {
                logger.LogError("Invalid poster data provided for creation.");
                return false;
            }

            poster.Id = Guid.NewGuid().ToString();

            return true;
        }

        public async Task<string> CreateCompressedPosterClientAsync(PosterDto poster)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("Local");

            JsonSerializerOptions options = new(JsonSerializerDefaults.Web);

            Uri uri = new($"streamsapi/server/posters/compression", UriKind.Relative);

            try
            {
                using MemoryStream memoryStream = new();
                await JsonSerializer.SerializeAsync(memoryStream, poster, options);
                memoryStream.Position = 0;

                using MemoryStream compressedMemoryStream = new();

                using (GZipStream gzipStream = new(compressedMemoryStream, CompressionMode.Compress, leaveOpen: true))
                {
                    await memoryStream.CopyToAsync(gzipStream);
                    await gzipStream.FlushAsync();
                }
                ;

                compressedMemoryStream.Position = 0;

                using StreamContent streamContent = new(compressedMemoryStream);
                streamContent.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
                streamContent.Headers.ContentEncoding.Add("gzip");

                using HttpRequestMessage request = new(HttpMethod.Post, uri);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Content = streamContent;

                using HttpResponseMessage response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError($"Failed to create compressed poster. Status code: {response.StatusCode}");
                }

                return response.Headers.Location?.ToString() ?? string.Empty;
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, "An error occurred while creating compressed poster.");
            }
            catch (UriFormatException ex)
            {
                logger.LogError(ex, "An error occurred while creating compressed poster.");
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "An error occurred while creating compressed poster.");
            }
            catch (OperationCanceledException ex)
            {
                logger.LogError(ex, "An error occurred while creating compressed poster.");
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "An error occurred while creating compressed poster.");
            }

            return string.Empty;
        }


        public async Task<bool> GetServerTrailerAsync(string trailerId, CancellationToken cancellationToken)
        {
            DateTime dateTime = DateTime.UtcNow;

            while ((DateTime.UtcNow - dateTime).TotalSeconds < 15)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(1000, cancellationToken);
            }

            return true;
        }

        public async Task<bool> GetClientTrailerAsync(string trailerId, int httptimeout, CancellationToken cancellationToken)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("Local");

            if (httptimeout > 0)
            {
                httpClient.Timeout = TimeSpan.FromSeconds(httptimeout);
            }

            Uri uri = new($"streamsapi/server/trailers/{trailerId}", UriKind.Relative);

            HttpRequestMessage message = new(HttpMethod.Get, uri);

            try
            {
                using HttpResponseMessage response = await httpClient.SendAsync(message, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError($"Failed to get trailer with id {trailerId}. Status code: {response.StatusCode}");
                    return false;
                }

                return true;

            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, $"An error occurred while fetching trailer with id {trailerId}.");
            }
            catch (UriFormatException ex)
            {
                logger.LogError(ex, $"An error occurred while fetching trailer with id {trailerId}.");
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, $"An error occurred while fetching trailer with id {trailerId}.");
            }
            catch (OperationCanceledException ex)
            {
                logger.LogError(ex, $"An error occurred while fetching trailer with id {trailerId}.");
                throw;
            }

            return false;
        }

        public async Task<MemoryStream?> GetCompressedPosterExceptionDetailsServerAsync(string posterId)
        {
            if (posterId == "500error")
            {
                return null;
            }

            Random random = new();
            byte[] data = new byte[1024 * 1024 * 5]; // 5 MB of random data
            random.NextBytes(data);

            PosterDto poster = new()
            {
                Id = posterId,
                Description = $"Generated poster - {DateTime.UtcNow}",
                Data = data
            };

            JsonSerializerOptions options = new(JsonSerializerDefaults.Web);

            MemoryStream memoryStream = new();
            JsonSerializer.Serialize(memoryStream, poster, options);
            memoryStream.Position = 0;

            MemoryStream compressedMemoryStream = new();

            await using (GZipStream gZipStream = new(compressedMemoryStream, CompressionMode.Compress, true))
            {
                await memoryStream.CopyToAsync(gZipStream);
                await gZipStream.FlushAsync();
            }

            compressedMemoryStream.Position = 0;

            return compressedMemoryStream;
        }

        public async Task<(PosterDto? poster, ProblemDetails? errors)> GetCompressedPosterExceptionDetailsClientAsync(string posterId)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("Local");

            JsonSerializerOptions options = new(JsonSerializerDefaults.Web);
            
            Uri uri = new($"streamsapi/server/posters/{posterId}/exception", UriKind.Relative);

            HttpRequestMessage request = new(HttpMethod.Get, uri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            using HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            try
            {
                if (!response.IsSuccessStatusCode)
                {
                    ProblemDetails? problemDetails = new();

                    Stream errorStream = await response.Content.ReadAsStreamAsync();

                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.BadRequest:
                            problemDetails = await JsonSerializer.DeserializeAsync<ProblemDetails>(errorStream, options);
                            break;
                        case HttpStatusCode.InternalServerError:
                            problemDetails = await JsonSerializer.DeserializeAsync<ProblemDetails>(errorStream, options);
                            break;
                    }

                    return (null, problemDetails);
                }

                using Stream responseStream = await response.Content.ReadAsStreamAsync();

                PosterDto? poster = await JsonSerializer.DeserializeAsync<PosterDto>(responseStream, options);

                return (poster, null);
            }
            catch (InvalidOperationException ex) 
            {
                logger.LogError(ex, $"An error occurred while fetching compressed poster with id {posterId}.");
            }
            catch (UriFormatException ex)
            {
                logger.LogError(ex, $"An error occurred while fetching compressed poster with id {posterId}.");
            }
            catch (HttpRequestException ex) 
            {
                logger.LogError(ex, $"An error occurred while fetching compressed poster with id {posterId}.");
            }
            catch(OperationCanceledException ex)
            {
                logger.LogError(ex, $"An error occurred while fetching compressed poster with id {posterId}.");
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, $"An error occurred while fetching compressed poster with id {posterId}.");
            }

            return (null, new ProblemDetails
            {
                Title = "Error occurred while fetching compressed poster",
                Detail = $"Failed to retrieve compressed poster with ID {posterId}"
            });
        }
    }
}