using Microsoft.AspNetCore.Mvc;

namespace ClassLibrary1.Components
{
    public class C2ViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
