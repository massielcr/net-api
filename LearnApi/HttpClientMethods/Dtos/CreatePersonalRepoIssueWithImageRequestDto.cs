using System.Text.Json.Serialization;

namespace HttpClientMethods.Dtos
{
    public class CreatePersonalRepoIssueWithImageRequestDto
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("image")]
        public string ImageFileName { get; set; } = string.Empty;
    }
}
