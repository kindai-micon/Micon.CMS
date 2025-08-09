using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Micon.CMS.Models
{
    public class ComponentSetting: BaseModel
    {
        [ForeignKey(nameof(Page))]
        public Guid PageId { get; set; }
        public Page Page { get; set; }


        [Required]
        [ForeignKey(nameof(Component))]
        public Guid ComponentId { get; set; }
        public Component Component { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Type { get; set; } 
    }
}
