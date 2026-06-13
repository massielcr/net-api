using HttpClientMethods.Dtos;
using HttpClientMethods.Interfaces;
using HttpClientMethods.Models;
using HttpClientMethods.Services;
using Microsoft.AspNetCore.Mvc;

namespace HttpClientMethods.Endpoints
{
    public static class PatchAsyncEndpoints
    {
        public static void MapPatchAsyncEndpoints(this WebApplication app)
        {
            app.MapPatch("/patchasync/repos/{owner}/{repo}/update", async ([FromRoute] string owner, [FromRoute] string repo,
                                                                           [FromBody] UpdateRepoRequestDto updateRepoRequestDto,
                                                                           [FromServices] IPatchAsyncEndpoinsService patchAsyncEndpoinsService) =>
            {

                if (updateRepoRequestDto == null || string.IsNullOrEmpty(updateRepoRequestDto.Visibility)) 
                { 
                    return Results.BadRequest(new { error = "Visibility cannot be empty." }); 
                }

                bool response = await patchAsyncEndpoinsService.UpdateRepoVisibilityAsync(owner, repo, updateRepoRequestDto.Visibility);

                if(!response)
                {
                    return Results.Problem("Failed to update repository visibility.");
                }

                return Results.Ok();
            })
            .WithName("PatchAsync_UpdateRepoVisibilityAsync")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError); ;


            app.MapPatch("/patchasync/repos/{owner}/{repo}/issues/updatebatch", async ([FromRoute] string owner, [FromRoute] string repo,
                                                                                       [FromQuery(Name ="cid")] string connectionId,
                                                                                       [FromBody] UpdateRepoIssuesRequestDto updateRepoIssuesRequestDto,
                                                                                       [FromServices] IPatchAsyncEndpoinsService patchAsyncEndpoinsService,
                                                                                       [FromServices] ICancellationService cancellationService) =>
            {
                if (updateRepoIssuesRequestDto == null || updateRepoIssuesRequestDto.Issues == null || updateRepoIssuesRequestDto.Issues.Count == 0)
                {
                    return Results.BadRequest(new { error = "There are no issues to update." });
                }

                CancellationToken cancellationToken = cancellationService.GetToken(connectionId, seconds: 30);

                try
                {
                    GitHubIssue[] githubIssues = [.. updateRepoIssuesRequestDto.Issues.Select(issue => new GitHubIssue
                    {
                        IssueNumber = issue.IssueNumber,
                        State = issue.State,
                        StateReason = issue.StateReason,
                        Labels = issue.Labels
                    })];

                    bool response = await patchAsyncEndpoinsService.UpdateRepoIssuesAsync(owner, repo, githubIssues, cancellationToken);
                    
                    if (!response)
                    {
                        return Results.InternalServerError(new { error = "Failed to update repository issues." });
                    }
                }
                catch (OperationCanceledException)
                {
                    return Results.StatusCode(499);
                }

                return Results.Ok();
            })
            .WithName("PatchAsync_UpdateRepoIssuesAsync")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status499ClientClosedRequest)
            .Produces(StatusCodes.Status500InternalServerError);

            app.MapPatch("/patchasync/repos/{owner}/{repo}/issues/update", async ([FromRoute] string owner, [FromRoute] string repo,
                                                                                  [FromBody] UpdateRepoIssueRequestDto updateRepoIssueRequestDto,
                                                                                  [FromServices] IPatchAsyncEndpoinsService patchAsyncEndpoinsService) =>
            {
                if (updateRepoIssueRequestDto == null)
                {
                    return Results.BadRequest(new { error = "There is no issue to update." });
                }

                GitHubIssue gitHubIssue = new()
                {
                    IssueNumber = updateRepoIssueRequestDto.IssueNumber,
                    State = updateRepoIssueRequestDto.State,
                    StateReason = updateRepoIssueRequestDto.StateReason,
                    Labels = updateRepoIssueRequestDto.Labels
                };

                bool response = await patchAsyncEndpoinsService.UpdateRepoIssueAsync(owner, repo, gitHubIssue);

                if (!response)
                {
                    return Results.InternalServerError(new { error = "Failed to update the repository issue." });
                }

                return Results.Ok();
            })
            .WithName("PatchAsync_UpdateRepoIssueAsync")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

            app.MapPatch("/patchasync/repos/{owner}/{repo}/issues/updatebatchstream", async ([FromRoute] string owner, [FromRoute] string repo,
                                                                                             [FromQuery(Name = "cid")] string connectionId,
                                                                                             [FromBody] UpdateRepoIssuesRequestDto updateRepoIssuesRequestDto,
                                                                                             [FromServices] IPatchAsyncEndpoinsService patchAsyncEndpoinsService,
                                                                                             [FromServices] ICancellationService cancellationService,
                                                                                             HttpContext context) =>
            {

                if (updateRepoIssuesRequestDto == null || updateRepoIssuesRequestDto.Issues == null || updateRepoIssuesRequestDto.Issues.Count == 0)
                {
                    return Results.BadRequest(new { error = "There are no issues to update." });
                }

                CancellationToken cancellationToken = cancellationService.GetToken(connectionId, seconds: 30);

                IEnumerable<GitHubIssue> githubIssues = updateRepoIssuesRequestDto.Issues.Select(issue => new GitHubIssue
                {
                    IssueNumber = issue.IssueNumber,
                    State = issue.State,
                    StateReason = issue.StateReason,
                    Labels = issue.Labels
                });

                try
                {
                    int count = 0;

                    await foreach (int issueNumber in patchAsyncEndpoinsService.UpdateRepoIssuesStreamAsync(owner, repo, githubIssues, cancellationToken))
                    {
                        count++;

                        string result = $"Updated Issue #{issueNumber}";

                        await context.Response.WriteAsync($"{result}\n", cancellationToken);

                        await context.Response.Body.FlushAsync(cancellationToken);

                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }

                    await context.Response.WriteAsync($"----Total Issues Updated: {count}-------\n", cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    context.Response.StatusCode = 499;
                }
                finally
                {
                    cancellationService.Cancel(connectionId);
                }

                return Results.Empty;
            })
            .WithName("PatchAsync_UpdateRepoIssuesStreamAsync")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status499ClientClosedRequest)
            .Produces(StatusCodes.Status500InternalServerError);
        }
    }
}
