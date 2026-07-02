using HttpClientMethods.Dtos;
using HttpClientMethods.Interfaces;
using HttpClientMethods.Services;
using Microsoft.AspNetCore.Mvc;

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
