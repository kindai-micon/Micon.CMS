namespace Micon.CMS.Models
{
    public class ComponentHierarchy
    {
        public Guid Id { get; set; }
        public Guid ParentId { get; set; }
        public Guid ChildId { get; set; }
        public int Order { get; set; }
        public int Level { get; set; }

        // 親コンポーネント情報
        public Component Parent { get; set; }
        // 子コンポーネント情報
        public Component Child { get; set; }
    }
}
