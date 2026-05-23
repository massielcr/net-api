using HttpClientMethods.Dtos;
using System.Text.Json.Serialization;

[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(GitHubAvatarResponseDto))]
[JsonSerializable(typeof(IEnumerable<CommitSummaryDto>))]
[JsonSerializable(typeof(ErrorResponse))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}