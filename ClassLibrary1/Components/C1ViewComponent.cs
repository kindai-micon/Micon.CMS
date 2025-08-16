using Microsoft.AspNetCore.Mvc;
using Micon.CMS.Library.Models.Form;
using System.Threading.Tasks;

namespace ClassLibrary1.Components
{
    public class C1ViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(PageComponentViewModel model)
        {
            return View(model);
        }
    }
}
