namespace Micon.CMS.Models
{
    public class ComponentNode
    {
        public ComponentNode()
        {

        }
        List<ComponentNode> Children { get; set; }
        string ComponentName { get; set; }
        Guid ComponentId
    }
}
