using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Micon.CMS.Controllers;
using Micon.CMS.Models;
using Micon.CMS.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Micon.CMS.Tests
{
    /// <summary>
    /// PageTemplateControllerの差分管理機能（MergeComponentRelationTree）のテスト
    /// </summary>
    public class PageTemplateControllerDiffMergeTests : IClassFixture<MiconCmsAppFactory>, IAsyncLifetime
    {
        private readonly MiconCmsAppFactory _factory;
        private IServiceScope? _scope;
        private ApplicationDbContext? _dbContext;
        private IComponentRepository? _componentRepository;
        private IComponentRelationRepository? _componentRelationRepository;
        private CancellationToken _cancellationToken = CancellationToken.None;

        // テスト用コンポーネントID
        private Guid _layoutComponentId;
        private Guid _headerComponentId;
        private Guid _menuComponentId;
        private Guid _sidebarComponentId;

        public PageTemplateControllerDiffMergeTests(MiconCmsAppFactory factory)
        {
            _factory = factory;
        }

        public async Task InitializeAsync()
        {
            _factory.CreateClient();
            _scope = _factory.Services.CreateScope();
            _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _componentRepository = _scope.ServiceProvider.GetRequiredService<IComponentRepository>();
            _componentRelationRepository = _scope.ServiceProvider.GetRequiredService<IComponentRelationRepository>();

            await _dbContext.Database.MigrateAsync();
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

        /// <summary>
        /// テストデータをデータベースに挿入
        /// </summary>
        private async Task SeedDatabaseAsync()
        {
            // 既存データを削除
            var componentRelations = await _dbContext.ComponentRelations.ToListAsync();
            _dbContext.ComponentRelations.RemoveRange(componentRelations);

            var components = await _dbContext.Components.ToListAsync();
            _dbContext.Components.RemoveRange(components);

            await _dbContext.SaveChangesAsync();

            // テスト用コンポーネントを作成
            _layoutComponentId = Guid.NewGuid();
            _headerComponentId = Guid.NewGuid();
            _menuComponentId = Guid.NewGuid();
            _sidebarComponentId = Guid.NewGuid();

            var layoutComponent = new Component
            {
                Id = _layoutComponentId,
                Name = "Layout",
                PackageId = Guid.NewGuid(),
                Parents = new List<ComponentRelation>(),
                Children = new List<ComponentRelation>()
            };
            var headerComponent = new Component
            {
                Id = _headerComponentId,
                Name = "Header",
                PackageId = Guid.NewGuid(),
                Parents = new List<ComponentRelation>(),
                Children = new List<ComponentRelation>()
            };
            var menuComponent = new Component
            {
                Id = _menuComponentId,
                Name = "Menu",
                PackageId = Guid.NewGuid(),
                Parents = new List<ComponentRelation>(),
                Children = new List<ComponentRelation>()
            };
            var sidebarComponent = new Component
            {
                Id = _sidebarComponentId,
                Name = "Sidebar",
                PackageId = Guid.NewGuid(),
                Parents = new List<ComponentRelation>(),
                Children = new List<ComponentRelation>()
            };

            _dbContext.Components.Add(layoutComponent);
            _dbContext.Components.Add(headerComponent);
            _dbContext.Components.Add(menuComponent);
            _dbContext.Components.Add(sidebarComponent);

            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// 同じ構造の場合、既存のComponentRelationを再利用するテスト
        /// </summary>
        [Fact]
        public async Task SameStructure_ShouldReuseExistingRelations()
        {
            // Arrange
            var layoutComponent = await _componentRepository!.GetByIdAsync(_layoutComponentId, _cancellationToken);
            var headerComponent = await _componentRepository!.GetByIdAsync(_headerComponentId, _cancellationToken);

            // 既存の構造を作成
            var existingHeaderRelation = new ComponentRelation
            {
                Id = Guid.NewGuid(),
                ParentId = null,
                ChildId = _headerComponentId,
                SlotName = "Main",
                Order = 0,
                IsPriority = false,
                Child = headerComponent
            };

            await _componentRelationRepository!.CreateAsync(existingHeaderRelation, _cancellationToken);

            var existingHeaderRelationId = existingHeaderRelation.Id;

            // 新しいツリーデータ（同じ構造）
            var newNode = new PageTemplateController.ComponentTreeNode
            {
                ComponentId = _headerComponentId.ToString(),
                ComponentName = "Header",
                PackageId = headerComponent.PackageId.ToString(),
                SlotName = "Main",
                Children = new List<PageTemplateController.ComponentTreeNode>()
            };

            // Act
            var controller = _scope!.ServiceProvider.GetRequiredService<PageTemplateController>();
            var resultRelation = await InvokeMergeComponentRelationTree(controller, newNode, existingHeaderRelation, null, 0);

            // Assert
            Assert.NotNull(resultRelation);
            Assert.Equal(existingHeaderRelationId, resultRelation.Id); // IDが同じ（再利用）
            Assert.Equal("Header", resultRelation.Child?.Name);
        }

        /// <summary>
        /// 新しいコンポーネントを追加するテスト
        /// </summary>
        [Fact]
        public async Task NewComponent_ShouldCreateNewRelation()
        {
            // Arrange
            var headerComponent = await _componentRepository!.GetByIdAsync(_headerComponentId, _cancellationToken);

            var newNode = new PageTemplateController.ComponentTreeNode
            {
                ComponentId = _headerComponentId.ToString(),
                ComponentName = "Header",
                PackageId = headerComponent.PackageId.ToString(),
                SlotName = "Main",
                Children = new List<PageTemplateController.ComponentTreeNode>()
            };

            // Act
            var controller = _scope!.ServiceProvider.GetRequiredService<PageTemplateController>();
            var resultRelation = await InvokeMergeComponentRelationTree(controller, newNode, null, null, 0);

            // Assert
            Assert.NotNull(resultRelation);
            Assert.NotEqual(Guid.Empty, resultRelation.Id); // 新しいIDが生成される
            Assert.Equal(_headerComponentId, resultRelation.ChildId);
            Assert.Equal("Main", resultRelation.SlotName);
        }

        /// <summary>
        /// 子要素を削除するテスト
        /// </summary>
        [Fact]
        public async Task RemoveChild_ShouldDeleteChildRelation()
        {
            // Arrange
            var headerComponent = await _componentRepository!.GetByIdAsync(_headerComponentId, _cancellationToken);
            var menuComponent = await _componentRepository!.GetByIdAsync(_menuComponentId, _cancellationToken);

            // 既存の構造：Header > Menu
            var existingHeaderRelation = new ComponentRelation
            {
                Id = Guid.NewGuid(),
                ParentId = null,
                ChildId = _headerComponentId,
                SlotName = "Main",
                Order = 0,
                IsPriority = false,
                Child = headerComponent
            };

            var existingMenuRelation = new ComponentRelation
            {
                Id = Guid.NewGuid(),
                ParentId = _headerComponentId,
                ChildId = _menuComponentId,
                SlotName = "Navigation",
                Order = 0,
                IsPriority = false,
                Child = menuComponent
            };

            headerComponent.Children = new List<ComponentRelation> { existingMenuRelation };

            await _componentRelationRepository!.CreateAsync(existingHeaderRelation, _cancellationToken);
            await _componentRelationRepository!.CreateAsync(existingMenuRelation, _cancellationToken);

            var existingMenuRelationId = existingMenuRelation.Id;

            // 新しいツリー：Header のみ（Menu を削除）
            var newNode = new PageTemplateController.ComponentTreeNode
            {
                ComponentId = _headerComponentId.ToString(),
                ComponentName = "Header",
                PackageId = headerComponent.PackageId.ToString(),
                SlotName = "Main",
                Children = new List<PageTemplateController.ComponentTreeNode>() // 子なし
            };

            // Act
            var controller = _scope!.ServiceProvider.GetRequiredService<PageTemplateController>();
            var resultRelation = await InvokeMergeComponentRelationTree(controller, newNode, existingHeaderRelation, null, 0);

            // Assert
            Assert.NotNull(resultRelation);

            // Menu関連が削除されていることを確認
            var deletedMenu = await _componentRelationRepository!.GetByIdAsync(existingMenuRelationId, _cancellationToken);
            Assert.Null(deletedMenu);
        }

        /// <summary>
        /// 複雑な構造の差分をマージするテスト
        /// </summary>
        [Fact]
        public async Task ComplexMerge_ShouldHandleMultipleChanges()
        {
            // Arrange
            var layoutComponent = await _componentRepository!.GetByIdAsync(_layoutComponentId, _cancellationToken);
            var headerComponent = await _componentRepository!.GetByIdAsync(_headerComponentId, _cancellationToken);
            var menuComponent = await _componentRepository!.GetByIdAsync(_menuComponentId, _cancellationToken);
            var sidebarComponent = await _componentRepository!.GetByIdAsync(_sidebarComponentId, _cancellationToken);

            // 既存の構造：Layout > Header > Menu
            var existingLayoutRelation = new ComponentRelation
            {
                Id = Guid.NewGuid(),
                ParentId = null,
                ChildId = _layoutComponentId,
                SlotName = "Main",
                Order = 0,
                IsPriority = false,
                Child = layoutComponent
            };

            var existingHeaderRelation = new ComponentRelation
            {
                Id = Guid.NewGuid(),
                ParentId = _layoutComponentId,
                ChildId = _headerComponentId,
                SlotName = "Header",
                Order = 0,
                IsPriority = false,
                Child = headerComponent
            };

            var existingMenuRelation = new ComponentRelation
            {
                Id = Guid.NewGuid(),
                ParentId = _headerComponentId,
                ChildId = _menuComponentId,
                SlotName = "Navigation",
                Order = 0,
                IsPriority = false,
                Child = menuComponent
            };

            layoutComponent.Children = new List<ComponentRelation> { existingHeaderRelation };
            headerComponent.Children = new List<ComponentRelation> { existingMenuRelation };

            await _componentRelationRepository!.CreateAsync(existingLayoutRelation, _cancellationToken);
            await _componentRelationRepository!.CreateAsync(existingHeaderRelation, _cancellationToken);
            await _componentRelationRepository!.CreateAsync(existingMenuRelation, _cancellationToken);

            var existingHeaderRelationId = existingHeaderRelation.Id;
            var existingMenuRelationId = existingMenuRelation.Id;

            // 新しい構造：Layout > Header（Header IDは再利用） > Sidebar（Menuを削除）
            var newNode = new PageTemplateController.ComponentTreeNode
            {
                ComponentId = _layoutComponentId.ToString(),
                ComponentName = "Layout",
                PackageId = layoutComponent.PackageId.ToString(),
                SlotName = "Main",
                Children = new List<PageTemplateController.ComponentTreeNode>
                {
                    new PageTemplateController.ComponentTreeNode
                    {
                        ComponentId = _headerComponentId.ToString(),
                        ComponentName = "Header",
                        PackageId = headerComponent.PackageId.ToString(),
                        SlotName = "Header",
                        Children = new List<PageTemplateController.ComponentTreeNode>
                        {
                            new PageTemplateController.ComponentTreeNode
                            {
                                ComponentId = _sidebarComponentId.ToString(),
                                ComponentName = "Sidebar",
                                PackageId = sidebarComponent.PackageId.ToString(),
                                SlotName = "Navigation",
                                Children = new List<PageTemplateController.ComponentTreeNode>()
                            }
                        }
                    }
                }
            };

            // Act
            var controller = _scope!.ServiceProvider.GetRequiredService<PageTemplateController>();
            var resultRelation = await InvokeMergeComponentRelationTree(controller, newNode, existingLayoutRelation, null, 0);

            // Assert
            Assert.NotNull(resultRelation);
            Assert.Equal(existingLayoutRelation.Id, resultRelation.Id); // Layout は再利用

            // Header関連は再利用されているはず
            var headerRelation = await _componentRelationRepository!.GetByIdAsync(existingHeaderRelationId, _cancellationToken);
            Assert.NotNull(headerRelation); // Header は保持

            // Menu関連は削除されているはず
            var menuRelation = await _componentRelationRepository!.GetByIdAsync(existingMenuRelationId, _cancellationToken);
            Assert.Null(menuRelation); // Menu は削除

            // Sidebar関連は新規作成されているはず
            var sidebarRelations = await _componentRelationRepository!.GetAllAsync(_cancellationToken);
            var sidebarRelation = sidebarRelations.FirstOrDefault(r => r.ChildId == _sidebarComponentId);
            Assert.NotNull(sidebarRelation); // Sidebar は新規作成
        }

        /// <summary>
        /// MergeComponentRelationTreeメソッドをリフレクションで呼び出す
        /// </summary>
        private async Task<ComponentRelation> InvokeMergeComponentRelationTree(
            PageTemplateController controller,
            PageTemplateController.ComponentTreeNode newNode,
            ComponentRelation? existingRelation,
            Guid? parentId,
            int order)
        {
            var method = typeof(PageTemplateController)
                .GetMethod("MergeComponentRelationTree",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            Assert.NotNull(method);

            var task = (Task<ComponentRelation>)method!.Invoke(controller,
                new object[] { newNode, existingRelation, parentId, order, _cancellationToken })!;

            return await task;
        }
    }
}
