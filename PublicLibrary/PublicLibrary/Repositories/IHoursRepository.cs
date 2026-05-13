using PublicLibrary.Models;

namespace PublicLibrary.Repositories
{
    public interface IHoursRepository
    {
        public IReadOnlyList<OpenPeriod> GetTodayOpenHours();
        public IReadOnlyList<OpenPeriod> GetTomorrowOpenHours();
    }
}
