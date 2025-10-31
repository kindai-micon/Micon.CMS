using Microsoft.AspNetCore.Mvc;
using Micon.CMS.Library.Models.Form;

namespace ClassLibrary1.Components
{
    public class C2ViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(PageComponentViewModel model)
        {
            return View(model);
        }
    }
}
