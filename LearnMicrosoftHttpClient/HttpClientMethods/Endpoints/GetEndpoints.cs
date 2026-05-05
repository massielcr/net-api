using HttpClientMethods.Services;
using System.Collections.Concurrent;

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

            app.Map("/repos/commits", async (string id, CancellationManager manager, IGetEndpointsService getEndpointsService, CancellationToken cancellationToken) =>
            {
                var token = manager.GetToken(id);

                IAsyncEnumerable<(string commitMessage, DateTime commitDate, int total)> commits = getEndpointsService.GetRepositoryCommits("dotnet", "runtime", 1, 10, cancellationToken);

                return Results.Ok(1);

            }).WithName("GetCommits");

            app.MapPost("/cancel-work/{id}", (string id, CancellationManager manager) => {
                manager.Cancel(id);
                return Results.Ok($"Work for {id} requested to stop.");
            });
        }
    }
}
