using System.Text.Json.Serialization;

namespace HttpClientMethods.Dtos
{
    public class CreatePersonalRepoIssueRequestDto
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;
    }
}
