using Micon.CMS.Models;
using Microsoft.EntityFrameworkCore;

namespace Micon.CMS.Repositories
{
    public class PageTemplateRepository(ApplicationDbContext dbContext) : BaseRepository<PageTemplate>(dbContext) , IPageTemplateRepository
    {

        public Task<List<PageCategory>> GetPageCategoriesAsync(PageTemplate pageTemplate, CancellationToken cancellationToken)
        {
            
        }

        public async Task<List<ComponentHierarchy>> GetAllChildComponentsFromPageTemplateWithCTE(PageTemplate pageTemplate)
        {
            // 再帰的CTEを使用してすべての子コンポーネントを取得
            var components = await dbContext.Set<ComponentHierarchy>()
                .FromSqlRaw(_componentHierarchyQuery, pageTemplate.Id)
                .ToListAsync();

            return components;
        }

        const string _componentHierarchyQuery = @"
        WITH RECURSIVE ComponentHierarchy AS (
            SELECT 
                cr.""Id"",
                cr.""ParentId"",
                cr.""ChildId"",
                cr.""Order"",
                1 AS Level,
                p.""Id"" AS ParentComponentId,
                p.""Name"" AS ParentComponentName,
                c.""Id"" AS ChildComponentId,
                c.""Name"" AS ChildComponentName
            FROM ""ComponentRelation"" cr
            LEFT JOIN ""Component"" c ON c.""Id"" = cr.""ChildId""
            WHERE cr.""Id"" = (SELECT ""ComponentRelationId"" FROM ""PageTemplate"" WHERE ""Id"" = {0})

            UNION ALL

            SELECT 
                cr.""Id"",
                cr.""ParentId"",
                cr.""ChildId"",
                cr.""Order"",
                ch.Level + 1 AS Level,
                p.""Id"" AS ParentComponentId,
                p.""Name"" AS ParentComponentName,
                c.""Id"" AS ChildComponentId,
                c.""Name"" AS ChildComponentName
            FROM ""ComponentRelation"" cr
            INNER JOIN ComponentHierarchy ch ON cr.""ParentId"" = ch.""ChildId""
            LEFT JOIN ""Component"" p ON p.""Id"" = cr.""ParentId""
            LEFT JOIN ""Component"" c ON c.""Id"" = cr.""ChildId""
        )

        SELECT * FROM ComponentHierarchy;
            ";
    }
}
