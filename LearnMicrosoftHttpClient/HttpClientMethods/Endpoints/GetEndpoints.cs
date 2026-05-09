using HttpClientMethods.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.RegularExpressions;

namespace HttpClientMethods.Methods
{
    public static class GetEndpoints
    {
        public static void MapGetEndpoints(this WebApplication app)
        {
            #region Repositories

            app.MapGet("/orgs/{orgname}/repos/count", async ([FromRoute] string orgname,
                                                             [FromServices] IGetEndpointsService getEndpointsService) =>
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


            app.MapGet("/orgs/{orgname}/repos", async ([FromRoute] string orgName, 
                                                       [FromQuery] int page, [FromQuery] int perPage, [FromQuery] int totalPages, [FromQuery(Name = "cid")] string connectionId, 
                                                       [FromServices] IGetEndpointsService getEndpointsService, [FromServices] CancellationManager cancellationManager) =>
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


            app.MapGet("/orgs/{orgname}/repos/stream", async () =>
            {
                await Task.Delay(1000);
                return Results.Ok("Hello world");

            }).WithName("GetRepositoriesStreamAsync");


            app.MapGet("/orgs/{orgname}/repos/stream/cancel", async () =>
            {
                await Task.Delay(1000);
                return Results.Ok("Hello world");

            }).WithName("GetRepositoriesStreamCancelAsync");


            #endregion


            #region Commits

            app.MapGet("/orgs/{orgname}/repos/{reponame}/commits/count", async ([FromRoute] string orgName, [FromRoute] string reponame,
                                                                                [FromServices] IGetEndpointsService getEndpointsService) =>
            {
                int commitsCount = await getEndpointsService.GetCommitsCountAsync(orgName, reponame);

                if (commitsCount >= 0)
                {
                    return Results.Ok(commitsCount);
                }
                else
                {
                    return Results.Problem("An error ocurred");
                }

            }).WithName("GetCommitsCountAsync");


            app.MapGet("/orgs/{orgname}/repos/{reponame}/commits", async ([FromRoute] string orgname, [FromRoute] string reponame,
                                                                          [FromQuery(Name = "page")] int page, [FromQuery(Name = "perPage")] int perPage, [FromQuery(Name = "totalPages")] int totalPages,
                                                                          [FromQuery(Name = "cid")] string connectionId,
                                                                          [FromServices] IGetEndpointsService getEndpointsService, [FromServices] CancellationManager cancellationManager) =>
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
            }).WithName("GetCommitsAsync");


            app.MapGet("/orgs/{orgname}/repos/{reponame}/commits/stream", async ([FromRoute] string orgname, [FromRoute] string reponame,
                                                                          [FromQuery(Name = "page")] int page, [FromQuery(Name = "perPage")] int perPage, [FromQuery(Name = "totalPages")] int totalPages,
                                                                          [FromQuery(Name = "cid")] string connectionId,
                                                                          [FromServices] IGetEndpointsService getEndpointsService, [FromServices] CancellationManager cancellationManager, HttpContext context) =>
            {
                var responseFeature = context.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpResponseBodyFeature>();
                responseFeature?.DisableBuffering();

                CancellationToken token = cancellationManager.GetToken(connectionId, 30);

                context.Response.ContentType = "application/text";

                try
                {

                    IAsyncEnumerable<(string commitMessage, DateTime commitDate)> commitsStream = getEndpointsService.GetCommitsStreamAsync(orgname, reponame, page, perPage, totalPages, token);

                    int count = 0;

                    await foreach (var commit in commitsStream.WithCancellation(token))
                    {
                        count++;

                        string result = $"{commit.commitDate}-{commit.commitMessage}";

                        await context.Response.WriteAsync($"{result}\n", token);

                        await context.Response.Body.FlushAsync(token);
                    }

                    await context.Response.WriteAsync($"----Processed: {count}-------\n", token);
                }
                catch (Exception) when (context.RequestAborted.IsCancellationRequested)
                {
                    // Client disconnected, stop silently
                }

                return Results.Empty;

            }).WithName("GetCommitsStreamAsync");


            app.MapGet("/orgs/{orgname}/repos/{reponame}/commits/streams/cancel",() =>
            {

            }).WithName("GetCommitsStreamCancelAsync");


            #endregion


            app.MapPost("/cancel-work", ([FromQuery(Name ="cid")] string connectionId, [FromServices] CancellationManager cancellationManager) => {

                cancellationManager.Cancel(connectionId);

                return Results.Ok($"Work for {connectionId} requested to stop.");
            });
        }
    }
}
