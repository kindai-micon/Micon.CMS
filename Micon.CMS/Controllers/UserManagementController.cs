using Microsoft.AspNetCore.Mvc;

namespace Micon.CMS.Controllers
{
    public class UserManagementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult User([FromRoute]string id)
        {
            return View();
        }
    }
}
