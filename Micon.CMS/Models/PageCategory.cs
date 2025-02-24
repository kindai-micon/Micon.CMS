using System.ComponentModel.DataAnnotations.Schema;

namespace Micon.CMS.Models
{
    public class PageCategory : BaseModel
    {
        [Column(TypeName = "varchar(256)")]
        public string Name { get;set; }

        [ForeignKey(nameof(PageTemplate))]
        public Guid PageTemplateId { get; set; }
        public PageTemplate PageTemplate { get; set; }
    }
}
