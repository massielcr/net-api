using PublicLibrary.Models;

namespace PublicLibrary.Tests
{
    public class OfficeHoursServiceUnitTests_TestCaseSource()
    {
        public static IEnumerable<TestCaseData> GetTotalHoursToday_Success_Source()
        {
            TimeSpan expectedResult = new(8, 0, 0);

            IReadOnlyList<OpenPeriod> weekdayHours = new List<OpenPeriod>()
            {
                new OpenPeriod(new TimeOnly(8, 30), new TimeOnly(12, 0)),
                new OpenPeriod(new TimeOnly(13, 0), new TimeOnly(17, 30))
            }.AsReadOnly(); ;

            yield return new TestCaseData(weekdayHours, expectedResult);
        }
    }
}
