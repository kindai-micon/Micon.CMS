using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;
using Micon.CMS.Library.Models.Form;

namespace Micon.CMS.Library.TagHelpers
{
    /// <summary>
    /// asp-scoped-css Tag Helper
    /// 指定されたクラス名に対して、Model の PackageId と ComponentId をプレフィックスとして付与
    ///
    /// 使用例:
    /// <div asp-scoped-css="my-component">
    ///   ...
    /// </div>
    ///
    /// 出力例:
    /// <div class="pkg-xxx_cmp-yyy_my-component">
    ///   ...
    /// </div>
    /// </summary>
    [HtmlTargetElement("*", Attributes = "asp-scoped-css")]
    public class ScopedCssTagHelper : TagHelper
    {
        private readonly ILogger<ScopedCssTagHelper> _logger;

        [HtmlAttributeName("asp-scoped-css")]
        public string ScopedClassName { get; set; } = string.Empty;

        [ViewContext]
        public ViewContext? ViewContext { get; set; }

        public ScopedCssTagHelper(ILogger<ScopedCssTagHelper> logger)
        {
            _logger = logger;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            try
            {
                if (string.IsNullOrEmpty(ScopedClassName))
                {
                    _logger.LogWarning("asp-scoped-css attribute is empty");
                    return;
                }

                // @Model から ComponentId と PackageId を取得
                var model = ViewContext?.ViewData.Model;
                if (model == null)
                {
                    _logger.LogWarning("Model is null in ScopedCssTagHelper");
                    return;
                }

                // PageComponentViewModel を想定
                if (model is not PageComponentViewModel pageComponentViewModel)
                {
                    _logger.LogWarning($"Model is not PageComponentViewModel: {model.GetType().Name}");
                    return;
                }

                var packageId = pageComponentViewModel.PackageId;
                var componentName = pageComponentViewModel.ComponentName;

                if (!packageId.HasValue || packageId.Value == Guid.Empty || string.IsNullOrEmpty(componentName))
                {
                    _logger.LogWarning("PackageId or ComponentName is empty");
                    return;
                }

                // スコープ用のクラス名を生成
                // 形式: {packageId}_{ComponentName}_{specifiedClassName}
                var packageIdStr = packageId.Value.ToString("N");

                // スコープ付きクラス名を生成
                var scopedClass = $"{packageIdStr}_{componentName}_{ScopedClassName}";

                // class 属性に追加
                if (output.Attributes.ContainsName("class"))
                {
                    var existingClass = output.Attributes["class"].Value?.ToString() ?? string.Empty;
                    output.Attributes.SetAttribute("class", $"{scopedClass} {existingClass}");
                }
                else
                {
                    output.Attributes.SetAttribute("class", scopedClass);
                }

                _logger.LogInformation($"Applied scoped CSS class: {scopedClass}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ScopedCssTagHelper: {ex.Message}");
                // エラーが発生してもタグ出力は続行
            }
        }
    }
}
