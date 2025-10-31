namespace Micon.CMS.Models.Api
{
    public class ComponentTreeNode
    {
        public required string ComponentId { get; set; }
        public required string ComponentName { get; set; }
        public required string PackageId { get; set; }
        public required string SlotName { get; set; }
        public List<ComponentTreeNode> Children { get; set; } = new List<ComponentTreeNode>();
    }
}
