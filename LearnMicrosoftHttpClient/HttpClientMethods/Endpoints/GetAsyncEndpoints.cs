using HttpClientMethods.Dtos;
using HttpClientMethods.Helpers;
using HttpClientMethods.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace HttpClientMethods.Endpoints
{
    public static class GetAsyncEndpoints
    {
        private static readonly Regex _cleanMessageRegex = new(@"[^a-zA-Z0-9\s]", RegexOptions.Compiled);

        public static void MapGetAsyncEndpoints(this WebApplication app)
        {
            #region Repositories

            app.MapGet("/getasyncapi/orgs/{orgname}/repos/count", async ([FromRoute] string orgname,
                                                                         [FromServices] IGetAsyncEndpointsService getEndpointsService) =>
            {
                int? reposCount = await getEndpointsService.GetRepositoriesCountAsync(orgname);

                if (!reposCount.HasValue)
                {
                    return Results.Problem("An error ocurred");
                }

                return Results.Ok(reposCount.Value);

            })
            .WithName("GetAsync_GetRepositoriesCountAsync")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);


            app.MapGet("/getasyncapi/orgs/{orgname}/repos", async ([FromRoute] string orgName, 
                                                       [FromQuery] int page, [FromQuery] int perPage, [FromQuery] int totalPages, [FromQuery(Name = "cid")] string connectionId, 
                                                       [FromServices] IGetAsyncEndpointsService getEndpointsService, [FromServices] CancellationManager cancellationManager) =>
            {
                CancellationToken token = cancellationManager.GetToken(connectionId, 30);

                try
                {
                    IEnumerable<string> repos = await getEndpointsService.GetRepositoriesAsync(orgName, page, perPage, totalPages, token);

                    if (!repos.Any())
                    {
                        return Results.Problem("No repos");
                    }

                    return Results.Ok(repos);
                }
                finally
                {
                    cancellationManager.Cancel(connectionId);
                }                

            })
            .WithName("GetAsync_GetRepositoriesAsync")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);


            app.MapGet("/getasyncapi/repos/{owner}/{repo}/readme", async ([FromRoute] string owner, [FromRoute] string repo,
                                                                          [FromServices] IGetAsyncEndpointsService getEndpointsService,
                                                                          HttpRequest httpRequest) =>
            {
                string contentType = httpRequest.Headers.Accept.FirstOrDefault()?.ToString() ?? "application/json";

                string? readmeContent = await getEndpointsService.GetRepositoriesReadmeAsync(owner, repo, contentType);

                if (string.IsNullOrEmpty(readmeContent))
                {
                    return Results.Problem("No readme");
                }

                return Results.Content(readmeContent, contentType);

            })
            .WithName("GetAsync_GetRepositoriesReadmeAsync")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);


            app.MapGet("/getasyncapi/orgs/{owner}/repos/streamcancel", ([FromRoute] string owner, 
                                                                        [FromQuery] int perPage, [FromQuery(Name = "cid")] string connectionId,
                                                                        [FromServices] IGetAsyncEndpointsService getEndpointsService,
                                                                        [FromServices] CancellationManager cancellationManager,
                                                                        HttpContext httpContext) =>
            {
                CancellationToken cancellationToken = cancellationManager.GetToken(connectionId, 30);

                async IAsyncEnumerable<GitHubRepositoryDto?> StreamWithLifecycleAsync()
                {
                    IAsyncEnumerable<GitHubRepositoryDto?> reposStream = getEndpointsService.GetRepositoriesStreamAsync(owner, perPage, cancellationToken);

                    try
                    {
                        await foreach (var repo in reposStream.WithCancellation(cancellationToken).ConfigureAwait(false))
                        {
                            yield return repo;
                        }
                    }
                    finally
                    {
                        cancellationManager.Cancel(connectionId);
                    }
                }

                return Results.Ok(StreamWithLifecycleAsync());

            })
            .WithName("GetAsync_GetRepositoriesStreamCancelAsync")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);


            #endregion


            #region Commits

            app.MapGet("/getasyncapi/repos/{owner}/{repo}/commits/count", async ([FromRoute] string owner, [FromRoute] string repo,
                                                                     [FromServices] IGetAsyncEndpointsService getEndpointsService) =>
            {
                int? commitsCount = await getEndpointsService.GetCommitsCountAsync(owner, repo);

                if (!commitsCount.HasValue)
                {
                    return Results.Problem("An error ocurred");
                }

                return Results.Ok(commitsCount.Value);

            })
            .WithName("GetAsync_GetCommitsCountAsync")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);


            app.MapGet("/getasyncapi/orgs/{orgname}/repos/{reponame}/commits", async ([FromRoute] string orgname, [FromRoute] string reponame,
                                                                                   [FromQuery(Name = "page")] int page, [FromQuery(Name = "perPage")] int perPage, [FromQuery(Name = "totalPages")] int totalPages,
                                                                                   [FromQuery(Name = "cid")] string connectionId,
                                                                                   [FromServices] IGetAsyncEndpointsService getEndpointsService, [FromServices] CancellationManager cancellationManager) =>
            {
                var cancellationToken = cancellationManager.GetToken(connectionId, 30);

                try
                {
                    IEnumerable<GitHubCommitDto> result = await getEndpointsService.GetCommitsAsync(orgname, reponame, page, perPage, totalPages, cancellationToken).ConfigureAwait(false);

                    if (result == null || !result.Any())
                    {
                        return Results.Ok(new CommitsSummaryResponseDto { Commits = [], Total = 0 });
                    }

                    // Cache the execution list count to avoid double-enumeration of the IEnumerable
                    var commitList = result.ToList();

                    List<string> commitsSummary = commitList.Select(c =>
                    {
                        if (string.IsNullOrEmpty(c.CommitMessage)) return $"{c.CommitDate} -- ";

                        string decodedMessage = WebUtility.HtmlDecode(c.CommitMessage);
                        string flattenedMessage = decodedMessage.Replace("\n", " ", StringComparison.OrdinalIgnoreCase);
                        string cleanMessage = _cleanMessageRegex.Replace(flattenedMessage, "").Trim();

                        return $"{c.CommitDate} -- {cleanMessage}";
                    }).ToList();

                    return Results.Ok(new CommitsSummaryResponseDto()
                    {
                        Commits = commitsSummary,
                        Total = commitList.Count
                    });
                }
                catch (OperationCanceledException)
                {
                    // Return 499 indicating the request was stopped by client or system timeout
                    return Results.StatusCode(StatusCodes.Status499ClientClosedRequest);
                }
                finally
                {
                    cancellationManager.Cancel(connectionId);
                }
            })
            .WithName("GetAsync_GetCommitsAsync")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);


            app.MapGet("/getasyncapi/orgs/{orgname}/repos/{reponame}/commits/streams", async ([FromRoute] string orgname, [FromRoute] string reponame,
                                                                                           [FromQuery(Name = "page")] int page, [FromQuery(Name = "perPage")] int perPage, [FromQuery(Name = "totalPages")] int totalPages,
                                                                                           [FromServices] IGetAsyncEndpointsService getEndpointsService, HttpContext context) =>
            {
                return Results.Ok("Hello from GetAsync_GetCommitsStreamAsync");
            })
            .WithName("GetAsync_GetCommitsStreamAsync");


            app.MapGet("/getasyncapi/orgs/{orgname}/repos/{reponame}/commits/streams/cancel", async ([FromRoute] string orgname, [FromRoute] string reponame,
                                                                                                 [FromQuery(Name = "page")] int page, [FromQuery(Name = "perPage")] int perPage, [FromQuery(Name = "totalPages")] int totalPages,
                                                                                                 [FromQuery(Name = "cid")] string connectionId,
                                                                                                 [FromServices] IGetAsyncEndpointsService getEndpointsService, 
                                                                                                 [FromServices] CancellationManager cancellationManager, 
                                                                                                 HttpContext context) =>
            {
                CancellationToken token = cancellationManager.GetToken(connectionId, 30);

                int count = 0;

                try
                {
                    IAsyncEnumerable<(string commitMessage, DateTime commitDate)> commitsStream = getEndpointsService.GetCommitsStreamAsync(orgname, reponame, page, perPage, totalPages, token);

                    await foreach (var commit in commitsStream.WithCancellation(token))
                    {
                        count++;

                        string result = $"{commit.commitDate}-{commit.commitMessage}";

                        await context.Response.WriteAsync($"{result}\n", token);

                        await context.Response.Body.FlushAsync(token);

                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }

                    await context.Response.WriteAsync($"----Processed: {count}-------\n", token);
                }
                catch (OperationCanceledException)
                {
                    await context.Response.WriteAsync("Client disconnected\n", token);
                    await context.Response.Body.FlushAsync(token);
                }
                finally
                {
                    cancellationManager.Cancel(connectionId);
                }

                return Results.Empty;

            })
            .WithName("GetAsync_GetCommitsStreamCancelAsync");


            #endregion
        }
    }
}
