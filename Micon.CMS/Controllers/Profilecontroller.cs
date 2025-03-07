using Microsoft.AspNetCore.Mvc;

namespace Micon.CMS.Controllers
{
    public class Profilecontroller : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
