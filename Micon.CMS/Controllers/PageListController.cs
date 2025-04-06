using Micon.CMS.Models;
using Microsoft.AspNetCore.Mvc;

namespace Micon.CMS.Controllers
{
    public class PageListController : Controller
    {
        public class ProductController : Controller
        {
            private readonly List<PageTemplate> _products = new List<PageTemplate>
    {
        new PageTemplate { Name = ,PageCategories = , Pages =  ,PageTemplateHistories = },
        new PageTemplate { Id = 2, Name = "Product2", Price = 200 },
        new PageTemplate { Id = 3, Name = "Product3", Price = 300 }
    };
          


            public IActionResult Index()
            {
                return View();
            }
        }
    }
}
