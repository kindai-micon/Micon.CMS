using Microsoft.AspNetCore.Mvc;

namespace ClassLibrary1.Components.Test
{
    public class TestViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            Console.WriteLine("test");
            return View("TestView");
        }
    }
}
