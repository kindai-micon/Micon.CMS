using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Micon.CMS.Models
{
    public class BaseModel
    {
        public BaseModel()
        {
            Id = Guid.CreateVersion7();
        }
        [Key]
        public Guid Id { get; set; }
        
        [ForeignKey(nameof(Tenant))]
        public Guid TenantId { get; set; }
        public Tenant Tenant { get;set; }
        [Required]
        public DateTimeOffset Modified { get; set; } = DateTimeOffset.Now;
        [Required]
        public DateTimeOffset Created { get; set; } = DateTimeOffset.Now;

    }
}
