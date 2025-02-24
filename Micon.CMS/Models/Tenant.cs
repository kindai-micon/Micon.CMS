using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Micon.CMS.Models
{
    public class Tenant
    {
        [Key]
        public Guid Id { get; set; }
        [Column(TypeName = "varchar")]
        public string TenantName { get; set; }
    }
}
