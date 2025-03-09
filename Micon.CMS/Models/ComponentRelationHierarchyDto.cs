namespace Micon.CMS.Models
{
    public class ComponentRelationHierarchyDto
    {
        public Guid ComponentId { get; set; }
        public Guid? RelationId { get; set; }
        public Guid? ParentId { get; set; }
        public int Level { get; set; }
        public int Order { get; set; }
        public Guid PackageId { get; set; }

        // 追加情報を格納するプロパティ
        public Component Component { get; set; }
        public ComponentRelation Relation { get; set; }
    }
}
