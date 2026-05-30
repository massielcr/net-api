using System.Text.Json.Serialization;

namespace HttpClientMethods.Dtos
{
    public class UpdateRepoTopicsRequestDto
    {
        [JsonPropertyName("names")]
        public List<string> Names { get; set; } = [];
    }
}
