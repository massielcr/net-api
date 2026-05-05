namespace HttpClientMethods.Services
{
    public interface IGetEndpointsService
    {
        Task<int> GetRepositoriesCountAsync();
        Task<IEnumerable<string>> GetAllRepositoriesAsync();
    }
}
