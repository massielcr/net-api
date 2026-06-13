using System.Text.Json.Serialization;

namespace HttpClientMethods.Dtos
{
    public class CreatePersonalRepositoryRequestDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("private")]
        public bool IsPrivate { get; set; }

        [JsonPropertyName("auto_init")]
        public bool InitialREADME { get; set; }

        [JsonPropertyName("has_downloads")]
        public bool HasDownloads { get; set; }
    }
}
