using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Micon.CMS.Models
{
    public class Page : BaseModel
    {
        [Required]
        [StringLength(256)]
        public string Title { get;set; }
        [Required]
        [ForeignKey(nameof(PageTemplate))]
        public Guid PageTemplateId { get; set; }
        public PageTemplate PageTemplate { get; set; }
        public List<PageHistory> PageHistories { get; set; }
    }
}
