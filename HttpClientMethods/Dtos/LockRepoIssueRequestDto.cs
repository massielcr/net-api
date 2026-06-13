using System.Text.Json.Serialization;

namespace HttpClientMethods.Dtos
{
    public class LockRepoIssueRequestDto
    {
        [JsonPropertyName("issue_number")]
        public int IssueNumber { get; set; }

        [JsonPropertyName("lock_reason")]
        public string LockReason { get; set; } = string.Empty;
    }
}
