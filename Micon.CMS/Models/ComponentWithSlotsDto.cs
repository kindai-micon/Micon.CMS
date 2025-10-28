namespace Micon.CMS.Models
{
    /// <summary>
    /// コンポーネント情報とそのスロット情報を含むDTO
    /// </summary>
    public class ComponentWithSlotsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid PackageId { get; set; }
        public List<string> Slots { get; set; } = new List<string>();
    }
}
