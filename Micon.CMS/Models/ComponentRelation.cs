using System.ComponentModel.DataAnnotations.Schema;

namespace Micon.CMS.Models
{
    public class ComponentRelation: BaseModel
    {
        [ForeignKey(nameof(Parent))]
        public Guid ParentId { get; set; }
        public Component Parent { get; set; }

        [ForeignKey(nameof(Child))]
        public Guid ChildId { get; set; }
        public Component Child { get; set; }
        public int Order { get; set; }
        public ComponentSetting ComponentSetting { get; set; }
    }
}
