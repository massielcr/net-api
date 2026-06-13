using HttpClientMethods.Dtos;
using System.Text.Json.Serialization;

[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(PosterDto))]
[JsonSerializable(typeof(GitHubAvatarResponseDto))]
[JsonSerializable(typeof(IEnumerable<GitHubCommitDto>))]
[JsonSerializable(typeof(CommitsSummaryResponseDto))]
[JsonSerializable(typeof(ErrorResponse))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}