using Micon.CMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Micon.CMS
{
    public static class IdentityDataInitializer
    {
        public static async Task SeedData(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            // Add a default role
            string roleName = "Admin";
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole(roleName));
            }

            // Get Initial User Config
            var initialUserConfig = configuration.GetSection("InitialUser");
            var organizationName = initialUserConfig["Organization"];
            var userName = initialUserConfig["UserName"];
            var password = initialUserConfig["Password"];

            // Ensure Tenant exists
            var tenant = await context.Tenants.FirstOrDefaultAsync(t => t.TenantName == organizationName);
            if (tenant == null)
            {
                tenant = new Tenant { TenantName = organizationName };
                context.Tenants.Add(tenant);
                await context.SaveChangesAsync();
            }

            // Add a default user if not exists
            if (await userManager.FindByNameAsync(userName) == null)
            {
                ApplicationUser user = new ApplicationUser(userName)
                {
                    TenantId = tenant.Id
                };

                IdentityResult result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, roleName);
                }
            }
        }
    }
}
