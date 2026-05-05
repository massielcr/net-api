namespace HttpClientMethods.Dtos
{
    public class CommitResponseDto
    {
        List<(string commitMessage, DateTime commitDate)> Commits { get; set; }
        public int Total { get; set; }
    }
}
