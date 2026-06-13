using HttpClientMethods.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace HttpClientMethods.Endpoints
{
    public static class CancellationEndpoints
    {
        public static void MapCancellationEndpoints(this WebApplication app)
        {
            app.MapGet("/cancellationapi/cancel", ([FromQuery(Name = "cid")] string connectionId, [FromServices] CancellationManager cancellationManager) =>
            {
                cancellationManager.Cancel(connectionId);

                return Results.Ok($"Cancellation requested for connectionId: {connectionId}");
            })
            .WithName("CancelOperation")
            .Produces(StatusCodes.Status200OK);
        }
    }
}
