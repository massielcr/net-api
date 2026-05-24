using HttpClientMethods.Dtos;
using HttpClientMethods.Services;
using Microsoft.AspNetCore.Mvc;

namespace HttpClientMethods.Endpoints
{
    public static class PostAsyncEndpoints
    {
        public static void MapPostAsyncEndpoints(this WebApplication app)
        {
            app.MapPost("postasync/repos", async ([FromBody] CreatePersonalRepositoryRequestDto repo,
                                                  [FromServices] IPostAsyncEndpointsService postAsyncEndpointsService) =>
            {

                if (string.IsNullOrWhiteSpace(repo.Name))
                {
                    return Results.BadRequest(new { error = "Repository name cannot be empty." });
                }

                bool success = await postAsyncEndpointsService.CreatePersonalRepositoryAsync(repo.Name, repo.Description, repo.IsPrivate, repo.InitialREADME, repo.HasDownloads);

                if (!success) { return Results.InternalServerError(new { error = "Could not create the repository." }); }

                return Results.StatusCode(StatusCodes.Status201Created);

            })
            .WithName("PostAsync_CreateRepoAsync")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
        }
    }
}
