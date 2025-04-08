using Micon.CMS.Models;
using Micon.CMS.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace Micon.CMS.Controllers
{
    public class PageListController(IPageRepository pageRepository) : Controller
    {
       public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var all = await pageRepository.GetAllAsync(cancellationToken);
            return View(all);
        }
    }
}
