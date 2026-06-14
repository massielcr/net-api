using Microsoft.AspNetCore.Mvc;
using PluralsightKDStreams.Dtos;
using PluralsightKDStreams.Interfaces;
using PluralsightKDStreams.Services;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PluralsightKDStreams.Endpoints
{
    public static class StreamerEndpoints
    {
        public static void MapStreamerEndpoints(this WebApplication app)
        {
            var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("SendAsyncEndpoints");

            //GET SERVER
            app.MapGet("/streamsapi/server/posters/{posterId}", async ([FromRoute] string posterId,
                                                                      [FromServices] IStreamerService streamerService) =>
            {
                if (string.IsNullOrWhiteSpace(posterId))
                {
                    return Results.BadRequest("Invalid poster ID.");
                }

                PosterDto posterData = await streamerService.GetPosterServerAsync(posterId);

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
            app.MapGet("/streamsapi/client/posters/{posterId}", async ([FromRoute] string posterId,
                                                                       [FromServices] IStreamerService streamerService) =>
            {
                if (string.IsNullOrWhiteSpace(posterId))
                {
                    return Results.BadRequest("Invalid poster ID.");
                }

                Stopwatch stopwatch = new();
                stopwatch.Start();

                PosterDto? poster = await streamerService.GetPosterClientAsync(posterId);

                stopwatch.Stop();
                logger.LogInformation("Time taken to fetch poster: {ElapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);

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
            app.MapPost("/streamsapi/server/posters", async ([FromBody] PosterDto? poster,
                                                            [FromServices] IStreamerService streamerService) =>
            {
                if (poster == null)
                {
                    return Results.BadRequest("Poster data is required.");
                }

                if (string.IsNullOrWhiteSpace(poster.Description))
                {
                    return Results.BadRequest("Invalid poster - description is required.");
                }

                bool created = await streamerService.CreatePosterServerAsync(poster);

                if (!created)
                {
                    return Results.Problem("Failed to create poster.");
                }

                return Results.Created($"/streamsapi/server/posters/{poster.Id}", null);
            })
            .WithName("SendAsync_Server_CreatePoster")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

            //POST CLIENT
            app.MapPost("/streamsapi/client/posters", async ([FromBody] PosterDto? poster,
                                                            [FromServices] IStreamerService streamerService) =>
            {
                if (poster == null)
                {
                    return Results.BadRequest("Poster data is required.");
                }

                Random random = new();
                byte[] data = new byte[10 * 1024 * 1024]; // 10 MB
                random.NextBytes(data);

                poster.Data = data;

                Stopwatch stopwatch = new();
                stopwatch.Start();

                string posterUri = await streamerService.CreatePosterClientAsync(poster);

                stopwatch.Stop();
                logger.LogInformation("Time taken to create poster: {ElapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);

                if (string.IsNullOrEmpty(posterUri))
                {
                    return Results.Problem("Failed to create poster.");
                }

                return Results.Created(posterUri, null);
            })
            .WithName("SendAsync_Client_CreatePoster")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);



            //COMPRESSION SERVER
            app.MapGet("/streamsapi/server/posters/{posterId}/compression", async ([FromRoute] string posterId,
                                                                                  [FromServices] IStreamerService streamerService,
                                                                                  HttpContext httpContext) =>
            {
                if (string.IsNullOrEmpty(posterId))
                {
                    return Results.BadRequest("Invalid poster ID.");
                }

                MemoryStream? posterStream = await streamerService.GetCompressedPosterServerAsync(posterId);

                if (posterStream == null)
                {
                    return Results.NotFound("Poster not found.");
                }

                httpContext.Response.Headers.ContentEncoding = "gzip";

                return Results.File(
                        fileStream: posterStream,
                        contentType: "application/json"
                    );
            })
            .WithName("SendAsync_Server_GetPosterGZip")
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status200OK, typeof(PosterDto), "application/json");

            //COMPRESSION CLIENT
            app.MapGet("/streamsapi/client/posters/{posterId}/compression", async ([FromRoute] string posterId,
                                                                                  [FromServices] IStreamerService streamerService) =>
            {
                if (string.IsNullOrEmpty(posterId))
                {
                    return Results.BadRequest("Invalid poster ID.");
                }

                Stopwatch stopwatch = new();
                stopwatch.Start();

                PosterDto? poster = await streamerService.GetCompressedPosterClientAsync(posterId);

                stopwatch.Stop();
                logger.LogInformation("Time taken to fetch compressed poster: {ElapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);

                if (poster == null)
                {
                    return Results.NotFound("Poster not found.");
                }

                return Results.Ok(poster);
            })
            .WithName("SendAsync_Client_GetPosterGZip")
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status200OK, typeof(PosterDto));

            //COMPRESSION CREATE SERVER
            app.MapPost("/streamsapi/server/posters/compression", async ([FromBody] PosterDto? poster,
                                                                        [FromServices] IStreamerService streamerService) =>
            {
                if (poster == null)
                {
                    return Results.BadRequest("Invalid poster - poster is required.");
                }
                if (string.IsNullOrWhiteSpace(poster.Description))
                {
                    return Results.BadRequest("Invalid poster - description is required.");
                }

                bool created = await streamerService.CreateCompressedPosterServerAsync(poster);

                if (!created)
                {
                    return Results.Problem("Failed to create poster.");
                }

                return Results.Created($"/streamsapi/server/posters/{poster.Id}/compression", null);
            })
            .WithName("SendAsync_Server_CreatePosterGZip")
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status201Created);

            //COMPRESSION CREATE CLIENT
            app.MapPost("/streamsapi/client/posters/compression", async ([FromBody] PosterDto? poster,
                                                                        [FromServices] IStreamerService streamerService) =>
            {
                if (poster == null)
                {
                    return Results.BadRequest("Invalid poster - poster is required.");
                }

                Random random = new();
                byte[] data = new byte[10 * 1024 * 1024]; // 10 MB
                random.NextBytes(data);

                poster.Data = data;

                Stopwatch stopwatch = new();
                stopwatch.Start();

                string posterUri = await streamerService.CreateCompressedPosterClientAsync(poster);

                stopwatch.Stop();
                logger.LogInformation("Time taken to create compressed poster: {ElapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);

                if (string.IsNullOrWhiteSpace(posterUri))
                {
                    return Results.Problem("Failed to create poster.");
                }

                return Results.Created(posterUri, null);
            })
            .WithName("SendAsync_Client_CreatePosterGZip")
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status201Created);



            //CANCELLATION SERVER
            app.MapGet("/streamsapi/server/trailers/{trailerId}", async ([FromRoute] string trailerId,
                                                                       [FromServices] IStreamerService streamerService,
                                                                       CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(trailerId))
                {
                    return Results.BadRequest("Invalid trailer ID.");
                }

                try
                {
                    bool success = await streamerService.GetServerTrailerAsync(trailerId, cancellationToken);

                    if (!success)
                    {
                        return Results.Problem("Failed to retrieve trailer.");
                    }

                    return Results.Ok();

                }
                catch (OperationCanceledException)
                {
                    return Results.StatusCode(StatusCodes.Status499ClientClosedRequest);
                }
            })
            .WithName("SendAsync_GetServerTrailerAsync")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status499ClientClosedRequest)
            .Produces(StatusCodes.Status500InternalServerError);

            //CANCELLATION CLIENT
            app.MapGet("/streamsapi/client/trailers/{trailerId}", async ([FromRoute] string trailerId,
                                                                        [FromQuery(Name = "cid")] string connectionId, [FromQuery] int timeout, [FromQuery] int httptimeout,
                                                                        [FromServices] IStreamerService streamerService,
                                                                        [FromServices] ICancellationService CancellationService) =>

            {
                if (string.IsNullOrWhiteSpace(trailerId))
                {
                    return Results.BadRequest("Invalid trailer ID.");
                }

                CancellationToken token = CancellationService.GetToken(connectionId, timeout);

                try
                {
                    bool success = await streamerService.GetClientTrailerAsync(trailerId, httptimeout, token);

                    if (!success)
                    {
                        return Results.Problem("Failed to retrieve trailer.");
                    }
                }
                catch (OperationCanceledException)
                {
                    return Results.StatusCode(StatusCodes.Status499ClientClosedRequest);
                }

                return Results.Ok();
            })
            .WithName("SendAsync_GetClientTrailerAsync")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status499ClientClosedRequest)
            .Produces(StatusCodes.Status500InternalServerError);



            //EXCEPTION HANDLING SERVER
            app.MapGet("/streamsapi/server/posters/{posterId}/exception", async ([FromRoute] string posterId,
                                                                                  [FromServices] IStreamerService streamerService,
                                                                                  HttpContext httpContext) =>
            {
                if (string.IsNullOrWhiteSpace(posterId) || posterId == "400error")
                {
                    return Results.Problem(
                                detail: $"Requested poster ID '{posterId}' is invalid.",
                                statusCode: StatusCodes.Status400BadRequest,
                                title: "Bad Request"
                           );
                }

                MemoryStream? poster = await streamerService.GetCompressedPosterExceptionDetailsServerAsync(posterId);

                if (poster == null)
                {
                    return Results.ValidationProblem(
                                    detail: $"Failed to retrieve poster ID '{posterId}'.",
                                    statusCode: StatusCodes.Status500InternalServerError,
                                    title: "Internal Server Error",
                                    errors: new Dictionary<string, string[]> { { $"PosterId-{posterId} ", new[] { $"Poster with ID '{posterId}' doesn't have any data." } } }                                 
                                );
                }

                httpContext.Response.Headers.ContentEncoding = "gzip";

                return Results.File(
                                    fileStream: poster,
                                    contentType: "application/json"
                                );
            })
             .WithName("SendAsync_Server_GetCompressedPosterExceptionDetails")
             .Produces(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status400BadRequest);


            //EXCEPTION HANDLING CLIENT
            app.MapGet("/streamsapi/client/posters/{posterId}/exception", async ([FromRoute] string posterId,
                                                                                 [FromServices] IStreamerService streamerService) =>
            {
                if (string.IsNullOrWhiteSpace(posterId))
                {
                    return Results.BadRequest("Invalid poster ID");
                }

                (PosterDto? poster, ValidationProblemDetails? errors) = await streamerService.GetCompressedPosterExceptionDetailsClientAsync(posterId);

                if (poster == null && errors != null)
                {
                    return Results.Problem(errors);
                }

                return Results.Ok(poster);
            })
            .WithName("SendAsync_Client_GetCompressedPosterExceptionDetails")
            .Produces(StatusCodes.Status200OK, typeof(PosterDto), "application/json")
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);



            //POLLY SERVER


            //POLLY CLIENT
        }
    }
}
