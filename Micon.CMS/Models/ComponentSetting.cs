using System.ComponentModel.DataAnnotations.Schema;

namespace Micon.CMS.Models
{
    public class ComponentSetting: BaseModel
    {
        [ForeignKey(nameof(Page))]
        public Guid PageId { get; set; }
        public Page Page { get; set; }
        [ForeignKey(nameof(ComponentRelation))]
        public Guid ComponentRelationId { get; set; }
        public ComponentRelation ComponentRelation { get; set; }
    }
}
