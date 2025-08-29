using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Micon.CMS.Models
{
    public class PageTemplate : BaseModel
    {
        [Required]
        [StringLength(256)]
        public string Name { get; set; }
        public List<PageCategory> PageCategories { get; set; }
        public List<Page> Pages { get; set; }
        public List<PageTemplateHistory> PageTemplateHistories { get; set; }
        [ForeignKey(nameof(ComponentRelation))]
        public Guid? ComponentRelationId { get; set; }
        public ComponentRelation? ComponentRelation { get; set; }
    }
}
