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


            app.MapGet("/orgs/{orgname}/repos/{reponame}/commits", async ([FromRoute] string orgname, [FromRoute] string reponame, [FromQuery(Name = "cid")] string connectionId,
                                                                       [FromServices] IGetEndpointsService getEndpointsService, [FromServices] CancellationManager cancellationManager) =>
            {
                var token = cancellationManager.GetToken(connectionId);

                (List<(string commitMessage, DateTime commitDate)> commits, int total) result = await getEndpointsService.GetRepositoryCommits(orgname, reponame, 1, 10, 100, token);

                return Results.Ok(new { Commits = result.commits.Select(c => 
                                        {
                                            var decodedMessage = WebUtility.HtmlDecode(c.commitMessage);
                                            var cleanMessage = Regex.Replace(decodedMessage.Replace("\n", " "), @"[^a-zA-Z0-9\s]", "").Trim();
                                            return $"{c.commitDate} -- {cleanMessage}";
                                        }), 
                                        Total = result.total });

            }).WithName("GetCommits");


            app.MapPost("/cancel-work/{id}", (string id, CancellationManager manager) => {
                manager.Cancel(id);
                return Results.Ok($"Work for {id} requested to stop.");
            });
        }
    }
}
