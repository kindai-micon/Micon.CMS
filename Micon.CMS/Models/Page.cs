using System.ComponentModel.DataAnnotations.Schema;

namespace Micon.CMS.Models
{
    public class Page : BaseModel
    {
        [Column(TypeName = "varchar(256)")]
        public string Title { get;set; }

        [ForeignKey(nameof(PageTemplate))]
        public Guid PageTemplateId { get; set; }
        public PageTemplate PageTemplate { get; set; }
        public List<PageHistory> PageHistories { get; set; }
    }
}
