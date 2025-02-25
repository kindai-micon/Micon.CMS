using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Micon.CMS.Models
{
    public class ApplicationUser:IdentityUser<Guid>
    {
        public ApplicationUser()
        {
            Id = Guid.CreateVersion7();
            SecurityStamp = Guid.CreateVersion7().ToString();
        }

        public ApplicationUser(string userName) : this()
        {
            UserName = userName;
        }

        [ForeignKey(nameof(Tenant))]
        public Guid TenantId { get; set; }
        public Tenant Tenant { get; init; }
    }
}
