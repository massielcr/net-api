using HttpClientMethods.Dtos;
using System.Text.Json.Serialization;

[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(GitHubAvatarResponseDto))]
[JsonSerializable(typeof(IEnumerable<(string commitMessage, DateTime commitDate)>))]
[JsonSerializable(typeof(ErrorResponse))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}