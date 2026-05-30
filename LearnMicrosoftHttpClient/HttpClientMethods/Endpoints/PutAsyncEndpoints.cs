using HttpClientMethods.Dtos;
using HttpClientMethods.Helpers;
using HttpClientMethods.Models;
using HttpClientMethods.Services;
using Microsoft.AspNetCore.Mvc;

namespace HttpClientMethods.Endpoints
{
    public static class PutAsyncEndPoints
    {
        public static void MapPutAsyncEndpoints(this WebApplication app)
        {
            app.MapPut("putasync/repos/{owner}/{repo}/topics", async ([FromRoute] string owner, [FromRoute] string repo,
                                                                      [FromBody] UpdateRepoTopicsRequestDto topics,
                                                                      [FromServices] IPutAsyncEndpointsService putAsyncEndpointsService) =>
            {
                if (topics == null || topics.Names == null || topics.Names.Count == 0)
                {
                    return Results.BadRequest(new { error = "Topics list cannot be empty." });
                }

                bool success = await putAsyncEndpointsService.UpdateRepositoryTopicsAsync(owner, repo, topics.Names);

                if (!success) { return Results.InternalServerError(new { error = "Could not replace the repository topics." }); }

                return Results.Ok();
            })
            .WithName("PutAsync_ReplaceRepositoryTopicsAsync")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

            app.MapPut("putasync/repos/{owner}/{repo}/issues/lockbatch", async ([FromRoute] string owner, [FromRoute] string repo,
                                                                                  [FromQuery(Name = "cid")] string connectionId,
                                                                                  [FromBody] LockRepoIssuesRequestDto issues,
                                                                                  [FromServices] IPutAsyncEndpointsService putAsyncEndpointsService,
                                                                                  [FromServices] CancellationManager cancellationManager) =>
            {
                if (issues == null || issues.Issues.Count == 0)
                {
                    return Results.BadRequest(new { error = "Issue number must be greater than zero." });
                }

                CancellationToken cancellationToken = cancellationManager.GetToken(connectionId, seconds: 30);

                List<GitHubIssue> githubIssues = issues.Issues.Select(i => new GitHubIssue
                {
                    IssueNumber = i.IssueNumber,
                    LockReason = i.LockReason
                }).ToList();

                try
                {
                    bool success = await putAsyncEndpointsService.LockRepositoryIssuesAsync(owner, repo, githubIssues, cancellationToken);

                    if (!success) { return Results.InternalServerError(new { error = "Could not lock the repository issues." }); }

                    return Results.Ok();

                }
                catch (OperationCanceledException)
                {
                    return Results.StatusCode(499); // Client Closed Request
                }
            })
            .WithName("PutAsync_LockRepositoryIssuesAsync")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status499ClientClosedRequest)
            .Produces(StatusCodes.Status500InternalServerError);

            app.MapPut("putasync/repos/{owner}/{repo}/issues/lock", async ( [FromRoute] string owner, [FromRoute] string repo,
                                                                            [FromBody] LockRepoIssueRequestDto issue,
                                                                            [FromServices] IPutAsyncEndpointsService putAsyncEndpointsService) =>
            {
                if (issue == null || issue.IssueNumber <= 0)
                {
                    return Results.BadRequest(new { error = "Issue number must be greater than zero." });
                }

                GitHubIssue githubIssue = new()
                {
                    IssueNumber = issue.IssueNumber,
                    LockReason = issue.LockReason
                };

                bool success = await putAsyncEndpointsService.LockRepositoryIssueAsync(owner, repo, githubIssue);

                if (!success) { return Results.InternalServerError(new { error = "Could not lock the repository issue." }); }

                return Results.Ok();
            })
            .WithName("PutAsync_LockRepositoryIssueAsync")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError); ;


            app.MapPut("putasync/repos/{owner}/{repo}/issues/lockbatchstream", async ([FromRoute] string owner, [FromRoute] string repo,
                                                                                        [FromQuery] string connectionId,
                                                                                        [FromBody] LockRepoIssuesRequestDto issues,                                                                                  
                                                                                        [FromServices] IPutAsyncEndpointsService putAsyncEndpointsService,
                                                                                        [FromServices] CancellationManager cancellationManager) =>
            {
                if (issues == null || !issues.Issues.Any())
                {
                    return Results.BadRequest(new { error = "Issue number must be greater than zero." });
                }

                CancellationToken cancellationToken = cancellationManager.GetToken(connectionId, seconds: 30);

                List<GitHubIssue> githubIssues = issues.Issues.Select(i => new GitHubIssue
                {
                    IssueNumber = i.IssueNumber,
                    LockReason = i.LockReason,
                }).ToList();

                IAsyncEnumerable<int> success = putAsyncEndpointsService.LockRepositoryIssuesStreamAsync(owner, repo, githubIssues, cancellationToken);

                return Results.Ok();
            })
            .WithName("PutAsync_LockRepositoryIssuesStreamAsync");
        }
    }
}
