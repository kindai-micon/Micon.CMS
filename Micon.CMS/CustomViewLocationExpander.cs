using Microsoft.AspNetCore.Mvc.Razor;

namespace Micon.CMS
{
    public class CustomViewLocationExpander : IViewLocationExpander
    {
        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            var theme = context.Values["theme"] ?? "Default";
            var newLocations = viewLocations.Select(location =>
    location.Replace("/Views/", $"/Themes/{theme}/Views/")).ToList();

            // Layout の場所も動的に変更
            newLocations.Add($"/Themes/{theme}/Views/Shared/_Layout.cshtml");
            return newLocations;
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            context.Values["theme"] = "Modern";
        }
    }
}
