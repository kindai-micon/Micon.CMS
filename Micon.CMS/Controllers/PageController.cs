using Micon.CMS.Models;
using Microsoft.AspNetCore.Mvc;
using Micon.CMS.Repositories;

namespace Micon.CMS.Controllers
{
    public class PageController(IPageRepository pageRepository,PageCategory pageCategory) : Controller
    {
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            if (pageCategory == null) {
                return View();
            }
            return View();
        }
        public async Task<IActionResult> PageList(CancellationToken cancellationToken)
        {
            return View();
        }
    }
}
