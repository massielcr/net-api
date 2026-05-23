using HttpClientMethods.Dtos;
using HttpClientMethods.Helpers;
using HttpClientMethods.Models;
using HttpClientMethods.Services;
using Microsoft.AspNetCore.Mvc;

namespace HttpClientMethods.Endpoints
{
    public static class GetByteArrayAsyncEndpoints
    {
        public static void MapGetByteArrayAsyncEndpoints(this WebApplication app)
        {
            app.MapGet("/getbytearrayasyncapi/users/{username}/downloadavatarstring", async ([FromRoute] string username, 
                                                                                           [FromQuery] bool overwrite,
                                                                                           [FromServices] IGetByteArrayAsyncEndpointsService getByteArrayAsyncEndpointsService,
                                                                                           [FromServices] IFileService fileService,
                                                                                           [FromServices] IWebHostEnvironment webHostEnvironment,
                                                                                           HttpRequest httpRequest) =>
            {

                GitHubAvatar? avatar = await getByteArrayAsyncEndpointsService.DownloadAvatarStringAsync(username);

                if (avatar == null || avatar.ContentLength == 0)
                {
                    return Results.NotFound($"Could not retrieve image data from GitHub for user '{username}'.");
                }


                string webRoot = webHostEnvironment.WebRootPath ?? Path.Combine(webHostEnvironment.ContentRootPath, "wwwroot");
                string folderPath = Path.Combine(webRoot, "images");

                bool isSaved = await fileService.SaveFileAsync(avatar!.Data!, folderPath, $"{username}_avatar.png", overwrite);

                if (!isSaved)
                {
                    return Results.Problem("Image was fetched from GitHub, but saving it to local storage failed");
                }


                string imageUrl = $"{httpRequest.Scheme}://{httpRequest.Host}/images/{username}_avatar.png";

                return Results.Ok(new GitHubAvatarResponseDto(imageUrl, avatar.ContentType, avatar.ContentLength));

            })
            .WithName("GetByteArrayAsync_DownloadAvatarStringAsync")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);


            app.MapGet("/getbytearrayasyncapi/users/{username}/downloadavataruri", async ([FromRoute] string username,
                                                                                        [FromQuery] bool overwrite,
                                                                                        [FromServices] IGetByteArrayAsyncEndpointsService getByteArrayAsyncEndpointsService,
                                                                                        [FromServices] IFileService fileService,
                                                                                        [FromServices] IWebHostEnvironment webHostEnvironment,
                                                                                        HttpRequest httpRequest) =>
            {

                GitHubAvatar? avatar = await getByteArrayAsyncEndpointsService.DownloadAvatarUriAsync(username);

                if (avatar == null || avatar.ContentLength == 0)
                {
                    return Results.NotFound($"Could not retrieve image data from GitHub for user '{username}'.");
                }

                string webRoot = webHostEnvironment.WebRootPath ?? Path.Combine(webHostEnvironment.ContentRootPath, "wwwroot");
                string folderPath = Path.Combine(webRoot, "images");

                bool isSaved = await fileService.SaveFileAsync(avatar!.Data!, folderPath, $"{username}_avatar.png", overwrite);

                if (!isSaved)
                {
                    return Results.Problem("Image was fetched from GitHub, but saving it to local storage failed");
                }

                string imageUrl = $"{httpRequest.Scheme}://{httpRequest.Host}/images/{username}_avatar.png";

                return Results.Ok(new GitHubAvatarResponseDto(imageUrl, avatar.ContentType, avatar.ContentLength));
            })
            .WithName("GetByteArrayAsync_DownloadAvatarUriAsync")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);
        }
    }
}
