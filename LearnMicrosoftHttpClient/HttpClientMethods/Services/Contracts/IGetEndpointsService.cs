namespace HttpClientMethods.Services
{
    public interface IGetEndpointsService
    {
        Task<IEnumerable<string>> GetRepositoriesStringAsync(string username);
    }
}
