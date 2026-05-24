using HttpClientMethods.Dtos;
using HttpClientMethods.Helpers;
using HttpClientMethods.Services;
using Microsoft.AspNetCore.Mvc;

namespace HttpClientMethods.Endpoints
{
    public static class PostAsyncEndpoints
    {
        public static void MapPostAsyncEndpoints(this WebApplication app)
        {
            app.MapPost("postasync/repos", async ([FromBody] CreatePersonalRepositoryRequestDto repo,
                                                  [FromServices] IPostAsyncEndpointsService postAsyncEndpointsService) =>
            {

                if (string.IsNullOrWhiteSpace(repo.Name))
                {
                    return Results.BadRequest(new { error = "Repository name cannot be empty." });
                }

                bool success = await postAsyncEndpointsService.CreatePersonalRepositoryAsync(repo.Name, repo.Description, repo.IsPrivate, repo.InitialREADME, repo.HasDownloads);

                if (!success) { return Results.InternalServerError(new { error = "Could not create the repository." }); }

                return Results.StatusCode(StatusCodes.Status201Created);

            })
            .WithName("PostAsync_CreatePersonalRepositoryAsync")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

            app.MapPost("postasync/repos/{owner}/{repo}/issues", async ([FromRoute] string owner, [FromRoute] string repo,
                                                                        [FromQuery] int count, [FromQuery(Name = "cid")] string connectionId,
                                                                        [FromBody] CreatePersonalRepoIssueRequestDto issue,
                                                                        [FromServices] IPostAsyncEndpointsService postAsyncEndpointsService,
                                                                        [FromServices] CancellationManager cancellationManager) =>
            {

                if (string.IsNullOrWhiteSpace(issue.Title))
                {
                    return Results.BadRequest(new { error = "Issue title cannot be empty." });
                }

                CancellationToken cancellationToken = cancellationManager.GetToken(connectionId, 30);

                try
                {
                    bool success = await postAsyncEndpointsService.CreateRepositoryIssuesAsync(owner, repo, issue.Title, issue.Body, count, cancellationToken);

                    if (!success) { return Results.InternalServerError(new { error = "Could not create the issue." }); }

                    return Results.StatusCode(StatusCodes.Status201Created);
                }
                catch(OperationCanceledException)
                {
                    return Results.StatusCode(StatusCodes.Status499ClientClosedRequest);
                }
            })
            .WithName("PostAsync_CreateRepositoryIssuesAsync")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status499ClientClosedRequest)
            .Produces(StatusCodes.Status500InternalServerError);


            app.MapPost("postasync/repos/{owner}/{repo}/issuewithimage", async ([FromRoute] string owner, [FromRoute] string repo,
                                                                                [FromBody] CreatePersonalRepoIssueWithImageRequestDto issue,
                                                                                [FromServices] IPostAsyncEndpointsService postAsyncEndpointsService,
                                                                                [FromServices] IFileService fileService,
                                                                                [FromServices] IWebHostEnvironment webHostEnvironment) =>
            {

                if (string.IsNullOrWhiteSpace(issue.Title))
                {
                    return Results.BadRequest(new { error = "Issue title cannot be empty." });
                }

                try
                {
                    //Get File content as byte array
                    string webRoot = webHostEnvironment.WebRootPath ?? Path.Combine(webHostEnvironment.ContentRootPath, "wwwroot");
                    string folderPath = Path.Combine(webRoot, "images");

                    byte[] issueImage = await fileService.GetFileAsync(folderPath, issue.ImageFileName);

                    if (issueImage == null || issueImage.Length == 0)
                    {
                        return Results.BadRequest(new { error = "Failed to retrieve the image." });
                    }

                    // Create issue with file content
                    bool success = await postAsyncEndpointsService.CreateRepositoryIssueAsync(owner, repo, issue.Title, issueImage);

                    if (!success) { return Results.InternalServerError(new { error = "Could not create the issue." }); }

                    return Results.StatusCode(StatusCodes.Status201Created);
                }
                catch (OperationCanceledException)
                {
                    return Results.StatusCode(StatusCodes.Status499ClientClosedRequest);
                }
            })
           .WithName("PostAsync_CreateRepositoryIssueWithImageAsync")
           .Produces(StatusCodes.Status201Created)
           .Produces(StatusCodes.Status400BadRequest)
           .Produces(StatusCodes.Status500InternalServerError);

            app.MapPost("postasync/repos/{owner}/{repo}/issueswithimage", async ([FromRoute] string owner, [FromRoute] string repo,
                                                                                 [FromQuery] int count, [FromQuery(Name = "cid")] string connectionId,
                                                                                 [FromBody] CreatePersonalRepoIssueWithImageRequestDto issue,
                                                                                 [FromServices] IPostAsyncEndpointsService postAsyncEndpointsService,
                                                                                 [FromServices] IFileService fileService,
                                                                                 [FromServices] IWebHostEnvironment webHostEnvironment,
                                                                                 [FromServices] CancellationManager cancellationManager) =>
            {

                if (string.IsNullOrWhiteSpace(issue.Title))
                {
                    return Results.BadRequest(new { error = "Issue title cannot be empty." });
                }

                CancellationToken cancellationToken = cancellationManager.GetToken(connectionId, 30);

                try
                {
                    //Get File content as byte array
                    string webRoot = webHostEnvironment.WebRootPath ?? Path.Combine(webHostEnvironment.ContentRootPath, "wwwroot");
                    string folderPath = Path.Combine(webRoot, "images");

                    byte[] issueImage = await fileService.GetFileAsync(folderPath, issue.ImageFileName);

                    if (issueImage == null || issueImage.Length == 0)
                    {
                        return Results.BadRequest(new { error = "Failed to retrieve the image." });
                    }

                    // Create issue with file content
                    bool success = await postAsyncEndpointsService.CreateRepositoryIssuesAsync(owner, repo, issue.Title, issueImage, count, cancellationToken);

                    if (!success) { return Results.InternalServerError(new { error = "Could not create the issue." }); }

                    return Results.StatusCode(StatusCodes.Status201Created);
                }
                catch (OperationCanceledException)
                {
                    return Results.StatusCode(StatusCodes.Status499ClientClosedRequest);
                }
            })
           .WithName("PostAsync_CreatePersonalRepositoryIssuesWithImageAsync")
           .Produces(StatusCodes.Status201Created)
           .Produces(StatusCodes.Status400BadRequest)
           .Produces(StatusCodes.Status499ClientClosedRequest)
           .Produces(StatusCodes.Status500InternalServerError);
        }
    }
}
