using HttpClientMethods.Services;
using Microsoft.AspNetCore.Mvc;

namespace HttpClientMethods.Endpoints
{
    public static class GetStringAsyncEndpoints
    {
        public static void MapGetStringAsyncEndpoints(this WebApplication app)
        {
            app.MapGet("/getstringasyncapi/{owner}/repos/{repo}/readmestring", async ([FromRoute] string owner, [FromRoute] string repo,
                                                                                      [FromServices] IGetStringAsyncEndpointsService getStringAsyncEndpointsService ) =>
            {

                string? result = await getStringAsyncEndpointsService.GetRepositoryReadmeStringAsync(owner, repo);


                if (string.IsNullOrWhiteSpace(result)) { return Results.NotFound(); }


                return Results.Content(result, "text/markdown");


            })
            .WithName("GetStringAsync_GetRepositoryReadmeStringAsync")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);


            app.MapGet("/getstringasyncapi/{owner}/repos/{repo}/readmeuri", async ([FromRoute] string owner, [FromRoute] string repo,
                                                                                   [FromServices] IGetStringAsyncEndpointsService getStringAsyncEndpointsService ) =>
            {
                string? result = await getStringAsyncEndpointsService.GetRepositoryReadmeUriAsync(owner, repo);


                if (string.IsNullOrWhiteSpace(result)) { return Results.NotFound(); }


                return Results.Content(result, "text/markdown");


            })
            .WithName("GetStringAsync_GetRepositoryReadmeUriAsync")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
