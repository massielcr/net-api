using System.Text.Json.Serialization;

namespace HttpClientMethods.Dtos
{
    public class LockRepoIssuesRequestDto
    {
        [JsonPropertyName("issues")]
        public List<LockRepoIssueRequestDto> Issues { get; set; } = [];
    }
}
