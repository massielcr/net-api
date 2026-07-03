using HttpClientMethods.Dtos;
using HttpClientMethods.Interfaces;
using HttpClientMethods.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HttpClientMethods.Endpoints
{
    public static class SendAsyncEndpoints
    {
        public static void MapSendAsyncEndpoints(this WebApplication app)
        {
            #region Specific HTTP Methods

            //HEAD
            app.MapGet("/sendasync/users/{username}/avatar", async ([FromRoute] string username,
                                                                    [FromServices] ISendAsyncEndpointsService sendEndpointsService) =>
            {
                IEnumerable<string> headers = await sendEndpointsService.GetAvatarHeadersAsync(username);

                return Results.Ok(headers.ToList());

            })
            .WithName("SendAsync_GetAvatarHeadersAsync");

            //OPTIONS
            app.MapGet("/sendasync/users/{username}/options", async ([FromRoute] string username,
                                                                     [FromServices] ISendAsyncEndpointsService sendEndpointsService) =>
            {
                IEnumerable<string> options = await sendEndpointsService.GetUserOptionsAsync(username);

                return Results.Ok(options.ToList());

            })
            .WithName("SendAsync_GetUserOptionsAsync");

            #endregion



            #region BASIC TASKS

            //Task - create issue with cancellation token, timeout, and exception handling
            app.MapPost("/sendasync/{owner}/{repo}/issues", async ([FromRoute] string owner, [FromRoute] string repo,
                                                                  [FromQuery(Name = "cid")] string connectionId,
                                                                  [FromQuery(Name = "to")] int timeout,
                                                                  [FromBody] IssueCreateRequestDto issueRequest,
                                                                  [FromServices] ISendAsyncEndpointsService sendEndpointsService,
                                                                  [FromServices] ICancellationService cancellationService) =>
            {
                if (issueRequest == null || string.IsNullOrEmpty(issueRequest.Title) || string.IsNullOrEmpty(issueRequest.Body))
                {
                    return Results.BadRequest("Invalid issue request.");
                }

                CancellationToken token = cancellationService.GetToken(connectionId, timeout);

                try
                {
                    await sendEndpointsService.CreateIssueAsync(owner, repo, issueRequest.Title, issueRequest.Body, token);
                }
                catch (OperationCanceledException)
                {
                    return Results.StatusCode(499);
                }
                finally
                {
                    cancellationService.Cancel(connectionId);
                }

                return Results.Ok();
            })
             .WithName("SendAsync_CreateIssueAsync_Task")
             .Produces(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status499ClientClosedRequest);


            app.MapGet("/sendasync/{owner}/repos/{repo}", async ([FromRoute] string owner, [FromRoute] string repo,
                                                                [FromQuery(Name = "cid")] string connectionId, [FromQuery(Name ="to")] int timeout,
                                                                [FromServices] ISendAsyncEndpointsService sendAsyncEndpointsService,
                                                                [FromServices] ICancellationService cancellationService) =>
            {
                CancellationToken token = cancellationService.GetToken(connectionId, timeout);

                try
                {
                    JsonElement repoJson = await sendAsyncEndpointsService.GetRepoInfoAsync(owner, repo, token);

                    if (repoJson.ValueKind == JsonValueKind.Undefined)
                    {
                        return Results.Problem("Repository not found or an error occurred.", statusCode: 500);
                    }

                    var repoInfo = new
                    {
                        Name = repoJson.GetProperty("name").GetString(),
                        FullName = repoJson.GetProperty("full_name").GetString(),
                        Description = repoJson.GetProperty("description").GetString(),
                        HtmlUrl = repoJson.GetProperty("html_url").GetString()
                    };

                    return Results.Ok(repoInfo);
                }
                catch (OperationCanceledException)
                {
                    return Results.StatusCode(499);
                }
                finally
                {
                    cancellationService.Cancel(connectionId);
                }               
            })
             .WithName("SendAsync_GetRepoInfoAsync_Task<T>")
             .Produces(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status500InternalServerError)
             .Produces(StatusCodes.Status499ClientClosedRequest);

            #endregion



            #region OTHERS

            app.MapGet("/sendasync/orgs/{orgname}/repos", async ([FromRoute] string orgName,
                                                                 [FromQuery] int page, [FromQuery] int perPage, [FromQuery] int totalPages, [FromQuery(Name = "cid")] string connectionId,
                                                                 [FromServices] ISendAsyncEndpointsService sendEndpointsService, [FromServices] ICancellationService cancellationService) =>
            {
                CancellationToken token = cancellationService.GetToken(connectionId, 30);

                try
                {
                    (IEnumerable<string> repos, double time) = await sendEndpointsService.GetRepositoriesAsync(orgName, page, perPage, totalPages, token);

                    return Results.Ok(new RepositoriesResponseDto
                    {
                        Repos = repos,
                        ExecutionTimeMs = time,
                    });
                }
                finally
                {
                    cancellationService.Cancel(connectionId);
                }

            })
             .WithName("SendAsync_GetRepositoriesAsync");

            app.MapGet("/sendasync/orgs/{orgname}/repos/parallel", async ([FromRoute] string orgName,
                                                                          [FromQuery] int page, [FromQuery] int perPage, [FromQuery] int totalPages, [FromQuery(Name = "cid")] string connectionId,
                                                                          [FromServices] ISendAsyncEndpointsService sendEndpointsService, [FromServices] ICancellationService cancellationService) =>
            {
                CancellationToken token = cancellationService.GetToken(connectionId, 30);

                try
                {
                    (IEnumerable<string> repos, double time) = await sendEndpointsService.GetRepositoriesParallelAsync(orgName, page, perPage, totalPages, token);

                    return Results.Ok(new RepositoriesResponseDto
                    {
                        Repos = repos,
                        ExecutionTimeMs = time,
                    });
                }
                finally
                {
                    cancellationService.Cancel(connectionId);
                }

            })
             .WithName("SendAsync_GetRepositoriesParallelAsync");

            #endregion
        }
    }
}
