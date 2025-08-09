using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Micon.CMS.Models
{
    public class ComponentSetting: BaseModel
    {
        [ForeignKey(nameof(Page))]
        public Guid PageId { get; set; }
        public Page Page { get; set; }

        [ForeignKey(nameof(PageTemplate))]
        public Guid PageTemplateId { get; set; }
        public PageTemplate PageTemplate { get; set; }

        [Required]
        [ForeignKey(nameof(ComponentRelation))]
        public Guid ComponentRelationId { get; set; }
        public ComponentRelation ComponentRelation { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Type { get; set; } 
    }
}
