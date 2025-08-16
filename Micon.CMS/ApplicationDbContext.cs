using Micon.CMS.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using System.Threading;
using System.Threading.Tasks;

namespace Micon.CMS
{
    public class ApplicationDbContext:DbContext
    {
        // A fixed TenantId to be used for all data.
        private static readonly Guid DefaultTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

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
        public DbSet<ComponentHierarchy> ComponentHierarchies { get; set; }

        public override int SaveChanges()
        {
            SetTenantIdAndTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetTenantIdAndTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void SetTenantIdAndTimestamps()
        {
            var entries = ChangeTracker.Entries<IBaseModel>();
            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    // Set the fixed TenantId for new entities.
                    entry.Entity.TenantId = DefaultTenantId;
                    entry.Entity.Created = DateTimeOffset.UtcNow;
                }
                entry.Entity.Modified = DateTimeOffset.UtcNow;
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ComponentHierarchy>(builder =>
            {
                builder.HasNoKey();
                builder.ToView(null);
            });

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
                builder.HasOne(t => t.PageCategory)
                    .WithOne(t => t.PageTemplate);
                builder.HasOne(t => t.ComponentRelation)
                    .WithMany(t => t.PageTemplates)
                    .HasForeignKey(x=>x.ComponentRelationId);

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


            });

            modelBuilder.Entity<Component>(builder =>
            {
                builder.HasMany(t => t.ComponentSettings)
                .WithOne(t => t.Component)
                .HasForeignKey(t => t.ComponentId);
                builder.HasIndex(t => t.TenantId);
                builder.HasIndex(t => t.Id);
            });
             
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.ReplaceService<IMigrationsSqlGenerator, CustomNpgsqlMigrationsSqlGenerator>();
        }
    }
}
