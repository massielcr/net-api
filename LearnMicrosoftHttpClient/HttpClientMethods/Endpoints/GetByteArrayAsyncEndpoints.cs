using HttpClientMethods.Helpers;
using HttpClientMethods.Services;
using Microsoft.AspNetCore.Mvc;

namespace HttpClientMethods.Endpoints
{
    public static class GetByteArrayAsyncEndpoints
    {
        public static void MapGetByteArrayAsyncEndpoints(this WebApplication app)
        {
            app.MapGet("/getbytearrayasyncapi/users/{username}/downloadlogostring", async ([FromRoute] string username, 
                                                                                           [FromQuery] bool overwrite,
                                                                                           [FromServices] IGetByteArrayAsyncEndpointsService getByteArrayAsyncEndpointsService,
                                                                                           [FromServices] IFileService fileService,
                                                                                           [FromServices] IWebHostEnvironment env,
                                                                                           HttpRequest httpRequest) =>
            {

                byte[] data = await getByteArrayAsyncEndpointsService.DownloadLogoStringAsync(username);

                if (data == null || data.Length == 0)
                {
                    return Results.NotFound($"Could not retrieve image data from GitHub for user '{username}'.");
                }


                string webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
                string folderPath = Path.Combine(webRoot, "images");

                bool isSaved = await fileService.SaveFileAsync(data, folderPath, $"{username}_logo.png", overwrite);

                if (!isSaved)
                {
                    return Results.Problem("Image was fetched from GitHub, but saving it to local storage failed");
                }


                string imageUrl = $"{httpRequest.Scheme}://{httpRequest.Host}/images/{username}_logo.png";

                return Results.Ok(new
                {
                    Url = imageUrl,
                    ContentType = "image/png",
                    ContentLength = data.Length
                });

            })
            .WithName("GetByteArrayAsync_DownloadLogoStringAsync")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);


            app.MapGet("/getbytearrayasyncapi/users/{username}/downloadlogouri", async ([FromRoute] string username, 
                                                                                     [FromServices] IGetByteArrayAsyncEndpointsService getByteArrayAsyncEndpointsService) =>
            {
                byte[] data = await getByteArrayAsyncEndpointsService.DownloadLogoUriAsync(username);
                return Results.File(data, "image/png");
            })
            .WithName("GetByteArrayAsync_DownloadLogoUri");
        }
    }
}
