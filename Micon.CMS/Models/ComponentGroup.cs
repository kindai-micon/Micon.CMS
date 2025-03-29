namespace Micon.CMS.Models
{
    public class ComponentGroup:BaseModel
    {
        public string Name { get; set; }
        public Guid ComponentRelationId { get; set; }
        public ComponentRelation ComponentRelation { get; set; }
        public string Description{ get; set; }
    }
}
