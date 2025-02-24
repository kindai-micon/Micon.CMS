using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Micon.CMS.Models
{
    public class ApplicationUser:IdentityUser<Guid>
    {
        public ApplicationUser()
        {
            Id = Guid.NewGuid();
            SecurityStamp = Guid.NewGuid().ToString();
        }

        public ApplicationUser(string userName) : this()
        {
            UserName = userName;
        }
        [ForeignKey(nameof(Tenant))]
        public Guid TenantKey { get; set; }
        public Tenant Tenant { get; init; }
    }
}
