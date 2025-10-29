using Micon.CMS.Models;
using Micon.CMS.Repositories;
using Micon.CMS.Library.Models.Form;
using Micon.CMS.Library.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using System;

namespace Micon.CMS.Controllers
{
    public class PageTemplateController : Controller
    {
        private readonly IPageTemplateRepository _pageTemplateRepository;
        private readonly IComponentRepository _componentRepository;
        private readonly IComponentRelationRepository _componentRelationRepository;
        private readonly IPageTemplateWorkspaceRepository _workspaceRepository;
        private readonly IViewComponentHelper _viewComponentHelper;
        private readonly ComponentSlotAnalyzerService _slotAnalyzerService;

        public PageTemplateController(
            IPageTemplateRepository pageTemplateRepository,
            IComponentRepository componentRepository,
            IComponentRelationRepository componentRelationRepository,
            IPageTemplateWorkspaceRepository workspaceRepository,
            IViewComponentHelper viewComponentHelper,
            ComponentSlotAnalyzerService slotAnalyzerService)
        {
            _pageTemplateRepository = pageTemplateRepository;
            _componentRepository = componentRepository;
            _componentRelationRepository = componentRelationRepository;
            _workspaceRepository = workspaceRepository;
            _viewComponentHelper = viewComponentHelper;
            _slotAnalyzerService = slotAnalyzerService;
        }
        public async Task<IActionResult> Index(CancellationToken cancellation)
        {
            var templates = await _pageTemplateRepository.GetAllWithCategoryAsync(cancellation);
            return View(templates);
        }

