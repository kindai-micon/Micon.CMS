using System;
using System.Collections.Generic;

namespace Micon.CMS.Library.Models.Form
{
    public class PageComponentViewModel
    {
        public Guid ComponentId { get; set; }
        public string ComponentName { get; set; }
        public string SlotName { get; set; }
        public Guid? PackageId { get; set; }
        public Dictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();
        public List<PageComponentViewModel> Children { get; set; } = new List<PageComponentViewModel>();
    }
}
