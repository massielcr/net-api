using System.Text.Json.Serialization;

namespace HttpClientMethods.Dtos
{
    public class UpdateRepoIssuesRequestDto
    {
        [JsonPropertyName("issues")]
        public List<UpdateRepoIssueRequestDto> Issues { get; set; } = [];
    }
}
