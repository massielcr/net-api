
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WebAPIClient.Dtos;

using HttpClient httpClient = new();

httpClient.DefaultRequestHeaders.Accept.Clear();
httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
httpClient.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");


var repositories = await ProcessRepositoriesAsync(httpClient);

foreach (var repo in repositories)
{
    Console.WriteLine($"Name: {repo.Name}");
    Console.WriteLine($"Homepage: {repo.Homepage}");
    Console.WriteLine($"GitHub: {repo.GitHubHomeUrl}");
    Console.WriteLine($"Description: {repo.Description}");
    Console.WriteLine($"Watchers: {repo.Watchers:#,0}");
    Console.WriteLine($"Last push: {repo.LastPush}");
    Console.WriteLine();
}    


static async Task<List<Repository>> ProcessRepositoriesAsync(HttpClient httpClient)
{
    var repositories = await httpClient.GetFromJsonAsync<List<Repository>>("https://api.github.com/orgs/dotnet/repos");

    return repositories ?? [];    
}






Console.ReadLine();