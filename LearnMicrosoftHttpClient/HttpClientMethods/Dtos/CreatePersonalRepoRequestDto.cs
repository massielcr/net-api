using Microsoft.AspNetCore.Mvc;

namespace HttpClientMethods.Dtos
{
    public class CreatePersonalRepoRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsPrivate { get; set; }
        public bool InitialCommit { get; set; }
    }
}
