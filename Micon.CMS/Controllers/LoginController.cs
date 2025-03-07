using Microsoft.AspNetCore.Mvc;

namespace Micon.CMS.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
