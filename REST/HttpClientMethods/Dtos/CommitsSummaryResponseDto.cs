namespace HttpClientMethods.Dtos
{
    public class CommitsSummaryResponseDto
    {
        public List<string> Commits { get; set; } = [];
        public int Total { get; set; }
    }
}
