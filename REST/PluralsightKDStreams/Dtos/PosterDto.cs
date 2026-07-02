using System.Text.Json.Serialization;

namespace PluralsightKDStreams.Dtos
{
    public class PosterDto(string id, string description, byte[] data)
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; } = id;

        [JsonPropertyName("description")]
        public string Description { get; set; } = description;

        [JsonPropertyName("data")]
        public byte[]? Data { get; set; } = data;

        public PosterDto() : this(default!, default!, default!) { }
    }
}
