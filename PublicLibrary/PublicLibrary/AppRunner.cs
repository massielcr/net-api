using PublicLibrary.Repositories;
using PublicLibrary.Services;

namespace PublicLibrary
{
    public class AppRunner(IOfficeHoursService officeHoursService)
    {
        public void Run()
        {
            Console.WriteLine("Hello");

            Console.ReadLine();
        }
    }
}
