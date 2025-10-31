using Micon.CMS.Models;
using Micon.CMS.Models.Api;
using Micon.CMS.Repositories;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Micon.CMS.Services
{
    /// <summary>
    /// PageTemplateWorkspace の管理を行うサービス
    /// </summary>
    public class WorkspaceService
    {
        private readonly IPageTemplateWorkspaceRepository _workspaceRepository;
        private readonly IPageTemplateRepository _pageTemplateRepository;
        private readonly ComponentTreeService _treeService;

        public WorkspaceService(
            IPageTemplateWorkspaceRepository workspaceRepository,
            IPageTemplateRepository pageTemplateRepository,
            ComponentTreeService treeService)
        {
            _workspaceRepository = workspaceRepository;
            _pageTemplateRepository = pageTemplateRepository;
            _treeService = treeService;
        }

        /// <summary>
        /// Workspace を取得または作成（DB の最新状態から）
        /// </summary>
        public async Task<PageTemplateWorkspace> GetOrCreateWorkspaceAsync(
            Guid pageTemplateId,
            PageTemplate pageTemplate,
            CancellationToken cancellationToken)
        {
            var workspace = await _workspaceRepository.GetByPageTemplateIdAsync(pageTemplateId, cancellationToken);

            if (workspace != null)
            {
                return workspace;
            }

            // Workspace がない場合は作成
            var initialTree = new List<ComponentTreeNode>();

            if (pageTemplate.ComponentRelationId.HasValue)
            {
                var hierarchy = await _pageTemplateRepository.GetComponentHierarchy(pageTemplate, cancellationToken);
                initialTree = await _treeService.BuildComponentTreeFromHierarchy(hierarchy, cancellationToken);
            }

            workspace = new PageTemplateWorkspace
            {
                PageTemplateId = pageTemplateId,
                WorkspaceData = JsonSerializer.Serialize(initialTree),
                CreatedByUserId = null,
                UpdatedAt = DateTime.UtcNow
            };

            return await _workspaceRepository.CreateAsync(workspace, cancellationToken);
        }

        /// <summary>
        /// Workspace からツリーを取得
        /// </summary>
        public List<ComponentTreeNode> GetTreeFromWorkspace(PageTemplateWorkspace workspace)
        {
            if (workspace == null)
            {
                return new List<ComponentTreeNode>();
            }

            return JsonSerializer.Deserialize<List<ComponentTreeNode>>(workspace.WorkspaceData)
                ?? new List<ComponentTreeNode>();
        }

        /// <summary>
        /// Workspace にツリーを保存
        /// </summary>
        public async Task SaveTreeToWorkspaceAsync(
            PageTemplateWorkspace workspace,
            List<ComponentTreeNode> tree,
            CancellationToken cancellationToken)
        {
            workspace.WorkspaceData = JsonSerializer.Serialize(tree);
            workspace.UpdatedAt = DateTime.UtcNow;

            await _workspaceRepository.UpdateAsync(workspace, cancellationToken);
        }

        /// <summary>
        /// Workspace を削除
        /// </summary>
        public async Task DeleteWorkspaceAsync(
            PageTemplateWorkspace workspace,
            CancellationToken cancellationToken)
        {
            if (workspace != null)
            {
                await _workspaceRepository.DeleteAsync(workspace, cancellationToken);
            }
        }
    }
}
