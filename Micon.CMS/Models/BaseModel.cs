using System.ComponentModel.DataAnnotations.Schema;

namespace Micon.CMS.Models
{
    public class BaseModel
    {
        [ForeignKey(nameof(Tenant))]
        public Guid TenantId { get; set; }
        public Tenant Tenant { get;set; }

    }
}
