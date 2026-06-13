
using HttpClientMethods.Dtos;
using PluralsightKDStreams.Dtos;
using System.Text.Json.Serialization;

[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(PosterDto))]
[JsonSerializable(typeof(ErrorResponse))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}