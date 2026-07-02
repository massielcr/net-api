
using HttpClientMethods.Dtos;
using Microsoft.AspNetCore.Mvc;
using PluralsightKDStreams.Dtos;
using System.Text.Json.Serialization;

[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(PosterDto))]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(ProblemDetails))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}