using HttpClientMethods.Data.Entities;

namespace HttpClientMethods.Data.Services
{
    internal interface IRepoRepository
    {
        public Repo GetRepo(int id);
        public IEnumerable<Repo> GetRepos();

        public bool Create(Repo repo);
        public bool Update(Repo repo);
        public void Delete(Repo repo);        
    }
}
