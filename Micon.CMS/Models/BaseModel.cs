using System.ComponentModel.DataAnnotations.Schema;

namespace Micon.CMS.Models
{
    public class BaseModel
    {
        public BaseModel()
        {
            Id = Guid.CreateVersion7();
        }
        public Guid Id { get; set; }
        
        [ForeignKey(nameof(Tenant))]
        public Guid TenantId { get; set; }
        public Tenant Tenant { get;set; }

    }
}