        public async Task<IActionResult> Create( CancellationToken cancellationToken)
        {

            var template = new PageTemplate();
            return View(template);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSubmit(PageTemplate pageTemplate,CancellationToken cancellationToken)
        {
            var newPageTemplate = await _pageTemplateRepository.CreateAsync(pageTemplate, cancellationToken);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(Guid Id, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Edit action called with Id: {Id}");

            var pageTemplate = await _pageTemplateRepository.GetByIdAsync(Id, cancellationToken);
            if (pageTemplate == null)
            {
                Console.WriteLine("PageTemplate not found");
                return RedirectToAction(nameof(Index));
            }

            Console.WriteLine($"PageTemplate found: {pageTemplate.Name}");

            // ワークスペースを取得、存在しなければ作成
            var workspace = await _workspaceRepository.GetByPageTemplateIdAsync(Id, cancellationToken);

            if (workspace == null)
            {
                Console.WriteLine("Workspace not found, creating new one");
                // 新しいワークスペースを作成
                var initialTree = new List<ComponentTreeNode>();

                // 元の PageTemplate に ComponentRelation がある場合、ツリーとして保存
                if (pageTemplate.ComponentRelationId.HasValue)
                {
                    var hierarchy = await _pageTemplateRepository.GetComponentHierarchy(pageTemplate, cancellationToken);
                    initialTree = await BuildComponentTreeFromHierarchy(hierarchy, cancellationToken);
                }

                workspace = new PageTemplateWorkspace
                {
                    PageTemplateId = Id,
                    WorkspaceData = System.Text.Json.JsonSerializer.Serialize(initialTree),
                    CreatedByUserId = null,
                    UpdatedAt = DateTime.UtcNow
                };

                workspace = await _workspaceRepository.CreateAsync(workspace, cancellationToken);
                Console.WriteLine($"Workspace created with {initialTree.Count} root nodes");
            }
            else
            {
                Console.WriteLine($"Workspace found: {workspace.Id}, Data length: {workspace.WorkspaceData.Length}");
            }

            // コンポーネント情報とスロット情報を取得
            var components = await _componentRepository.GetAllAsync(cancellationToken);
            var componentsWithSlots = await BuildComponentsWithSlots(components, cancellationToken);
            ViewBag.Components = componentsWithSlots;
            ViewBag.WorkspaceId = workspace.Id;

            // ワークスペースのツリーをデシリアライズして表示
            var workspaceTree = System.Text.Json.JsonSerializer.Deserialize<List<ComponentTreeNode>>(workspace.WorkspaceData) ?? new List<ComponentTreeNode>();
            Console.WriteLine($"Deserialized workspace tree with {workspaceTree.Count} root nodes");
            Console.WriteLine($"Workspace data being sent to view: {System.Text.Json.JsonSerializer.Serialize(workspaceTree)}");

            ViewBag.ComponentHierarchy = workspaceTree;

            // ワークスペースのツリーからPageComponentViewModelを構築してプレビュー用に渡す
            var previewViewModel = await BuildPageComponentViewModelFromWorkspace(workspaceTree, pageTemplate.Id, cancellationToken);
            ViewBag.PreviewViewModel = previewViewModel;

            return View(pageTemplate);
        }

        private async Task<List<ComponentTreeNode>> BuildComponentTreeFromHierarchy(List<Models.ComponentHierarchy> hierarchy, CancellationToken cancellationToken)
        {
            if (hierarchy == null || !hierarchy.Any())
            {
                return new List<ComponentTreeNode>();
            }

            var rootNodes = hierarchy.Where(h => h.ParentId == null || h.ParentId == Guid.Empty).ToList();
            var result = new List<ComponentTreeNode>();

            foreach (var root in rootNodes)
            {
                result.Add(await BuildNodeTree(root, hierarchy, cancellationToken));
            }

            return result;
        }

        private async Task<ComponentTreeNode> BuildNodeTree(Models.ComponentHierarchy hierarchyItem, List<Models.ComponentHierarchy> allHierarchy, CancellationToken cancellationToken)
        {
            var node = new ComponentTreeNode
            {
                ComponentId = hierarchyItem.ChildComponentId.ToString(),
                ComponentName = hierarchyItem.ChildComponentName,
                PackageId = hierarchyItem.ChildComponentPackageId.ToString(),
                SlotName = hierarchyItem.SlotName,
                Children = new List<ComponentTreeNode>()
            };

            var childItems = allHierarchy.Where(h => h.ParentComponentId == hierarchyItem.ChildComponentId).ToList();
            foreach (var child in childItems)
            {
                node.Children.Add(await BuildNodeTree(child, allHierarchy, cancellationToken));
            }

            return node;
        }

        private Micon.CMS.Library.Models.Form.PageComponentViewModel BuildComponentViewModelTree(List<Models.ComponentHierarchy> hierarchy)
        {
            if (hierarchy == null || !hierarchy.Any())
            {
                return new Micon.CMS.Library.Models.Form.PageComponentViewModel();
            }

            var lookup = hierarchy.ToDictionary(
                h => h.ChildId,
                h => new Micon.CMS.Library.Models.Form.PageComponentViewModel
                {
                    ComponentId = h.ChildComponentId,
                    ComponentName = h.ChildComponentName,
                    SlotName = h.SlotName,
                    PackageId = h.ChildComponentPackageId
                });

            var rootNode = new Micon.CMS.Library.Models.Form.PageComponentViewModel
            {
                ComponentName = "Root",
                SlotName = "Main"
            };

            foreach (var item in hierarchy)
            {
                if (item.ParentId.HasValue && lookup.ContainsKey(item.ParentId.Value))
                {
                    lookup[item.ParentId.Value].Children.Add(lookup[item.ChildId]);
                }
                else
                {
                    rootNode.Children.Add(lookup[item.ChildId]);
                }
            }

            return rootNode.Children.Count == 1 ? rootNode.Children.First() : rootNode;
        }

        [HttpPost]
        public async Task<IActionResult> InitializeComponentRelation(Guid Id, CancellationToken cancellationToken)
        {
            var pageTemplate = await _pageTemplateRepository.GetByIdAsync(Id, cancellationToken);
            if (pageTemplate == null)
            {
                return NotFound();
            }

            // 初期化済みフラグを設定（実際のComponentRelationは最初のコンポーネント追加時に作成）
            TempData["Initialized"] = true;

            return RedirectToAction(nameof(Edit), new { Id = Id });
        }

        [HttpPost]
        public async Task<IActionResult> AddComponent([FromBody] AddComponentRequest request, CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine($"AddComponent called with: {System.Text.Json.JsonSerializer.Serialize(request)}");

                if (request == null)
                {
                    return BadRequest("Request body is required");
                }

                if (!Guid.TryParse(request.PageTemplateId, out var pageTemplateId))
                {
                    Console.WriteLine($"Invalid pageTemplateId: {request.PageTemplateId}");
                    return BadRequest("Invalid pageTemplateId format");
                }

                if (!Guid.TryParse(request.ComponentId, out var componentId))
                {
                    Console.WriteLine($"Invalid componentId: {request.ComponentId}");
                    return BadRequest("Invalid componentId format");
                }

                Console.WriteLine($"Looking for PageTemplate: {pageTemplateId}");
                var pageTemplate = await _pageTemplateRepository.GetByIdAsync(pageTemplateId, cancellationToken);
                if (pageTemplate == null)
                {
                    Console.WriteLine("PageTemplate not found");
                    return NotFound("PageTemplate not found");
                }

                Console.WriteLine($"Looking for Component: {componentId}");
                var component = await _componentRepository.GetByIdAsync(componentId, cancellationToken);
                if (component == null)
                {
                    Console.WriteLine("Component not found");
                    return BadRequest("Component not found");
                }

                // Workspace を取得
                Console.WriteLine($"Looking for Workspace for PageTemplate: {pageTemplateId}");
                var workspace = await _workspaceRepository.GetByPageTemplateIdAsync(pageTemplateId, cancellationToken);
                if (workspace == null)
                {
                    Console.WriteLine("Workspace not found");
                    return NotFound("Workspace not found");
                }

                Console.WriteLine($"Workspace found: {workspace.Id}, Data length: {workspace.WorkspaceData.Length}");

                // ワークスペースのツリーをデシリアライズ
                var tree = System.Text.Json.JsonSerializer.Deserialize<List<ComponentTreeNode>>(workspace.WorkspaceData) ?? new List<ComponentTreeNode>();
                Console.WriteLine($"Deserialized tree with {tree.Count} root nodes");

                // 新しいノードを作成
                var newNode = new ComponentTreeNode
                {
                    ComponentId = component.Id.ToString(),
                    ComponentName = component.Name,
                    PackageId = component.PackageId.ToString(),
                    SlotName = request.SlotName ?? "Main",
                    Children = new List<ComponentTreeNode>()
                };

                // ツリーに追加
                if (request.ParentPath == null || request.ParentPath.Count == 0)
                {
                    Console.WriteLine("Adding node as root");
                    tree.Add(newNode);
                }
                else
                {
                    Console.WriteLine($"Adding node as child at path: {string.Join(",", request.ParentPath)}");
                    var parent = GetNodeByPath(tree, request.ParentPath);
                    if (parent != null)
                    {
                        var targetSlot = newNode.SlotName ?? "Main";
                        if (parent.Children.Any(c => string.Equals(c.SlotName ?? "Main", targetSlot, StringComparison.OrdinalIgnoreCase)))
                        {
                            Console.WriteLine("Parent slot already occupied");
                            return BadRequest("指定したスロットには既にコンポーネントが配置されています。先に削除してください。");
                        }

                        parent.Children.Add(newNode);
                        Console.WriteLine("Child node added successfully");
                    }
                    else
                    {
                        Console.WriteLine("Parent node not found");
                        return BadRequest("Parent node not found");
                    }
                }

                // ワークスペースを更新して保存
                workspace.WorkspaceData = System.Text.Json.JsonSerializer.Serialize(tree);
                workspace.UpdatedAt = DateTime.UtcNow;
                Console.WriteLine($"Saving workspace with {tree.Count} root nodes");
                await _workspaceRepository.UpdateAsync(workspace, cancellationToken);
                Console.WriteLine("Workspace saved successfully");

                // 更新後のツリーと追加コンポーネント情報を返却
                return Ok(new
                {
                    tree,
                    addedComponent = new
                    {
                        componentId = component.Id.ToString(),
                        componentName = component.Name,
                        packageId = component.PackageId.ToString(),
                        slotName = request.SlotName ?? "Main"
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in AddComponent: {ex}");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveComponent([FromBody] RemoveComponentRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Request body is required");
                }

                if (!Guid.TryParse(request.PageTemplateId, out var pageTemplateId))
                {
                    return BadRequest("Invalid pageTemplateId format");
                }

                if (request.Path == null || request.Path.Count == 0)
                {
                    return BadRequest("Path is required");
                }

                var workspace = await _workspaceRepository.GetByPageTemplateIdAsync(pageTemplateId, cancellationToken);
                if (workspace == null)
                {
                    return NotFound("Workspace not found");
                }

                var tree = System.Text.Json.JsonSerializer.Deserialize<List<ComponentTreeNode>>(workspace.WorkspaceData) ?? new List<ComponentTreeNode>();

                if (!RemoveNodeFromTree(tree, request.Path))
                {
                    return BadRequest("Target node not found");
                }

                workspace.WorkspaceData = System.Text.Json.JsonSerializer.Serialize(tree);
                workspace.UpdatedAt = DateTime.UtcNow;
                await _workspaceRepository.UpdateAsync(workspace, cancellationToken);

                return Ok(new { tree });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        private ComponentTreeNode GetNodeByPath(List<ComponentTreeNode> tree, List<int> path)
        {
            ComponentTreeNode current = null;

            foreach (var index in path)
            {
                if (current == null)
                {
                    current = tree.ElementAtOrDefault(index);
                }
                else
                {
                    current = current.Children.ElementAtOrDefault(index);
                }

                if (current == null)
                    return null;
            }

            return current;
        }

        private bool RemoveNodeFromTree(List<ComponentTreeNode> tree, List<int> path)
        {
            if (path == null || path.Count == 0)
            {
                return false;
            }

            if (path.Count == 1)
            {
                var index = path[0];
                if (index < 0 || index >= tree.Count)
                {
                    return false;
                }

                tree.RemoveAt(index);
                return true;
            }

            var parentPath = path.Take(path.Count - 1).ToList();
            var parent = GetNodeByPath(tree, parentPath);
            if (parent == null || parent.Children == null)
            {
                return false;
            }

            var childIndex = path[^1];
            if (childIndex < 0 || childIndex >= parent.Children.Count)
            {
                return false;
            }

            parent.Children.RemoveAt(childIndex);
            return true;
        }

        /// <summary>
        /// ワークスペースのツリー（ComponentTreeNode）からPageComponentViewModelを構築
        /// </summary>
        private async Task<Micon.CMS.Library.Models.Form.PageComponentViewModel> BuildPageComponentViewModelFromWorkspace(
            List<ComponentTreeNode> workspaceTree,
            Guid pageTemplateId,
            CancellationToken cancellationToken)
        {
            var rootViewModel = new Micon.CMS.Library.Models.Form.PageComponentViewModel
            {
                ComponentName = "Root",
                SlotName = "Main"
            };

            if (workspaceTree == null || workspaceTree.Count == 0)
            {
                return rootViewModel;
            }

            // 各ComponentTreeNodeをPageComponentViewModelに変換
            foreach (var treeNode in workspaceTree)
            {
                var viewModel = await ConvertTreeNodeToViewModel(treeNode, cancellationToken);
                if (viewModel != null)
                {
                    rootViewModel.Children.Add(viewModel);
                }
            }

            // ルートが1つの場合はそれを返す
            return rootViewModel.Children.Count == 1 ? rootViewModel.Children.First() : rootViewModel;
        }

        /// <summary>
        /// ComponentTreeNodeをPageComponentViewModelに再帰的に変換
        /// </summary>
        private async Task<Micon.CMS.Library.Models.Form.PageComponentViewModel> ConvertTreeNodeToViewModel(
            ComponentTreeNode treeNode,
            CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(treeNode.ComponentId, out var componentId))
            {
                return null;
            }

            if (!Guid.TryParse(treeNode.PackageId, out var packageId))
            {
                return null;
            }

            var viewModel = new Micon.CMS.Library.Models.Form.PageComponentViewModel
            {
                ComponentId = componentId,
                ComponentName = treeNode.ComponentName,
                SlotName = treeNode.SlotName ?? "Main",
                PackageId = packageId,
                Settings = new Dictionary<string, string>() // 編集中のプレビューなので空
            };

            // 子要素を再帰的に変換
            if (treeNode.Children != null && treeNode.Children.Count > 0)
            {
                foreach (var childNode in treeNode.Children)
                {
                    var childViewModel = await ConvertTreeNodeToViewModel(childNode, cancellationToken);
                    if (childViewModel != null)
                    {
                        viewModel.Children.Add(childViewModel);
                    }
                }
            }

            return viewModel;
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSubmit(PageTemplate pageTemplate, CancellationToken cancellationToken)
        {
            var updatedPageTemplate = await _pageTemplateRepository.UpdateAsync(pageTemplate, cancellationToken);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> SavePageTemplate([FromBody] SavePageTemplateRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Request body is required");
                }

                if (!Guid.TryParse(request.PageTemplateId, out var pageTemplateId))
                {
                    return BadRequest("Invalid pageTemplateId format");
                }

                var pageTemplate = await _pageTemplateRepository.GetByIdAsync(pageTemplateId, cancellationToken);
                if (pageTemplate == null)
                {
                    return NotFound("PageTemplate not found");
                }

                // Workspace を取得
                var workspace = await _workspaceRepository.GetByPageTemplateIdAsync(pageTemplateId, cancellationToken);
                if (workspace == null)
                {
                    return NotFound("Workspace not found");
                }

                // ツリーが空の場合は ComponentRelationId をクリア
                if (request.Tree == null || request.Tree.Count == 0)
                {
                    // 既存の ComponentRelation を削除
                    if (pageTemplate.ComponentRelationId.HasValue)
                    {
                        var existingRelation = await _componentRelationRepository.GetByIdAsync(pageTemplate.ComponentRelationId.Value, cancellationToken);
                        if (existingRelation != null)
                        {
                            await _componentRelationRepository.DeleteAsync(existingRelation, cancellationToken);
                        }
                    }

                    pageTemplate.ComponentRelationId = null;
                    await _pageTemplateRepository.UpdateAsync(pageTemplate, cancellationToken);

                    // ワークスペースを削除
                    await _workspaceRepository.DeleteAsync(workspace, cancellationToken);

                    return Ok(new { message = "PageTemplate saved successfully (empty tree)" });
                }

                // 既存の ComponentRelation ツリーを取得
                ComponentRelation existingRootRelation = null;
                if (pageTemplate.ComponentRelationId.HasValue)
                {
                    existingRootRelation = await _componentRelationRepository.GetByIdAsync(pageTemplate.ComponentRelationId.Value, cancellationToken);
                }

                // ツリーを差分管理で保存
                var rootNode = request.Tree[0];
                var rootRelation = await MergeComponentRelationTree(rootNode, existingRootRelation, null, 0, cancellationToken);

                if (rootRelation != null)
                {
                    pageTemplate.ComponentRelationId = rootRelation.Id;
                    await _pageTemplateRepository.UpdateAsync(pageTemplate, cancellationToken);
                }

                // ワークスペースを削除
                await _workspaceRepository.DeleteAsync(workspace, cancellationToken);

                return Ok(new { message = "PageTemplate saved successfully", componentRelationId = rootRelation?.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetComponents(CancellationToken cancellationToken)
        {
            var components = await _componentRepository.GetAllAsync(cancellationToken);
            return Json(components.Select(c => new
            {
                id = c.Id,
                packageId = c.PackageId,
                name = c.Name
            }));
        }

        [HttpPost]
        public async Task<IActionResult> SaveComponentTree([FromBody] SaveComponentTreeRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Request body is required");
                }

                if (!Guid.TryParse(request.PageTemplateId, out var pageTemplateId))
                {
                    return BadRequest("Invalid pageTemplateId format");
                }

                var pageTemplate = await _pageTemplateRepository.GetByIdAsync(pageTemplateId, cancellationToken);
                if (pageTemplate == null)
                {
                    return NotFound("PageTemplate not found");
                }

                // 既存のComponentRelationを削除
                if (pageTemplate.ComponentRelationId.HasValue)
                {
                    var existingRelation = await _componentRelationRepository.GetByIdAsync(pageTemplate.ComponentRelationId.Value, cancellationToken);
                    if (existingRelation != null)
                    {
                        await _componentRelationRepository.DeleteAsync(existingRelation, cancellationToken);
                    }
                    pageTemplate.ComponentRelationId = null;
                }

                // ツリーが空の場合は保存して返す
                if (request.Tree == null || request.Tree.Count == 0)
                {
                    await _pageTemplateRepository.UpdateAsync(pageTemplate, cancellationToken);
                    return Ok(new { message = "Component tree saved successfully (empty)" });
                }

                // ツリーを保存（ルートノードのみ）
                var rootNode = request.Tree[0];
                var rootRelation = await CreateComponentRelationTree(rootNode, null, 0, cancellationToken);

                if (rootRelation != null)
                {
                    pageTemplate.ComponentRelationId = rootRelation.Id;
                    await _pageTemplateRepository.UpdateAsync(pageTemplate, cancellationToken);
                }

                return Ok(new { message = "Component tree saved successfully", componentRelationId = rootRelation?.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        private async Task<ComponentRelation> CreateComponentRelationTree(
            ComponentTreeNode node,
            Guid? parentId,
            int order,
            CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(node.ComponentId, out var componentId))
            {
                return null;
            }

            var component = await _componentRepository.GetByIdAsync(componentId, cancellationToken);
            if (component == null)
            {
                return null;
            }

            var relation = new ComponentRelation
            {
                Id = Guid.NewGuid(),
                ParentId = parentId,
                ChildId = componentId,
                SlotName = node.SlotName ?? "Main",
                Order = order,
                IsPriority = false
            };

            await _componentRelationRepository.CreateAsync(relation, cancellationToken);

            // 子ノードを処理
            if (node.Children != null && node.Children.Count > 0)
            {
                int childOrder = 0;
                foreach (var child in node.Children)
                {
                    await CreateComponentRelationTree(child, relation.ChildId, childOrder++, cancellationToken);
                }
            }

            return relation;
        }

        /// <summary>
        /// 既存のComponentRelationツリーと新しいツリーを比較・マージして差分管理
        /// PackageId と Name が同じコンポーネントの関係は既存のレコードを再利用
        /// </summary>
        private async Task<ComponentRelation> MergeComponentRelationTree(
            ComponentTreeNode newNode,
            ComponentRelation existingRelation,
            Guid? parentId,
            int order,
            CancellationToken cancellationToken)
        {
            var component = await ExtractComponentAsync(newNode.ComponentId, cancellationToken);
            if (component == null)
            {
                return null;
            }

            // 既存のRelationが同じ構造か判定
            var slotName = newNode.SlotName ?? "Main";
            var isSameStructure = IsRelationUnchanged(existingRelation, component, slotName, parentId, order);

            // 既存の関連を再利用するか、新しいものを作成
            var relation = isSameStructure
                ? existingRelation
                : await CreateComponentRelationAsync(component, slotName, parentId, order, cancellationToken);

            // 子要素の処理
            var existingChildrenMap = BuildExistingChildrenMap(existingRelation);

            if (newNode.Children != null && newNode.Children.Count > 0)
            {
                await MergeChildrenAsync(newNode, relation, existingRelation, existingChildrenMap, cancellationToken);
            }
            else if (existingRelation != null)
            {
                // 子要素がない場合で、既存の子がある場合のみ削除
                await DeleteAllChildrenAsync(existingRelation, cancellationToken);
            }

            return relation;
        }

        /// <summary>
        /// 新しいComponentRelationを作成
        /// </summary>
        private async Task<ComponentRelation> CreateComponentRelationAsync(
            Component component,
            string slotName,
            Guid? parentId,
            int order,
            CancellationToken cancellationToken)
        {
            var relation = new ComponentRelation
            {
                Id = Guid.NewGuid(),
                ParentId = parentId,
                ChildId = component.Id,
                SlotName = slotName,
                Order = order,
                IsPriority = false
            };

            await _componentRelationRepository.CreateAsync(relation, cancellationToken);
            return relation;
        }

        /// <summary>
        /// ComponentRelation と その子孫をすべて削除
        /// </summary>
        private async Task DeleteComponentRelationTree(ComponentRelation relation, CancellationToken cancellationToken)
        {
            await DeleteComponentRelationTreeWithTracking(relation, new HashSet<Guid>(), cancellationToken);
        }

        /// <summary>
        /// ComponentRelationツリーの削除（削除済みIDを追跡）
        /// </summary>
        private async Task DeleteComponentRelationTreeWithTracking(
            ComponentRelation relation,
            HashSet<Guid> deletedIds,
            CancellationToken cancellationToken)
        {
            if (relation == null || deletedIds.Contains(relation.Id))
            {
                return;
            }

            deletedIds.Add(relation.Id);

            // このComponentRelationの子要素（relation.Child の下の要素）を再帰的に削除
            if (relation.Child?.Children != null && relation.Child.Children.Count > 0)
            {
                var childrenToDelete = relation.Child.Children.ToList();
                foreach (var child in childrenToDelete)
                {
                    await DeleteComponentRelationTreeWithTracking(child, deletedIds, cancellationToken);
                }
            }

            // このComponentRelationを削除
            await _componentRelationRepository.DeleteAsync(relation, cancellationToken);
        }

        /// <summary>
        /// ComponentIdを検証してComponentを取得
        /// </summary>
        private async Task<Component> ExtractComponentAsync(string componentId, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(componentId, out var parsedId))
            {
                return null;
            }

            return await _componentRepository.GetByIdAsync(parsedId, cancellationToken);
        }

        /// <summary>
        /// 子要素のキーを生成（PackageId + Name + SlotName）
        /// </summary>
        private static string CreateChildRelationKey(string packageId, string name, string slotName)
        {
            return $"{packageId}_{name}_{slotName}";
        }

        /// <summary>
        /// 既存のComponentRelationが新しいツリーと同じ構造か判定
        /// </summary>
        private static bool IsRelationUnchanged(
            ComponentRelation existingRelation,
            Component newComponent,
            string newSlotName,
            Guid? parentId,
            int order)
        {
            if (existingRelation?.Child == null)
            {
                return false;
            }

            return existingRelation.Child.PackageId == newComponent.PackageId &&
                   existingRelation.Child.Name == newComponent.Name &&
                   existingRelation.SlotName == newSlotName &&
                   existingRelation.ParentId == parentId &&
                   existingRelation.Order == order;
        }

        /// <summary>
        /// 既存の子要素をキー別にマッピング
        /// </summary>
        private static Dictionary<string, ComponentRelation> BuildExistingChildrenMap(ComponentRelation existingRelation)
        {
            var map = new Dictionary<string, ComponentRelation>();

            if (existingRelation?.Child?.Children == null)
            {
                return map;
            }

            foreach (var child in existingRelation.Child.Children)
            {
                var key = CreateChildRelationKey(
                    child.Child?.PackageId.ToString() ?? "",
                    child.Child?.Name ?? "",
                    child.SlotName);

                map[key] = child;
            }

            return map;
        }

        /// <summary>
        /// 新しい子要素を処理・マージ
        /// </summary>
        private async Task MergeChildrenAsync(
            ComponentTreeNode newNode,
            ComponentRelation relation,
            ComponentRelation existingRelation,
            Dictionary<string, ComponentRelation> existingChildrenMap,
            CancellationToken cancellationToken)
        {
            if (newNode?.Children == null || newNode.Children.Count == 0)
            {
                return;
            }

            var processedChildKeys = new HashSet<string>();
            int childOrder = 0;

            foreach (var newChild in newNode.Children)
            {
                var childComponent = await ExtractComponentAsync(newChild.ComponentId, cancellationToken);
                if (childComponent == null)
                {
                    continue;
                }

                var childSlotName = newChild.SlotName ?? "Main";
                var childKey = CreateChildRelationKey(childComponent.PackageId.ToString(), childComponent.Name, childSlotName);
                processedChildKeys.Add(childKey);

                var existingChild = existingChildrenMap.TryGetValue(childKey, out var found) ? found : null;

                // 再帰的にマージ
                await MergeComponentRelationTree(newChild, existingChild, relation.ChildId, childOrder++, cancellationToken);
            }

            // 処理されなかった子要素は削除対象
            await DeleteObsoleteChildrenAsync(existingChildrenMap, processedChildKeys, cancellationToken);
        }

        /// <summary>
        /// 処理されなかった子要素を削除
        /// </summary>
        private async Task DeleteObsoleteChildrenAsync(
            Dictionary<string, ComponentRelation> existingChildrenMap,
            HashSet<string> processedChildKeys,
            CancellationToken cancellationToken)
        {
            var obsoleteKeys = existingChildrenMap.Keys.Where(k => !processedChildKeys.Contains(k));

            foreach (var key in obsoleteKeys)
            {
                var relationToDelete = existingChildrenMap[key];
                await DeleteComponentRelationTree(relationToDelete, cancellationToken);
            }
        }

        /// <summary>
        /// 既存の子要素をすべて削除
        /// </summary>
        private async Task DeleteAllChildrenAsync(ComponentRelation existingRelation, CancellationToken cancellationToken)
        {
            if (existingRelation == null)
            {
                return;
            }

            // ParentIdがexistingRelation.ChildIdである全てのRelationを削除
            var parentId = existingRelation.ChildId;

            // データベースから削除対象の子要素を全て取得
            var allRelations = await _componentRelationRepository.GetAllAsync(cancellationToken);
            var childrenToDelete = allRelations
                .Where(r => r.ParentId == parentId)
                .ToList();

            // 各子要素に対してツリー削除を実行
            foreach (var childRelation in childrenToDelete)
            {
                await DeleteComponentRelationTree(childRelation, cancellationToken);
            }
        }

        /// <summary>
        /// コンポーネント情報とそのスロット情報を取得
        /// </summary>
        private async Task<List<ComponentWithSlotsDto>> BuildComponentsWithSlots(List<Component> components, CancellationToken cancellationToken)
        {
            var result = new List<ComponentWithSlotsDto>();

            foreach (var component in components)
            {
                var dto = new ComponentWithSlotsDto
                {
                    Id = component.Id,
                    Name = component.Name,
                    PackageId = component.PackageId,
                    Slots = new List<string>()
                };

                try
                {
                    // アセンブリをロード
                    var assembly = AssemblyService.GetAssembly(component.PackageId);
                    if (assembly != null)
                    {
                        // コンポーネント型を取得
                        var componentType = AssemblyService.GetType(component.PackageId, component.Name);
                        if (componentType != null)
                        {
                            Console.WriteLine($"[DEBUG] Analyzing slots for component: {component.Name}");
                            // 特定のコンポーネント型のスロット情報を取得
                            // assemblyBasePath を指定しない（DLL の埋め込みリソースから読む）
                            var slots = _slotAnalyzerService.AnalyzeComponentSlots(componentType, null);
                            var slotNames = slots
                                .Select(slot => slot.SlotName)
                                .Distinct()
                                .ToList();

                            Console.WriteLine($"[DEBUG] Found {slotNames.Count} slots: {string.Join(", ", slotNames)}");
                            dto.Slots = slotNames;
                        }
                        else
                        {
                            Console.WriteLine($"[DEBUG] Component type not found for: {component.Name}");
                            dto.Slots = new List<string>();
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[DEBUG] Assembly not found for PackageId: {component.PackageId}");
                        dto.Slots = new List<string>();
                    }
                }
                catch (Exception ex)
                {
                    // スロット取得に失敗した場合は空リスト
                    Console.WriteLine($"[ERROR] Error analyzing slots for component {component.Name}: {ex.Message}");
                    Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                    dto.Slots = new List<string>();
                }

                result.Add(dto);
            }

            return result;
        }
    }

    public class AddComponentRequest
    {
        public required string PageTemplateId { get; set; }
        public required string ComponentId { get; set; }
        public List<int>? ParentPath { get; set; }
        public string? SlotName { get; set; }
    }

    public class RemoveComponentRequest
    {
        public required string PageTemplateId { get; set; }
        public List<int>? Path { get; set; }
    }

    public class SaveComponentTreeRequest
    {
        public required string PageTemplateId { get; set; }
        public List<ComponentTreeNode>? Tree { get; set; }
    }

    public class SavePageTemplateRequest
    {
        public required string PageTemplateId { get; set; }
        public List<ComponentTreeNode>? Tree { get; set; }
    }

    public class ComponentTreeNode
    {
        public required string ComponentId { get; set; }
        public required string ComponentName { get; set; }
        public required string PackageId { get; set; }
        public required string SlotName { get; set; }
        public List<ComponentTreeNode> Children { get; set; } = new List<ComponentTreeNode>();
    }
}
