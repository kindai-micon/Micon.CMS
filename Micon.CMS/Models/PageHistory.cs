using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Micon.CMS.Models
{
    public class PageHistory:BaseModel
    {
        [Required]
        [ForeignKey(nameof(Page))]
        public Guid PageId { get; set; }
        public Page Page { get; set; }
        
        [Required]
        [ForeignKey(nameof(ApplicationUser))]
        public Guid ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }
}
