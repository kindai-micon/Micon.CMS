using Microsoft.AspNetCore.Mvc;

namespace ClassLibrary1.Components
{
    public class C1ViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
