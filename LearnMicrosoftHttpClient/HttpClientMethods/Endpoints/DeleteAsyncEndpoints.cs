using HttpClientMethods.Helpers;
using HttpClientMethods.Services;
using Microsoft.AspNetCore.Mvc;

namespace HttpClientMethods.Endpoints
{
    public static class DeleteAsyncEndpoints
    {
        public static void MapDeleteAsyncEndpoints(this WebApplication app)
        {
            app.MapDelete("/deleteasync/repos/{owner}/{repo}/issues/{id}/unlock", async ([FromRoute] string owner, [FromRoute] string repo, [FromRoute] int id,
                                                                                         [FromServices] IDeleteAsyncEndpointsService deleteService) =>
            {
                if (id <= 0)
                {
                    return Results.BadRequest("Invalid issue ID.");
                }

                bool success = await deleteService.UnlockIssueAsync(owner, repo, id);

                if(!success) 
                {
                    return Results.NotFound("Issue not found.");
                }

                return Results.Ok();
            })
            .WithName("DeleteAsync_UnlockIssueAsync")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

            app.MapPost("/deleteasync/repos/{owner}/{repo}/issues/unlockbatch", async ([FromRoute] string owner, [FromRoute] string repo,
                                                                                       [FromQuery(Name = "cid")] string connectionId,
                                                                                       [FromBody] List<int> ids,
                                                                                       [FromServices] IDeleteAsyncEndpointsService deleteService,
                                                                                       [FromServices] CancellationManager cancellationManager) =>
            {
                if (ids.Any(id => id <= 0))
                {
                    return Results.BadRequest("Invalid issue IDs.");
                }

                CancellationToken cancellationToken = cancellationManager.GetToken(connectionId, 30);

                try
                {
                    bool success = await deleteService.UnlockIssuesAsync(owner, repo, ids, cancellationToken);

                    if (!success)
                    {
                        return Results.NotFound("Issue not found.");
                    }
                }
                catch (OperationCanceledException)
                {
                    return Results.StatusCode(StatusCodes.Status499ClientClosedRequest);
                }                  

                return Results.Ok();
            })
            .WithName("DeleteAsync_UnlockIssuesAsync")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status499ClientClosedRequest);


            app.MapDelete("/deleteasync/repos/{owner}/{repo}/issues/{id}/unlockuri", async ([FromRoute] string owner, [FromRoute] string repo, [FromRoute] int id,
                                                                                            [FromServices] IDeleteAsyncEndpointsService deleteService) =>
            {
                if (id <= 0)
                {
                    return Results.BadRequest("Invalid issue ID.");
                }

                bool success = await deleteService.UnlockIssueUriAsync(owner, repo, id);

                if (!success)
                {
                    return Results.NotFound("Issue not found.");
                }

                return Results.Ok();
            })
            .WithName("DeleteAsync_UnlockIssueUriAsync")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);


            app.MapPost("/deleteasync/repos/{owner}/{repo}/issues/unlockbatchstream", async ([FromRoute] string owner, [FromRoute] string repo,
                                                                                             [FromQuery(Name = "cid")] string connectionId,
                                                                                             [FromBody] List<int> ids,
                                                                                             [FromServices] IDeleteAsyncEndpointsService deleteService,
                                                                                             [FromServices] CancellationManager cancellationManager) =>
            {
                if (ids.Any(id => id <= 0))
                {
                    return Results.BadRequest("Invalid issue IDs.");
                }

                CancellationToken cancellationToken = cancellationManager.GetToken(connectionId, 30);

                try
                {
                    await foreach (int id in deleteService.UnlockIssuesStreamAsync(owner, repo, ids, cancellationToken))
                    {
                        // Process each unlocked issue ID as needed
                        Console.WriteLine($"Unlocked issue ID: {id}");
                    }

                }
                catch (OperationCanceledException)
                {
                    return Results.StatusCode(StatusCodes.Status499ClientClosedRequest);
                }


                return Results.Empty;
            })
            .WithName("DeleteAsync_UnlockIssuesStreamAsync")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status499ClientClosedRequest);
        }
    }
}
