using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Micon.CMS.Library.Models.Form;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Micon.CMS.Library.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static async Task<IHtmlContent> LoadChildComponent(this IHtmlHelper htmlHelper, string slotName)
        {
            var viewModel = htmlHelper.ViewData.Model as PageComponentViewModel;
            if (viewModel == null)
            {
                throw new InvalidOperationException("LoadChildComponent can only be used within a view that has a PageComponentViewModel as its model.");
            }

            var childComponent = viewModel.Children.FirstOrDefault(c => c.SlotName == slotName);

            if (childComponent != null)
            {
                // Note: This still depends on "_RenderComponent". This partial view needs to be accessible.
                // For a true plugin architecture, a different rendering mechanism might be needed,
                // but for now, we'll assume the main app provides this partial.
                return await htmlHelper.PartialAsync("_RenderComponent", childComponent);
            }

            return HtmlString.Empty;
        }
    }
}
