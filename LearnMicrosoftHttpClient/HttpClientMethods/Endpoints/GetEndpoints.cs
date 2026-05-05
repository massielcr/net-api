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
            app.Map("/repos/count", async (IGetEndpointsService getEndpointsService) =>
            {
                int reposCount = await getEndpointsService.GetRepositoriesCountAsync();

                return Results.Ok(reposCount);

            }).WithName("GetRepositoriesCount");


            app.Map("/repos", async (IGetEndpointsService getEndpointsService) =>
            {
                IEnumerable<string> repos = await getEndpointsService.GetAllRepositoriesAsync();

                return Results.Ok(repos);

            }).WithName("GetRepositories");

            app.Map("/orgs/{orgname}/repos/{reponame}/commits", async ([FromRoute] string orgname, [FromRoute] string reponame, [FromQuery(Name = "cid")] string connectionId,
                                                                       [FromServices] IGetEndpointsService getEndpointsService, [FromServices] CancellationManager cancellationManager) =>
            {
                var token = cancellationManager.GetToken(connectionId);

                (List<(string commitMessage, DateTime commitDate)> commits, int total) result = await getEndpointsService.GetRepositoryCommits(orgname, reponame, 1, 10, 1, token);

                return Results.Ok(new { Commits = result.commits.Select(c => 
                                        {
                                            var decoded = WebUtility.HtmlDecode(c.commitMessage);
                                            var clean = Regex.Replace(decoded.Replace("\n", " "), @"[^a-zA-Z0-9\s]", "").Trim();
                                            return $"{c.commitDate} -- {clean}";
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
