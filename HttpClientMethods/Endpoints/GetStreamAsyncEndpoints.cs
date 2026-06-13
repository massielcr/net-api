using HttpClientMethods.Dtos;
using HttpClientMethods.Helpers;
using HttpClientMethods.Services;
using Microsoft.AspNetCore.Mvc;

namespace HttpClientMethods.Endpoints
{
    public static class GetStreamAsyncEndpoints
    {
        public static void MapGetStreamAsyncEndpoints(this WebApplication app)
        {
            app.MapGet("/getstreamasyncapi/users/{username}/downloadavatarstring", async ([FromRoute] string username, [FromQuery] bool overwrite,
                                                                                          [FromServices] IGetStreamAsyncEndpointsService getStreamAsyncEndpointsService,
                                                                                          [FromServices] IFileService fileService,
                                                                                          [FromServices] IWebHostEnvironment webHostEnvironment,
                                                                                          HttpRequest httpRequest) =>
            {
                Stream? avatarStream = await getStreamAsyncEndpointsService.GetAvatarStringAsync(username);

                if (avatarStream == null) { return Results.NotFound($"Could not retrieve image data from GitHub for user '{username}'."); }

                string webRoot = webHostEnvironment.WebRootPath ?? Path.Combine(webHostEnvironment.ContentRootPath, "wwwroot");
                string folderPath = Path.Combine(webRoot, "images");
                string fileName = $"{username}_avatar.png";

                bool isSaved = await fileService.SaveFileAsync(folderPath, fileName, avatarStream, overwrite);

                if (!isSaved) { return Results.Problem("Image was fetched from GitHub, but saving it to local storage failed"); }

                string imageUrl = $"{httpRequest.Scheme}://{httpRequest.Host}/images/{fileName}";

                return Results.Ok(new GitHubAvatarResponseDto(imageUrl, "image/png", 0));
            })
            .WithName("GetStreamAsync_GetRepositoryReadmeStringAsync")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

            app.MapGet("/getstreamasyncapi/users/{username}/downloadavataruri", async ([FromRoute] string username, [FromQuery] bool overwrite,
                                                                                       [FromServices] IGetStreamAsyncEndpointsService getStreamAsyncEndpointsService,
                                                                                       [FromServices] IFileService fileService,
                                                                                       [FromServices] IWebHostEnvironment webHostEnvironment,
                                                                                       HttpRequest httpRequest) =>
            {
                Stream? avatarStream = await getStreamAsyncEndpointsService.GetAvatarUriAsync(username);

                if (avatarStream == null) { return Results.NotFound($"Could not retrieve image data from GitHub for user '{username}'."); }

                string webRoot = webHostEnvironment.WebRootPath ?? Path.Combine(webHostEnvironment.ContentRootPath, "wwwroot");
                string folderPath = Path.Combine(webRoot, "images");
                string fileName = $"{username}_avatar.png";

                bool isSaved = await fileService.SaveFileAsync(folderPath, fileName, avatarStream, overwrite);

                if (!isSaved) { return Results.Problem("Image was fetched from GitHub, but saving it to local storage failed"); }

                string imageUrl = $"{httpRequest.Scheme}://{httpRequest.Host}/images/{fileName}";

                return Results.Ok(new GitHubAvatarResponseDto(imageUrl, "image/png", 0));
            })
            .WithName("GetStreamAsync_GetRepositoryReadmeUriAsync")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);
        }
    }
}
