using HttpClientMethods.Services;

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
        }
    }
}
