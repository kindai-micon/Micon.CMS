using System.ComponentModel.DataAnnotations.Schema;

namespace Micon.CMS.Models
{
    public class PageTemplateHistory
    {
        [ForeignKey(nameof(PageTemplate))]
        public Guid PageTemplateId { get; set; }
        public PageTemplate PageTemplate { get; set; }

        [ForeignKey(nameof(ApplicationUser))]
        public Guid ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public DateTime Modified { get; set; } = DateTime.Now;
    }
}
