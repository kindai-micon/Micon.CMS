using Micon.CMS.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.Pkcs;

namespace Micon.CMS
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<ApplicationRole> ApplicationRoles { get; set; }
        public DbSet<Page> Pages { get; set; }
        public DbSet<PageCategory> PageCategories { get; set; }
        public DbSet<PageHistory> PageHistories { get; set; }
        public DbSet<PageTemplate> PageTemplates { get; set; }
        public DbSet<PageTemplateHistory> PageTemplateHistories { get; set; }
        public DbSet<Component> Components { get; set; }
        public DbSet<ComponentRelation> ComponentRelations { get; set; }
        public DbSet<ComponentSetting> ComponentSettings { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
