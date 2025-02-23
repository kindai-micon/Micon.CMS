using System.Diagnostics;
using Micon.CMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;

namespace Micon.CMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRazorViewEngine _viewEngine;
        public HomeController(IRazorViewEngine viewEngine, ILogger<HomeController> logger)
        {
            _viewEngine = viewEngine;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            //var result = _viewEngine.GetView(null, "test/index1.cshtml",false);
            //if (!result.Success)
            //{
                
            //}
            //var layoutPath = "/Plugins/test/_Layout.cshtml"; // 既存の _Layout.cshtml のパス
            //var viewDictionary = new ViewDataDictionary(ViewData)
            //{
            //    ["Message"] = "これは動的に読み込んだビューです！",
            //    ["Layout"] = layoutPath // レイアウトの設定
            //};
            //using var sw = new StringWriter();
            //var viewContext = new ViewContext(
            //    ControllerContext,
            //    result.View,
            //    viewDictionary,
            //    TempData,
            //    sw,
            //    new HtmlHelperOptions()
            //)
            //{
            //    // レイアウトビューの指定
            //    ExecutingFilePath = layoutPath
            //}; ;
            //await result.View.RenderAsync(viewContext);
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
