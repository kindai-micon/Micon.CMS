namespace Micon.CMS.Models
{
    public class Component: BaseModel
    {
        public Component():base()
        {
            PackageId = Guid.CreateVersion7();
        }

        public Guid PackageId { get; set; }
    }
}
