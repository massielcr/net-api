using Moq;
using PublicLibrary.Models;
using PublicLibrary.Repositories;
using PublicLibrary.Services;

namespace PublicLibrary.Tests
{
    public class OfficeHoursServiceUnitTests
    {
        [Test]
        [TestCaseSource(typeof(OfficeHoursServiceUnitTests_TestCaseSource), 
                        nameof(OfficeHoursServiceUnitTests_TestCaseSource.GetTotalHoursToday_Success_Source))]
        public void GetTotalHoursToday_Success(IReadOnlyList<OpenPeriod> todayOpenHours, TimeSpan expectedResult)
        {
            Mock<IHoursRepository> _hoursRepository = new();
                                   _hoursRepository.Setup(hr => hr.GetTodayOpenHours()).Returns(todayOpenHours);

            var sut = new OfficeHoursService(_hoursRepository.Object);


            TimeSpan actualResult = sut.GetTotalHoursToday();


            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }
    }
}
