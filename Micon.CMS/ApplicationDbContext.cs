using Micon.CMS.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;
using System.Linq;
using System.Security.Cryptography.Pkcs;

namespace Micon.CMS
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public ApplicationDbContext():base()
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

            modelBuilder.Entity<Tenant>(builder =>
            {
                builder.HasIndex(Tenant => Tenant.TenantName)
                    .IsUnique();
                builder.HasIndex(Tenant => Tenant.Id);
            });

            modelBuilder.Entity<Page>(builder =>
            {
                builder.HasIndex(Page => Page.Id);
                builder.HasIndex(Page => Page.TenantId);
                builder.HasIndex(Page => Page.PageTemplateId);
                builder.HasOne(t => t.PageTemplate)
                    .WithMany(t => t.Pages)
                    .HasForeignKey(t => t.PageTemplateId);
            });

            modelBuilder.Entity<PageTemplate>(builder =>
            {
                builder.HasIndex(PageTemplate => PageTemplate.Id);
                builder.HasIndex(PageTemplate => PageTemplate.TenantId);
                builder.HasMany(t => t.PageCategories)
                    .WithOne(t => t.PageTemplate)
                    .HasForeignKey(t => t.PageTemplateId);
            });

            modelBuilder.Entity<PageTemplateHistory>(builder =>
            {
                builder.HasOne(t => t.PageTemplate)
                    .WithMany(t => t.PageTemplateHistories)
                    .HasForeignKey(t => t.PageTemplateId);
            });

            modelBuilder.Entity<PageCategory>(builder =>
            {
                builder.HasIndex(PageCategory => PageCategory.Id);
                builder.HasIndex(PageCategory => PageCategory.TenantId);
            });

            modelBuilder.Entity<PageHistory>(builder =>
            {
                builder.HasIndex(PageHistory => PageHistory.Id);
                builder.HasIndex(PageHistory => PageHistory.TenantId);
                builder.HasIndex(PageHistory => PageHistory.PageId);
                builder.HasOne(t => t.Page)
                    .WithMany(t => t.PageHistories)
                    .HasForeignKey(t => t.PageId);
            });

            modelBuilder.Entity<ComponentRelation>(builder =>
            {
                builder.HasIndex(t=>t.TenantId);
                builder.HasIndex(t =>t.ChildId);
                builder.HasIndex(t => t.ParentId);

                builder.HasOne(t => t.Parent)
                    .WithMany(t => t.Parents)
                    .HasForeignKey(t => t.ParentId);

                builder.HasOne(t => t.Child)
                    .WithMany(t => t.Children)
                    .HasForeignKey(t => t.ChildId);

                builder.HasMany(t=>t.ComponentSettings)
                    .WithOne(t => t.ComponentRelation)
                    .HasForeignKey(t=>t.ComponentRelationId);
            });

            modelBuilder.Entity<Component>(builder =>
            {
                builder.HasIndex(t => t.TenantId);
                builder.HasIndex(t => t.Id);
            });
             
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            //optionsBuilder.ReplaceService<IMigrationsSqlGenerator, CustomNpgsqlMigrationsSqlGenerator>();
        }
    }
}
