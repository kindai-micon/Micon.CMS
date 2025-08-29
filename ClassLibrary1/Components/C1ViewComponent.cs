using Microsoft.AspNetCore.Mvc;
using Micon.CMS.Library.Models.Form;
using System.Threading.Tasks;
using Micon.CMS.Library.Services;

namespace ClassLibrary1.Components
{
    public class C1ViewComponent(ITestService testService) : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(PageComponentViewModel model)
        {
            testService.Hello();
            return View(model);
        }
    }
}
