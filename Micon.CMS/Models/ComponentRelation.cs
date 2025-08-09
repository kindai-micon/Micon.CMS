using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Micon.CMS.Models
{
    public class ComponentRelation: BaseModel
    {
        [ForeignKey(nameof(Parent))]
        public Guid ParentId { get; set; }
        public Component Parent { get; set; }
        [Required]
        [ForeignKey(nameof(Child))]
        public Guid ChildId { get; set; }
        public Component? Child { get; set; }
        [Required]
        public int Order { get; set; }
        public List<ComponentSetting> ComponentSettings { get; set; }
        public List<PageTemplate> PageTemplates { get; set; }
        public bool IsPriority { get; set; }

    }
}
