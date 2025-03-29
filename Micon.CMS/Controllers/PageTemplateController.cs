using Microsoft.AspNetCore.Mvc;

namespace Micon.CMS.Controllers
{
    public class PageTemplateController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
