namespace Micon.CMS.Models
{
    public class Component: BaseModel
    {
        public Component():base()
        {
            PackageId = Guid.CreateVersion7();
        }

        public Guid PackageId { get; set; }
        
        public List<ComponentRelation> Parents { get; set; } = new List<ComponentRelation>();
        public List<ComponentRelation> Children { get; set; } = new List<ComponentRelation>();
    }
}
