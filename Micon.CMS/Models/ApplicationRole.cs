using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Micon.CMS.Models
{
    public class ApplicationRole:IdentityRole<Guid>
    {
        public ApplicationRole()
        {
            Id = Guid.NewGuid();
        }
        public ApplicationRole(string roleName) : this()
        {
            Name = roleName;
        }

        [ForeignKey(nameof(Tenant))]
        public Guid TenantId { get; set; } 
        public Tenant Tenant { get; init; }
    }
}
