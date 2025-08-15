using System.ComponentModel.DataAnnotations.Schema;

namespace Micon.CMS.Models
{
    public class Authority : BaseModel
    {
        public string Name { get; set; }
        [ForeignKey(nameof(Role))]
        public Guid RoleId { get; set; }
        public ApplicationRole Role { get; set; }
    }
}
