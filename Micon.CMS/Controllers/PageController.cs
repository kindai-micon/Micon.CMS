using Microsoft.AspNetCore.Mvc;
using Micon.CMS.Models;
using Micon.CMS.Repositories;

namespace Micon.CMS.Controllers
{
    public class PageController(IPageRepository PageRepository) : Controller
    {
        public async Task<IActionResult> Index(Guid pageCategoryId, CancellationToken cancellationToken)
        {
            var pages = await PageRepository.GetPagesByCategoryIdAsync(pageCategoryId, cancellationToken);
            return View(pages);
        }
        public async Task<IActionResult> Delete(Guid Id, CancellationToken cancellationToken)
        {
            var pages = await PageRepository.GetByIdAsync(Id, cancellationToken);
            if (pages != null)
            {
                await PageRepository.DeleteAsync(pages, cancellationToken);
            }
            return RedirectToAction("Index","Page");
        }
        public async Task<IActionResult> Create(CancellationToken cancellationToken)
        {
            return View();
        }
        public async Task<IActionResult> Edit(Guid Id, CancellationToken cancellationToken)
        {
            var pages = await PageRepository.GetByIdAsync(Id, cancellationToken);
            return View(pages);
        }
        public async Task<IActionResult> Copy(CancellationToken cancellationToken)
        {
            return View();
        }
        public async Task<IActionResult> Open(CancellationToken cancellationToken)
        {
            return View();
        }
        public async Task<IActionResult> Close(PageCategory pageCategory, CancellationToken cancellationToken)
        {
            return View();
        }
    }
}
