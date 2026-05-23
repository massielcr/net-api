using HttpClientMethods.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.RegularExpressions;

namespace HttpClientMethods.Endpoints
{
    public static class GetAsyncEndpoints
    {
        public static void MapGetAsyncEndpoints(this WebApplication app)
        {
            #region Repositories

            app.MapGet("/getasyncapi/orgs/{orgname}/repos/count", async ([FromRoute] string orgname,
                                                                         [FromServices] IGetAsyncEndpointsService getEndpointsService) =>
            {
                int? reposCount = await getEndpointsService.GetRepositoriesCountAsync(orgname);

                if (reposCount.HasValue)
                {
                    return Results.Ok(reposCount.Value);
                }
                else
                {
                    return Results.Problem("An error ocurred");
                }               

            }).WithName("GetRepositoriesCountAsync");


            app.MapGet("/getasyncapi/orgs/{orgname}/repos", async ([FromRoute] string orgName, 
                                                       [FromQuery] int page, [FromQuery] int perPage, [FromQuery] int totalPages, [FromQuery(Name = "cid")] string connectionId, 
                                                       [FromServices] IGetAsyncEndpointsService getEndpointsService, [FromServices] CancellationManager cancellationManager) =>
            {
                CancellationToken token = cancellationManager.GetToken(connectionId, 30);

                try
                {
                    IEnumerable<string> repos = await getEndpointsService.GetRepositoriesAsync(orgName, page, perPage, totalPages, token);

                    if (repos.Any())
                    {
                        return Results.Ok(repos);
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

            }).WithName("GetRepositoriesAsync");


            app.MapGet("/getasyncapi/orgs/{orgname}/repos/stream", async () =>
            {
                await Task.Delay(1000);
                return Results.Ok("Hello world");

            }).WithName("GetRepositoriesStreamAsync");


            app.MapGet("/getasyncapi/orgs/{orgname}/repos/stream/cancel", async () =>
            {
                await Task.Delay(1000);
                return Results.Ok("Hello world");

            }).WithName("GetRepositoriesStreamCancelAsync");


            #endregion


            #region Commits

            app.MapGet("/getasyncapi/repos/{owner}/{repo}/commits/count", async ([FromRoute] string owner, [FromRoute] string repo,
                                                                     [FromServices] IGetAsyncEndpointsService getEndpointsService) =>
            {
                int? commitsCount = await getEndpointsService.GetCommitsCountAsync(owner, repo);

                if (commitsCount.HasValue)
                {
                    return Results.Ok(commitsCount.Value);
                }
                else
                {
                    return Results.Problem("An error ocurred");
                }

            }).WithName("GetAsync_GetCommitsCountAsync");


            app.MapGet("/getasyncapi/orgs/{orgname}/repos/{reponame}/commits", async ([FromRoute] string orgname, [FromRoute] string reponame,
                                                                                   [FromQuery(Name = "page")] int page, [FromQuery(Name = "perPage")] int perPage, [FromQuery(Name = "totalPages")] int totalPages,
                                                                                   [FromQuery(Name = "cid")] string connectionId,
                                                                                   [FromServices] IGetAsyncEndpointsService getEndpointsService, [FromServices] CancellationManager cancellationManager) =>
            {
                var token = cancellationManager.GetToken(connectionId, 30);

                try
                {
                    IEnumerable<(string commitMessage, DateTime commitDate)> result = await getEndpointsService.GetCommitsAsync(orgname, reponame, page, perPage, totalPages, token);


                    if (result != null && result.Any())
                    {
                        return Results.Ok(new
                        {
                            Commits = result.Select(c =>
                            {
                                var decodedMessage = WebUtility.HtmlDecode(c.commitMessage);
                                var cleanMessage = Regex.Replace(decodedMessage.Replace("\n", " "), @"[^a-zA-Z0-9\s]", "").Trim();
                                return $"{c.commitDate} -- {cleanMessage}";
                            }),
                            Total = result.Count()
                        });
                    }
                    else
                    {
                        return Results.Problem("No commits");
                    }                    
                }
                catch (OperationCanceledException)
                {
                    // Return a specific status indicating the request was stopped
                    return Results.StatusCode(499);
                }
                finally
                {
                    cancellationManager.Cancel(connectionId);
                }
            }).WithName("GetAsync_GetCommitsAsync");


            app.MapGet("/getasyncapi/orgs/{orgname}/repos/{reponame}/commits/streams", async ([FromRoute] string orgname, [FromRoute] string reponame,
                                                                                           [FromQuery(Name = "page")] int page, [FromQuery(Name = "perPage")] int perPage, [FromQuery(Name = "totalPages")] int totalPages,
                                                                                           [FromServices] IGetAsyncEndpointsService getEndpointsService, HttpContext context) =>
            {
                return Results.Ok("Hello from GetAsync_GetCommitsStreamAsync");
            }).WithName("GetAsync_GetCommitsStreamAsync");


            app.MapGet("/getasyncapi/orgs/{orgname}/repos/{reponame}/commits/streams/cancel", async ([FromRoute] string orgname, [FromRoute] string reponame,
                                                                                                 [FromQuery(Name = "page")] int page, [FromQuery(Name = "perPage")] int perPage, [FromQuery(Name = "totalPages")] int totalPages,
                                                                                                 [FromQuery(Name = "cid")] string connectionId,
                                                                                                 [FromServices] IGetAsyncEndpointsService getEndpointsService, [FromServices] CancellationManager cancellationManager, HttpContext context) =>
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

            }).WithName("GetAsync_GetCommitsStreamCancelAsync");


            #endregion


            app.MapPost("/cancel-work", ([FromQuery(Name ="cid")] string connectionId, [FromServices] CancellationManager cancellationManager) => {

                cancellationManager.Cancel(connectionId);

                return Results.Ok($"Work for {connectionId} requested to stop.");
            });
        }
    }
}
