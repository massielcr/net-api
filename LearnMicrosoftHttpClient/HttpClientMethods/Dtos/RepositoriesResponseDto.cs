namespace HttpClientMethods.Dtos
{
    public class RepositoriesResponseDto
    {
        public IEnumerable<string> Repos { get; set; } = [];
        public double ExecutionTimeMs { get; set; }
    }
}
