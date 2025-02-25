using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Micon.CMS.Models
{
    public class PageTemplateHistory:BaseModel
    {
        [ForeignKey(nameof(PageTemplate))]
        public Guid PageTemplateId { get; set; }
        [Required]
        public PageTemplate PageTemplate { get; set; }
        [Required]
        [ForeignKey(nameof(ApplicationUser))]
        public Guid ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        
    }
}
