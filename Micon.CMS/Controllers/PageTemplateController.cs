using Micon.CMS.Models;
using Micon.CMS.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;

namespace Micon.CMS.Controllers
{
    public class PageTemplateController(IPageTemplateRepository pageTemplateRepository) : Controller
    {
        public async Task<IActionResult> Index(CancellationToken cancellation)
        {
            var templates = await pageTemplateRepository.GetAllWithCategoryAsync(cancellation);
            return View(templates);
        }

        public async Task<IActionResult> Create( CancellationToken cancellationToken)
        {

            var category = new PageCategory();
            var template = new PageTemplate();
            template.PageCategoryId = category.Id;
            template.PageCategory = category;
            return View(template);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSubmit(PageTemplate pageTemplate,CancellationToken cancellationToken)
        {
            var newPageTemplate = await pageTemplateRepository.CreateAsync(pageTemplate, cancellationToken);

            return RedirectToAction(nameof(Index), nameof(PageTemplateController));
        }
        public IActionResult Edit(Guid Id, CancellationToken cancellationToken)
        {
            var pageTemplate = pageTemplateRepository.GetByIdAsync(Id, cancellationToken);
            if (pageTemplate == null)
            {
                return RedirectToAction(nameof(Index), nameof(PageTemplateController));
            }
            return View();
        }
    }
}
