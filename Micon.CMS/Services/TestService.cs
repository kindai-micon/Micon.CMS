using Micon.CMS.Library.Services;

namespace Micon.CMS.Services
{
    public class TestService:ITestService
    {
        public void Hello()
        {
            Console.WriteLine("Hello from TestService in Micon.CMS.Services");
        }
    }
}