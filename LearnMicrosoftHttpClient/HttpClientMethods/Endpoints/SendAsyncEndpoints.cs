using HttpClientMethods.Dtos;
using HttpClientMethods.Helpers;
using HttpClientMethods.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace HttpClientMethods.Endpoints
{
    public static class SendAsyncEndpoints
    {
        public static void MapSendAsyncEndpoints(this WebApplication app)
        {
            app.MapGet("/sendasync/users/{username}/avatar", async ([FromRoute] string username,
                                                                    [FromServices] ISendAsyncEndpointsService sendEndpointsService) =>
            {

                IEnumerable<string> headers = await sendEndpointsService.GetAvatarHeadersAsync(username);

                return Results.Ok(headers.ToList());

            })
            .WithName("SendAsync_GetAvatarHeadersAsync");


            app.MapGet("/sendasync/users/{username}/options", async ([FromRoute] string username,
                                                                     [FromServices] ISendAsyncEndpointsService sendEndpointsService) =>
            {

                IEnumerable<string> options = await sendEndpointsService.GetUserOptionsAsync(username);

                return Results.Ok(options.ToList());

            })
            .WithName("SendAsync_GetUserOptionsAsync");


            app.MapGet("/sendasync/orgs/{orgname}/repos", async ([FromRoute] string orgName,
                                                                 [FromQuery] int page, [FromQuery] int perPage, [FromQuery] int totalPages, [FromQuery(Name = "cid")] string connectionId,
                                                                 [FromServices] ISendAsyncEndpointsService sendEndpointsService, [FromServices] CancellationManager cancellationManager) =>
            {
                CancellationToken token = cancellationManager.GetToken(connectionId, 30);

                try
                {
                    (IEnumerable<string> repos, double time) = await sendEndpointsService.GetRepositoriesAsync(orgName, page, perPage, totalPages, token);

                    if (repos.Any())
                    {
                        return Results.Ok(new
                        {
                            repos,
                            time,
                        });
                    }
                    else
                    {
                        return Results.Problem("No repos");
                    }
                }
                finally
                {
                    cancellationManager.Cancel(connectionId);
                }

            })
             .WithName("SendAsync_GetRepositoriesAsync");

            app.MapGet("/sendasync/orgs/{orgname}/repos/parallel", async ([FromRoute] string orgName,
                                                                          [FromQuery] int page, [FromQuery] int perPage, [FromQuery] int totalPages, [FromQuery(Name = "cid")] string connectionId,
                                                                          [FromServices] ISendAsyncEndpointsService sendEndpointsService, [FromServices] CancellationManager cancellationManager) =>
            {
                CancellationToken token = cancellationManager.GetToken(connectionId, 30);

                try
                {
                    (IEnumerable<string> repos, double time) = await sendEndpointsService.GetRepositoriesParallelAsync(orgName, page, perPage, totalPages, token);

                    if (repos.Any())
                    {
                        return Results.Ok(new
                        {
                            repos,
                            time,
                        });
                    }
                    else
                    {
                        return Results.Problem("No repos");
                    }
                }
                finally
                {
                    cancellationManager.Cancel(connectionId);
                }

            })
             .WithName("SendAsync_GetRepositoriesParallelAsync");



            #region Streams

            //GET SERVER
            app.MapGet("/sendasync/server/posters/{posterId}", async ([FromRoute] string posterId,
                                                               [FromServices] ISendAsyncEndpointsService sendEndpointsService) =>
            {
                if (string.IsNullOrWhiteSpace(posterId))
                {
                    return Results.BadRequest("Invalid poster ID.");
                }

                PosterDto posterData = await sendEndpointsService.GetPosterServerAsync(posterId);

                if (posterData == null)
                {
                    return Results.NotFound("Poster not found.");                    
                }

                return Results.Ok(posterData);
            })
            .WithName("SendAsync_Server_GetPoster")
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status200OK);


            //GET CLIENT
            app.MapGet("/sendasync/client/posters/{posterId}", async ([FromRoute] string posterId,
                                                                       [FromServices] ISendAsyncEndpointsService sendEndpointsService) =>
            {
                if (string.IsNullOrWhiteSpace(posterId))
                {
                    return Results.BadRequest("Invalid poster ID.");
                }
                Stopwatch stopwatch = new();
                stopwatch.Start();

                PosterDto? poster = await sendEndpointsService.GetPosterClientAsync(posterId);

                stopwatch.Stop();
                Console.WriteLine($"Time taken to fetch compressed poster: {stopwatch.ElapsedMilliseconds} ms");

                if (poster == null)
                {
                    return Results.NotFound("Poster not found.");
                }

                return Results.Ok(poster);
            })
            .WithName("SendAsync_Client_GetPoster")
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status200OK);


            //POST SERVER
            app.MapPost("/sendasync/server/posters", async ([FromBody] PosterDto poster,
                                                            [FromServices] ISendAsyncEndpointsService sendEndpointsService) =>
            {
                if (string.IsNullOrWhiteSpace(poster.Description))
                {
                    return Results.BadRequest("Invalid poster.");
                }

                bool created = await sendEndpointsService.CreatePosterServerAsync(poster);

                if (!created)
                {
                    return Results.Problem("Failed to create poster.");
                }

                return Results.Created($"/sendasync/client/posters/{poster.Id}", null);
            })
            .WithName("SendAsync_Server_CreatePoster")
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status200OK);


            //POST CLIENT
            app.MapPost("/sendasync/client/posters", async ([FromBody] PosterDto poster,
                                                            [FromServices] ISendAsyncEndpointsService sendEndpointsService) =>
            {
                Random random = new();
                byte[] data = new byte[10 * 1024 * 1024]; // 10 MB
                random.NextBytes(data);

                poster.Data = data;

                string posterUri = await sendEndpointsService.CreatePosterClientAsync(poster);

                if (string.IsNullOrEmpty(posterUri))
                {
                    return Results.Problem("Failed to create poster.");
                }

                return Results.Created(posterUri, null);
            })
            .WithName("SendAsync_Client_CreatePoster")
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status200OK);

            //COMPRESSION SERVER
            app.MapGet("/sendasync/server/posters/{posterId}/compression", async ([FromRoute] string posterId,
                                                                                  [FromServices] ISendAsyncEndpointsService sendEndpointsService) =>
            {
                if (string.IsNullOrEmpty(posterId))
                {
                    return Results.BadRequest("Invalid poster ID.");
                }

                PosterDto? poster = await sendEndpointsService.GetCompressedPosterServerAsync(posterId);

                if (poster == null)
                {
                    return Results.NotFound("Poster not found.");
                }
                return Results.Ok(poster);
            })
            .WithName("SendAsync_Server_GetPosterGZip")
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status200OK);

            //COMPRESSION CLIENT
            app.MapGet("/sendasync/client/posters/{posterId}/compression", async ([FromRoute] string posterId,
                                                                                  [FromServices] ISendAsyncEndpointsService sendEndpointsService) =>
            {
                if (string.IsNullOrEmpty(posterId))
                {
                    return Results.BadRequest("Invalid poster ID.");
                }

                Stopwatch stopwatch = new();
                stopwatch.Start();

                PosterDto? poster = await sendEndpointsService.GetCompressedPosterClientAsync(posterId);

                stopwatch.Stop();
                Console.WriteLine($"Time taken to fetch compressed poster: {stopwatch.ElapsedMilliseconds} ms");

                if (poster == null)
                {
                    return Results.NotFound("Poster not found.");
                }

                return Results.Ok(poster);
            })
            .WithName("SendAsync_Client_GetPosterGZip")
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status200OK);

            #endregion
        }
    }
}
