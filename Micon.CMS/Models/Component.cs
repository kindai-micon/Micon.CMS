using System.ComponentModel.DataAnnotations;

namespace Micon.CMS.Models
{
    public class Component: BaseModel
    {
        public Component():base()
        {
            PackageId = Guid.CreateVersion7();
        }
        [Required]
        public Guid PackageId { get; set; }
        public string Name { get; set; }
        public List<ComponentRelation> Parents { get; set; } = new List<ComponentRelation>();
        public List<ComponentRelation> Children { get; set; } = new List<ComponentRelation>();
        public List<ComponentSetting> ComponentSettings { get;set; } = new List<ComponentSetting>();

    }
}
