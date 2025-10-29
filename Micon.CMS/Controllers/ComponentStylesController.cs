using Microsoft.AspNetCore.Mvc;
using Micon.CMS.Library.Services;

namespace Micon.CMS.Controllers
{
    /// <summary>
    /// Component スタイルのエンドポイント
    /// すべてのコンポーネント CSS を集約して提供
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ComponentStylesController : ControllerBase
    {
        private readonly CssService _cssService;
        private readonly ILogger<ComponentStylesController> _logger;

        public ComponentStylesController(
            CssService cssService,
            ILogger<ComponentStylesController> logger)
        {
            _cssService = cssService;
            _logger = logger;
        }

        /// <summary>
        /// すべてのコンポーネント CSS を取得
        /// </summary>
        [HttpGet("all")]
        public IActionResult GetAllStyles()
        {
            try
            {
                var cssContent = _cssService.GetAllCssContent();

                if (string.IsNullOrEmpty(cssContent))
                {
                    _logger.LogWarning("No CSS content found");
                    return Ok("/* No CSS content available */");
                }

                // Content-Type を text/css に設定
                return Content(cssContent, "text/css");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting component styles: {ex.Message}");
                return StatusCode(500, "Error retrieving component styles");
            }
        }
    }
}
