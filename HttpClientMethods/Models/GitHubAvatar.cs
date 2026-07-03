namespace HttpClientMethods.Models
{
    public record GitHubAvatar 
    {
        public byte[]? Data { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public int ContentLength { get; set; }
    }
}
