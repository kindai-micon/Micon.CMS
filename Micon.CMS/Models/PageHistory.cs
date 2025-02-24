using System.ComponentModel.DataAnnotations.Schema;

namespace Micon.CMS.Models
{
    public class PageHistory:BaseModel
    {
        [ForeignKey(nameof(Page))]
        public Guid PageId { get; set; }
        public Page Page { get; set; }

        [ForeignKey(nameof(ApplicationUser))]
        public Guid ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        DateTime Modified { get; set; } = DateTime.Now;
    }
}
