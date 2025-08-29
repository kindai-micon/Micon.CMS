using System;
using System.Linq;
using System.Threading.Tasks;
using Micon.CMS.Models;
using Micon.CMS.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Micon.CMS.Tests
{
    public class PageTemplateRepositoryTests : IClassFixture<MiconCmsAppFactory>, IAsyncLifetime
    {
        private readonly MiconCmsAppFactory _factory;
        private IServiceScope? _scope;
        private ApplicationDbContext? _dbContext;
        private IPageTemplateRepository? _pageTemplateRepository;

        // Test data IDs
        private Guid _parentComponentId;
        private Guid _childComponentId;
        private Guid _pageTemplateId;

        public PageTemplateRepositoryTests(MiconCmsAppFactory factory)
        {
            _factory = factory;
        }

        public async Task InitializeAsync()
        {
            // Create a client to ensure the server is initialized and services are available
            _factory.CreateClient(); 
            
            _scope = _factory.Services.CreateScope();
            _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _pageTemplateRepository = new PageTemplateRepository(_dbContext);

            // Ensure the database is created and migrated
            await _dbContext.Database.MigrateAsync();
            
            // Clear and seed the database
            await SeedDatabaseAsync();
        }

        public async Task DisposeAsync()
        {
            if (_dbContext != null)
            {
                await _dbContext.DisposeAsync();
            }
            _scope?.Dispose();
        }

        [Fact]
        public async Task GetComponentHierarchy_ReturnsCorrectHierarchy()
        {
            // Arrange
            Assert.NotNull(_dbContext);
            Assert.NotNull(_pageTemplateRepository);
            var pageTemplate = await _dbContext.PageTemplates.FindAsync(_pageTemplateId);
            Assert.NotNull(pageTemplate); // Ensure the template was found

            // Act
            var hierarchy = await _pageTemplateRepository.GetComponentHierarchy(pageTemplate, default);

            // Assert
            Assert.NotNull(hierarchy);
            Assert.Equal(2, hierarchy.Count);

            var parentInHierarchy = hierarchy.SingleOrDefault(h => h.ChildComponentId == _parentComponentId);
            var childInHierarchy = hierarchy.SingleOrDefault(h => h.ChildComponentId == _childComponentId);

            Assert.NotNull(parentInHierarchy);
            Assert.Null(parentInHierarchy.ParentId); // Root component
            Assert.Equal("Root", parentInHierarchy.SlotName);

            Assert.NotNull(childInHierarchy);
            Assert.Equal(parentInHierarchy.ChildId, childInHierarchy.ParentId);
            Assert.Equal("ChildSlot", childInHierarchy.SlotName);
            Assert.Equal("C2ViewComponent", childInHierarchy.ChildComponentName);
        }

        private async Task SeedDatabaseAsync()
        {
            Assert.NotNull(_dbContext);
            await _dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Pages\", \"PageCategories\", \"PageTemplates\", \"ComponentRelations\", \"Components\" RESTART IDENTITY CASCADE;");

            var parentComponent = new Component { Name = "C1ViewComponent", PackageId = Guid.NewGuid() };
            var childComponent = new Component { Name = "C2ViewComponent", PackageId = Guid.NewGuid() };
            _dbContext.Components.AddRange(parentComponent, childComponent);
            await _dbContext.SaveChangesAsync();
            _parentComponentId = parentComponent.Id;
            _childComponentId = childComponent.Id;

            var childRelation = new ComponentRelation { ParentId = parentComponent.Id, ChildId = childComponent.Id, SlotName = "ChildSlot", Order = 1 };
            _dbContext.ComponentRelations.Add(childRelation);
            await _dbContext.SaveChangesAsync();

            var rootRelation = new ComponentRelation { ChildId = parentComponent.Id, SlotName = "Root", Order = 1 };
            _dbContext.ComponentRelations.Add(rootRelation);
            await _dbContext.SaveChangesAsync();

            var category = new PageCategory { Name = "TestCategory" };
            _dbContext.PageCategories.Add(category);
            await _dbContext.SaveChangesAsync();

            var template = new PageTemplate { Name = "TestTemplate", ComponentRelationId = rootRelation.Id };
            _dbContext.PageTemplates.Add(template);
            await _dbContext.SaveChangesAsync();
            _pageTemplateId = template.Id;
        }
    }
}