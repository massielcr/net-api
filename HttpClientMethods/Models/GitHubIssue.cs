using System.Text.Json.Serialization;

namespace HttpClientMethods.Models
{
    public class GitHubIssue
    {
        public int IssueNumber { get; set; }

        public string State { get; set; } = string.Empty;

        public string StateReason { get; set; } = string.Empty;

        public List<string> Labels { get; set; } = [];

        public string LockReason { get; set; } = string.Empty;
    }
}
