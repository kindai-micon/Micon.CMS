namespace Micon.CMS.Models
{
    public class ComponentHierarchy
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public Guid ChildId { get; set; }
        public int Order { get; set; }
        public int Level { get; set; }
        public Guid? ParentComponentId { get; set; }
        public string? ParentComponentName { get; set; }
        public Guid ? ParentComponentPackageId { get; set; }
        public Guid ChildComponentId { get; set; }
        public string ChildComponentName { get; set; } = string.Empty;
        public Guid ChildComponentPackageId { get; set; }

        // ナビゲーションプロパティ（必要に応じて）
        public Component? Parent { get; set; }
        public Component Child { get; set; } = null!;
    }
}
