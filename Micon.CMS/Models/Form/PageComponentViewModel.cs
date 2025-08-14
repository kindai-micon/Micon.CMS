using System.Collections.Generic;

namespace Micon.CMS.Models.Form
{
    public class PageComponentViewModel
    {
        public string ComponentName { get; set; }
        public Dictionary<string, string> Settings { get; set; }
        public List<PageComponentViewModel> Children { get; set; }

        public PageComponentViewModel()
        {
            Children = new List<PageComponentViewModel>();
            Settings = new Dictionary<string, string>();
        }
    }
}
