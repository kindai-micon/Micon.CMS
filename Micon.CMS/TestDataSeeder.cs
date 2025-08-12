using Micon.CMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Micon.CMS
{
    public class TestDataSeeder
    {
        private readonly IServiceProvider _serviceProvider;

        public TestDataSeeder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task SeedAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                // 1. Create a new Tenant for testing
                var testTenantName = "TestCorp";
                var testTenant = await context.Tenants.FirstOrDefaultAsync(t => t.TenantName == testTenantName);
                if (testTenant == null)
                {
                    testTenant = new Tenant { TenantName = testTenantName };
                    context.Tenants.Add(testTenant);
                    await context.SaveChangesAsync();
                }

                // 2. Create a new user in the new tenant
                var testUserName = "testuser";
                if (await userManager.FindByNameAsync(testUserName) == null)
                {
                    var testUser = new ApplicationUser(testUserName)
                    {
                        TenantId = testTenant.Id
                    };
                    var result = await userManager.CreateAsync(testUser, "Password123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(testUser, "Admin");
                    }
                }
            }
        }
    }
}
