using Micon.CMS.Models;
using Micon.CMS.Models.Api;
using Micon.CMS.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Micon.CMS.Services
{
    /// <summary>
    /// ComponentRelationTree の操作を管理するサービス
    /// </summary>
    public class ComponentTreeService
    {
        private readonly IComponentRelationRepository _componentRelationRepository;
        private readonly IComponentRepository _componentRepository;

        public ComponentTreeService(
            IComponentRelationRepository componentRelationRepository,
            IComponentRepository componentRepository)
        {
            _componentRelationRepository = componentRelationRepository;
            _componentRepository = componentRepository;
        }

        /// <summary>
        /// ComponentHierarchy からツリー構造を構築
        /// </summary>
        public async Task<List<ComponentTreeNode>> BuildComponentTreeFromHierarchy(
            List<ComponentHierarchy> hierarchy,
            CancellationToken cancellationToken)
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

        /// <summary>
        /// ツリーをマージして差分管理で保存
        /// </summary>
        public async Task<ComponentRelation> MergeComponentRelationTree(
            ComponentTreeNode newNode,
            ComponentRelation existingRelation,
            Guid? parentId,
            int order,
            CancellationToken cancellationToken)
        {
            var component = await ExtractComponentAsync(newNode.PackageId, newNode.ComponentName, cancellationToken);
            if (component == null)
            {
                return null;
            }

            var slotName = newNode.SlotName ?? "Main";
            var isSameStructure = IsRelationUnchanged(existingRelation, component, slotName, parentId, order);

            var relation = isSameStructure
                ? existingRelation
                : await CreateComponentRelationAsync(component, slotName, parentId, order, cancellationToken);

            var existingChildrenMap = BuildExistingChildrenMap(existingRelation);

            if (newNode.Children != null && newNode.Children.Count > 0)
            {
                await MergeChildrenAsync(newNode, relation, existingRelation, existingChildrenMap, cancellationToken);
            }
            else if (existingRelation != null)
            {
                await DeleteAllChildrenAsync(existingRelation, cancellationToken);
            }

            return relation;
        }

        /// <summary>
        /// ComponentRelationツリーを削除
        /// </summary>
        public async Task DeleteComponentRelationTree(
            ComponentRelation relation,
            CancellationToken cancellationToken)
        {
            await DeleteComponentRelationTreeWithTracking(relation, new HashSet<Guid>(), cancellationToken);
        }

        private async Task<ComponentTreeNode> BuildNodeTree(
            ComponentHierarchy hierarchyItem,
            List<ComponentHierarchy> allHierarchy,
            CancellationToken cancellationToken)
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

        private Dictionary<string, ComponentRelation> BuildExistingChildrenMap(ComponentRelation existingRelation)
        {
            var map = new Dictionary<string, ComponentRelation>();

            if (existingRelation?.Child?.Children == null || existingRelation.Child.Children.Count == 0)
            {
                return map;
            }

            foreach (var child in existingRelation.Child.Children)
            {
                var key = CreateChildRelationKey(
                    child.Child.PackageId.ToString(),
                    child.Child.Name,
                    child.SlotName);
                map[key] = child;
            }

            return map;
        }

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
                var childComponent = await ExtractComponentAsync(newChild.PackageId, newChild.ComponentName, cancellationToken);
                if (childComponent == null)
                {
                    continue;
                }

                var childSlotName = newChild.SlotName ?? "Main";
                var childKey = CreateChildRelationKey(childComponent.PackageId.ToString(), childComponent.Name, childSlotName);
                processedChildKeys.Add(childKey);

                var existingChild = existingChildrenMap.TryGetValue(childKey, out var found) ? found : null;

                await MergeComponentRelationTree(newChild, existingChild, relation.ChildId, childOrder++, cancellationToken);
            }

            await DeleteObsoleteChildrenAsync(existingChildrenMap, processedChildKeys, cancellationToken);
        }

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

        private async Task DeleteAllChildrenAsync(
            ComponentRelation existingRelation,
            CancellationToken cancellationToken)
        {
            if (existingRelation?.Child?.Children == null || existingRelation.Child.Children.Count == 0)
            {
                return;
            }

            var childrenToDelete = existingRelation.Child.Children.ToList();
            foreach (var child in childrenToDelete)
            {
                await DeleteComponentRelationTree(child, cancellationToken);
            }
        }

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

            if (relation.Child?.Children != null && relation.Child.Children.Count > 0)
            {
                var childrenToDelete = relation.Child.Children.ToList();
                foreach (var child in childrenToDelete)
                {
                    await DeleteComponentRelationTreeWithTracking(child, deletedIds, cancellationToken);
                }
            }

            await _componentRelationRepository.DeleteAsync(relation, cancellationToken);
        }

        private async Task<Component> ExtractComponentAsync(
            string packageId,
            string componentName,
            CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(packageId, out var parsedPackageId))
            {
                return null;
            }

            var allComponents = await _componentRepository.GetAllAsync(cancellationToken);
            var component = allComponents.FirstOrDefault(c => c.PackageId == parsedPackageId && c.Name == componentName);

            if (component == null)
            {
                component = new Component
                {
                    Id = Guid.NewGuid(),
                    PackageId = parsedPackageId,
                    Name = componentName,
                    TenantId = Guid.Empty
                };

                await _componentRepository.CreateAsync(component, cancellationToken);
            }

            return component;
        }

        private string CreateChildRelationKey(string packageId, string componentName, string slotName)
        {
            return $"{packageId}:{componentName}:{slotName}";
        }
    }
}
