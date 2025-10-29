using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;
using Micon.CMS.Library.Models.Form;
using System.Reflection;

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
                    return;
                }

                // ViewComponent のアセンブリから MiconCmsSettings.PackageId を取得
                var model = ViewContext?.ViewData.Model;

                if (model == null)
                {
                    return;
                }

                // Model が PageComponentViewModel であることを確認
                var pageComponentViewModel = model as PageComponentViewModel;
                if (pageComponentViewModel == null)
                {
                    return;
                }

                var packageId = pageComponentViewModel.PackageId;
                var componentName = pageComponentViewModel.ComponentName;

                if (!packageId.HasValue || packageId.Value == Guid.Empty)
                {
                    return;
                }

                if (string.IsNullOrEmpty(componentName))
                {
                    return;
                }

                // ComponentName から名前空間を除去（最後の . 以降を取得）
                var simpleComponentName = componentName.Split('.').Last();

                var packageIdStr = packageId.Value.ToString("N");

                // 複数のクラス名に対応（スペース区切り）
                var classNames = ScopedClassName.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                var scopedClasses = new List<string>();

                foreach (var className in classNames)
                {
                    var trimmedClassName = className.Trim();
                    if (!string.IsNullOrEmpty(trimmedClassName))
                    {
                        // スコープ付きクラス名を生成（simpleComponentName を使用）
                        var scopedClass = $"{packageIdStr}_{simpleComponentName}_{trimmedClassName}";
                        scopedClasses.Add(scopedClass);
                    }
                }

                if (scopedClasses.Count == 0)
                {
                    _logger.LogWarning("No valid class names found after splitting");
                    return;
                }

                // class 属性に追加（既存のクラスとマージ）
                var allScopedClasses = string.Join(" ", scopedClasses);
                if (output.Attributes.ContainsName("class"))
                {
                    var existingClass = output.Attributes["class"].Value?.ToString() ?? string.Empty;
                    var mergedClass = string.IsNullOrWhiteSpace(existingClass)
                        ? allScopedClasses
                        : $"{allScopedClasses} {existingClass}";
                    output.Attributes.SetAttribute("class", mergedClass);
                }
                else
                {
                    output.Attributes.SetAttribute("class", allScopedClasses);
                }
            }
            catch (Exception ex)
            {
                // エラーが発生してもタグ出力は続行
            }
        }
    }
}
