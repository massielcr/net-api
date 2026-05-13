using PublicLibrary.Models;

namespace PublicLibrary.Repositories
{
    public class HoursRepository : IHoursRepository
    {
        IReadOnlyList<OpenPeriod> IHoursRepository.GetTodayOpenHours()
        {
            return [];
        }

        IReadOnlyList<OpenPeriod> IHoursRepository.GetTomorrowOpenHours()
        {
            return [];
        }
    }
}
