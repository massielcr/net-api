using HttpClientMethods.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.RegularExpressions;

namespace HttpClientMethods.Methods
{
    public static class GetEndpoints
    {
        public static void MapGetEndpoints(this WebApplication app)
        {
            app.MapGet("/repos/count", async (IGetEndpointsService getEndpointsService) =>
            {
                int reposCount = await getEndpointsService.GetRepositoriesCountAsync();

                if (reposCount >= 0)
                {
                    return Results.Ok(reposCount);
                }
                else
                {
                    return Results.Problem("An error ocurred");
                }               

            }).WithName("GetRepositoriesCount");


            app.MapGet("/repos", async (IGetEndpointsService getEndpointsService) =>
            {
                IEnumerable<string> repos = await getEndpointsService.GetAllRepositoriesAsync();

                if (repos.Any())
                {
                    return Results.Ok(repos);
                }
                else
                {
                    return Results.Problem("No repos");
                }               

            }).WithName("GetRepositories");


            app.MapGet("/orgs/{orgname}/repos/{reponame}/commits", async ([FromRoute] string orgname, [FromRoute] string reponame,
                                                                          [FromQuery(Name = "page")] int page, [FromQuery(Name = "perPage")] int perPage, [FromQuery(Name = "totalPages")] int totalPages,
                                                                          [FromQuery(Name = "cid")] string connectionId,
                                                                          [FromServices] IGetEndpointsService getEndpointsService, [FromServices] CancellationManager cancellationManager) =>
            {
                var token = cancellationManager.GetToken(connectionId, 30);

                try
                {
                    (List<(string commitMessage, DateTime commitDate)> commits, int total) result = await getEndpointsService.GetRepositoryCommits(orgname, reponame, page, perPage, totalPages, token);


                    if (result.commits != null && result.commits.Any())
                    {
                        return Results.Ok(new
                        {
                            Commits = result.commits.Select(c =>
                            {
                                var decodedMessage = WebUtility.HtmlDecode(c.commitMessage);
                                var cleanMessage = Regex.Replace(decodedMessage.Replace("\n", " "), @"[^a-zA-Z0-9\s]", "").Trim();
                                return $"{c.commitDate} -- {cleanMessage}";
                            }),
                            Total = result.total
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
            }).WithName("GetCommits");


            app.MapGet("/orgs/{orgname}/repos/{reponame}/commitstream", async ([FromRoute] string orgname, [FromRoute] string reponame,
                                                                          [FromQuery(Name = "page")] int page, [FromQuery(Name = "perPage")] int perPage, [FromQuery(Name = "totalPages")] int totalPages,
                                                                          [FromQuery(Name = "cid")] string connectionId,
                                                                          [FromServices] IGetEndpointsService getEndpointsService, [FromServices] CancellationManager cancellationManager) =>
            {
                CancellationToken token = cancellationManager.GetToken(connectionId, 30);

                try
                {
                    IAsyncEnumerable<(string commitMessage, DateTime commitDate)> commitStream = getEndpointsService.GetRepositoryCommitsStreamAsync(orgname, reponame, page, perPage, totalPages, token);

                    return Results.Ok(commitStream.Select(c => new
                    {
                        Commit = $"{c.commitDate} - {c.commitMessage}"
                    }));
                }
                catch (OperationCanceledException)
                {
                    // Return a specific status indicating the request was stopped
                    return Results.StatusCode(499);
                }
            });


            app.MapPost("/cancel-work", ([FromQuery(Name ="cid")] string connectionId, [FromServices] CancellationManager cancellationManager) => {

                cancellationManager.Cancel(connectionId);

                return Results.Ok($"Work for {connectionId} requested to stop.");
            });
        }
    }
}
