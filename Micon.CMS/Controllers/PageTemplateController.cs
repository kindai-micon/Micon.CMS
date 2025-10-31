using Micon.CMS.Models;
using Micon.CMS.Models.Api;
using Micon.CMS.Repositories;
using Micon.CMS.Services;
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
        private readonly IComponentCacheService _componentCacheService;
        private readonly ComponentTreeService _treeService;
        private readonly WorkspaceService _workspaceService;

        public PageTemplateController(
            IPageTemplateRepository pageTemplateRepository,
            IComponentRepository componentRepository,
            IComponentRelationRepository componentRelationRepository,
            IPageTemplateWorkspaceRepository workspaceRepository,
            IViewComponentHelper viewComponentHelper,
            ComponentSlotAnalyzerService slotAnalyzerService,
            IComponentCacheService componentCacheService,
            ComponentTreeService treeService,
            WorkspaceService workspaceService)
        {
            _pageTemplateRepository = pageTemplateRepository;
            _componentRepository = componentRepository;
            _componentRelationRepository = componentRelationRepository;
            _workspaceRepository = workspaceRepository;
            _viewComponentHelper = viewComponentHelper;
            _slotAnalyzerService = slotAnalyzerService;
            _componentCacheService = componentCacheService;
            _treeService = treeService;
            _workspaceService = workspaceService;
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
            var pageTemplate = await _pageTemplateRepository.GetByIdAsync(Id, cancellationToken);
            if (pageTemplate == null)
            {
                return RedirectToAction(nameof(Index));
            }

            // Workspace を取得または作成
            var workspace = await _workspaceService.GetOrCreateWorkspaceAsync(Id, pageTemplate, cancellationToken);
            var initialTree = _workspaceService.GetTreeFromWorkspace(workspace);

            // コンポーネント情報を取得
            var components = await _componentCacheService.GetCachedComponentsAsync();
            var componentsWithSlots = await BuildComponentsWithSlots(components, cancellationToken);
            ViewBag.Components = componentsWithSlots;
            ViewBag.ComponentHierarchy = initialTree;

            // プレビュー用ViewModel を構築
            var previewViewModel = await BuildPageComponentViewModelFromWorkspace(initialTree, pageTemplate.Id, cancellationToken);
            ViewBag.PreviewViewModel = previewViewModel;

            return View(pageTemplate);
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
                if (request == null || !Guid.TryParse(request.PageTemplateId, out var pageTemplateId) || !Guid.TryParse(request.ComponentId, out var componentId))
                {
                    return BadRequest("Invalid request parameters");
                }

                var pageTemplate = await _pageTemplateRepository.GetByIdAsync(pageTemplateId, cancellationToken);
                if (pageTemplate == null)
                {
                    return NotFound("PageTemplate not found");
                }

                var cachedComponents = await _componentCacheService.GetCachedComponentsAsync();
                var component = cachedComponents.FirstOrDefault(c => c.Id == componentId);
                if (component == null)
                {
                    return BadRequest("Component not found");
                }

                // Workspace を取得または作成
                var workspace = await _workspaceService.GetOrCreateWorkspaceAsync(pageTemplateId, pageTemplate, cancellationToken);
                var tree = _workspaceService.GetTreeFromWorkspace(workspace);

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
                    tree.Add(newNode);
                }
                else
                {
                    var parent = GetNodeByPath(tree, request.ParentPath);
                    if (parent == null)
                    {
                        return BadRequest("Parent node not found");
                    }

                    var targetSlot = newNode.SlotName ?? "Main";
                    if (parent.Children.Any(c => string.Equals(c.SlotName ?? "Main", targetSlot, StringComparison.OrdinalIgnoreCase)))
                    {
                        return BadRequest("指定したスロットには既にコンポーネントが配置されています。先に削除してください。");
                    }

                    parent.Children.Add(newNode);
                }

                // Workspace を保存
                await _workspaceService.SaveTreeToWorkspaceAsync(workspace, tree, cancellationToken);

                return Ok(new { tree });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveComponent([FromBody] RemoveComponentRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (request == null || !Guid.TryParse(request.PageTemplateId, out var pageTemplateId) || request.Path == null || request.Path.Count == 0)
                {
                    return BadRequest("Invalid request parameters");
                }

                var pageTemplate = await _pageTemplateRepository.GetByIdAsync(pageTemplateId, cancellationToken);
                if (pageTemplate == null)
                {
                    return NotFound("PageTemplate not found");
                }

                // Workspace を取得または作成
                var workspace = await _workspaceService.GetOrCreateWorkspaceAsync(pageTemplateId, pageTemplate, cancellationToken);
                var tree = _workspaceService.GetTreeFromWorkspace(workspace);

                if (!RemoveNodeFromTree(tree, request.Path))
                {
                    return BadRequest("Target node not found");
                }

                // Workspace を保存
                await _workspaceService.SaveTreeToWorkspaceAsync(workspace, tree, cancellationToken);

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
                if (request == null || !Guid.TryParse(request.PageTemplateId, out var pageTemplateId))
                {
                    return BadRequest("Invalid request");
                }

                var pageTemplate = await _pageTemplateRepository.GetByIdAsync(pageTemplateId, cancellationToken);
                if (pageTemplate == null)
                {
                    return NotFound("PageTemplate not found");
                }

                var workspace = await _workspaceRepository.GetByPageTemplateIdAsync(pageTemplateId, cancellationToken);

                // ツリーが空の場合
                if (request.Tree == null || request.Tree.Count == 0)
                {
                    if (pageTemplate.ComponentRelationId.HasValue)
                    {
                        var existingRelation = await _componentRelationRepository.GetByIdAsync(pageTemplate.ComponentRelationId.Value, cancellationToken);
                        if (existingRelation != null)
                        {
                            await _treeService.DeleteComponentRelationTree(existingRelation, cancellationToken);
                        }
                    }

                    pageTemplate.ComponentRelationId = null;
                    await _pageTemplateRepository.UpdateAsync(pageTemplate, cancellationToken);
                    await _workspaceService.DeleteWorkspaceAsync(workspace, cancellationToken);

                    return Ok(new { message = "PageTemplate saved successfully (empty tree)" });
                }

                // 既存の ComponentRelation を取得
                ComponentRelation existingRootRelation = null;
                if (pageTemplate.ComponentRelationId.HasValue)
                {
                    existingRootRelation = await _componentRelationRepository.GetByIdAsync(pageTemplate.ComponentRelationId.Value, cancellationToken);
                }

                // ツリーを差分管理で保存
                var rootNode = request.Tree[0];
                var rootRelation = await _treeService.MergeComponentRelationTree(rootNode, existingRootRelation, null, 0, cancellationToken);

                if (rootRelation != null)
                {
                    pageTemplate.ComponentRelationId = rootRelation.Id;
                    await _pageTemplateRepository.UpdateAsync(pageTemplate, cancellationToken);
                }

                // Workspace を削除
                await _workspaceService.DeleteWorkspaceAsync(workspace, cancellationToken);

                return Ok(new { message = "PageTemplate saved successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetComponents(CancellationToken cancellationToken)
        {
            var components = await _componentCacheService.GetCachedComponentsAsync();
            return Json(components.Select(c => new
            {
                id = c.Id,
                packageId = c.PackageId,
                name = c.Name
            }));
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
                            // 特定のコンポーネント型のスロット情報を取得
                            // assemblyBasePath を指定しない（DLL の埋め込みリソースから読む）
                            var slots = _slotAnalyzerService.AnalyzeComponentSlots(componentType, null);
                            var slotNames = slots
                                .Select(slot => slot.SlotName)
                                .Distinct()
                                .ToList();

                            dto.Slots = slotNames;
                        }
                        else
                        {
                            dto.Slots = new List<string>();
                        }
                    }
                    else
                    {
                        dto.Slots = new List<string>();
                    }
                }
                catch (Exception ex)
                {
                    // スロット取得に失敗した場合は空リスト
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
}
