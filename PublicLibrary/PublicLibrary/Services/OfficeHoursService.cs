using PublicLibrary.Models;
using PublicLibrary.Repositories;

namespace PublicLibrary.Services
{
    public class OfficeHoursService(IHoursRepository hoursRepository) : IOfficeHoursService
    {
        public IReadOnlyList<OpenPeriod> OpenHoursToday { get; private init; } = hoursRepository.GetTodayOpenHours() ?? [];
        public IReadOnlyList<OpenPeriod> OpenHoursTomorrow { get; private init; } = hoursRepository.GetTomorrowOpenHours() ?? [];

        public TimeSpan GetTotalHoursToday()
        {
            IEnumerable<TimeSpan> openTimeSpans = OpenHoursToday.Select(h => h.CloseTime - h.OpenTime);

            return SumTimeSpans(openTimeSpans);
        }

        private static TimeSpan SumTimeSpans(IEnumerable<TimeSpan> sequence)
        {
            TimeSpan result = new();

            foreach (var time in sequence)
            {
                result += time;
            }

            return result;
        }
    }
}
