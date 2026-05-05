using HttpClientMethods.Services;

namespace HttpClientMethods.Methods
{
    public static class GetEndpoints
    {
        public static void MapSendEndpoints(this WebApplication app)
        {
            app.Map("/repos", async (string? username, IGetEndpointsService sendEndpointsService) =>
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    return Results.BadRequest("Username query parameter is required.");
                }

                var repos = await sendEndpointsService.GetRepositoriesStringAsync(username);

                return Results.Ok(repos);

            }).WithName("GetAsync()");
        }
    }
}
