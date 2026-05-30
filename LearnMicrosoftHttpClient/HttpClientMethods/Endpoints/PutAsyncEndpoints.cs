using HttpClientMethods.Dtos;
using HttpClientMethods.Services;
using Microsoft.AspNetCore.Mvc;

namespace HttpClientMethods.Endpoints
{
    public static class PutAsyncEndPoints
    {
        public static void MapPutAsyncEndpoints(this WebApplication app)
        {
            app.MapPut("putasync/repos/{owner}/{repo}/topics", async ([FromRoute] string owner, [FromRoute] string repo,
                                                                      [FromBody] UpdateRepoTopicsRequestDto topics,
                                                                      [FromServices] IPutAsyncEndpointsService putAsyncEndpointsService) =>
            {
                if (topics == null || topics.Names == null || topics.Names.Count == 0)
                {
                    return Results.BadRequest(new { error = "Topics list cannot be empty." });
                }

                bool success = await putAsyncEndpointsService.ReplaceRepositoryTopicsAsync(owner, repo, topics.Names);

                if (!success) { return Results.InternalServerError(new { error = "Could not replace the repository topics." }); }

                return Results.Ok();
            })
            .WithName("PutAsync_ReplaceRepositoryTopicsAsync")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
        }
    }
}
