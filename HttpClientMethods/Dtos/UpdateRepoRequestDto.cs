using System.Text.Json.Serialization;

namespace HttpClientMethods.Dtos
{
    public class UpdateRepoRequestDto
    {
        [JsonPropertyName("visibility")]
        public string Visibility { get; set; } = "public";
    }
}
