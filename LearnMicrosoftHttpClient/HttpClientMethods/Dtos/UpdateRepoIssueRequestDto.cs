using System.Text.Json.Serialization;

namespace HttpClientMethods.Dtos
{
    public class UpdateRepoIssueRequestDto
    {
        [JsonPropertyName("issue_number")]
        public int IssueNumber { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; } = string.Empty;

        [JsonPropertyName("state_reason")]
        public string StateReason { get; set; } = string.Empty;

        [JsonPropertyName("labels")]
        public List<string> Labels { get; set; } = [];
    }
}
