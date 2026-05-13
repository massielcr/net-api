using HttpClientMethods.Dtos;
using HttpClientMethods.Services;
using Microsoft.AspNetCore.Mvc;

namespace HttpClientMethods.Endpoints
{
    public static class PostAsyncEndpoints
    {
        public static void MapPostAsyncEndpoints(this WebApplication app)
        {
            app.MapPost("postasync/repos", async ([FromBody] CreatePersonalRepoRequest repo, 
                                                   [FromServices] IPostAsyncEndpointsService postAsyncEndpointsService) => {

                if (string.IsNullOrWhiteSpace(repo.Name))
                {
                    return Results.BadRequest(new { error = "Repository name cannot be empty." });
                }

                try
                {
                    bool success = await postAsyncEndpointsService.CreatePersonalRepoAsync(repo.Name, repo.Description, repo.IsPrivate, repo.InitialCommit);

                    if (success) { return Results.StatusCode(StatusCodes.Status201Created); }

                    return Results.BadRequest(new { error = "Could not create the repository." });
                }
                catch (Exception ex)
                {
                    return Results.Problem("An error ocurred");
                }

            }).WithName("PostAsync_CreateRepoAsync");
        }
    }
}
