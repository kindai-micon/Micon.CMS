using Micon.CMS.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Micon.CMS.Repositories
{
    public class PageTemplateRepository(ApplicationDbContext dbContext) : BaseRepository<PageTemplate>(dbContext) , IPageTemplateRepository
    {

        public Task<List<PageCategory>> GetPageCategoriesAsync(PageTemplate pageTemplate, CancellationToken cancellationToken)
        {
            return dbContext.PageTemplates.Where(x => x.Id == pageTemplate.Id)
                .Select(x => x.PageCategory)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ComponentHierarchy>> GetComponentHierarchy(PageTemplate pageTemplate, CancellationToken cancellationToken)
        {
            string componentHierarchyQuery = @"
    WITH RECURSIVE ComponentHierarchy AS (
        SELECT 
            cr.""Id"",
            cr.""ParentId"",
            cr.""ChildId"",
            cr.""Order"",
            1 AS Level,
            p.""Id"" AS ParentComponentId,
            p.""Name"" AS ParentComponentName,
            p.""PackageId"" AS ParentComponentPackageId,
            c.""Id"" AS ChildComponentId,
            c.""Name"" AS ChildComponentName,
            c.""PackageId"" As ChildComponentPackageId
        FROM ""ComponentRelations"" cr
        LEFT JOIN ""Components"" p ON p.""Id"" = cr.""ParentId""
        LEFT JOIN ""Components"" c ON c.""Id"" = cr.""ChildId""
        WHERE cr.""Id"" = @ComponentRelationId

        UNION ALL

        SELECT 
            cr.""Id"",
            cr.""ParentId"",
            cr.""ChildId"",
            cr.""Order"",
            ch.Level + 1 AS Level,
            p.""Id"" AS ParentComponentId,
            p.""Name"" AS ParentComponentName,
            p.""PackageId"" AS ParentComponentPackageId,
            c.""Id"" AS ChildComponentId,
            c.""Name"" AS ChildComponentName,
            c.""PackageId"" As ChildComponentPackageId
        FROM ""ComponentRelations"" cr
        INNER JOIN ComponentHierarchy ch ON cr.""ParentId"" = ch.""ChildId""
        LEFT JOIN ""Components"" p ON p.""Id"" = cr.""ParentId""
        LEFT JOIN ""Components"" c ON c.""Id"" = cr.""ChildId""
    )
    SELECT * FROM ComponentHierarchy ORDER BY Level, ""Order"";";

            var parameter = new Npgsql.NpgsqlParameter("@ComponentRelationId", pageTemplate.ComponentRelationId);

            return await dbContext.Set<ComponentHierarchy>()
                                  .FromSqlRaw(componentHierarchyQuery, parameter)
                                  .ToListAsync(cancellationToken);
        }

        public async Task SetPageTemplateComponentAsync(PageTemplate pageTemplate, ComponentRelation componentRelation, CancellationToken cancellationToken)
        {
            pageTemplate.ComponentRelationId = componentRelation.Id;
            await UpdateAsync(pageTemplate, cancellationToken);
        }
        public async Task SetPageTemplateComponentAsync(Guid pageTemplateId, ComponentRelation componentRelation, CancellationToken cancellationToken)
        {
            PageTemplate pageTemplate = await dbContext.PageTemplates.Where(x => x.Id == pageTemplateId).FirstOrDefaultAsync(cancellationToken);
            pageTemplate.ComponentRelationId= componentRelation.Id;
            await UpdateAsync(pageTemplate, cancellationToken);
        }
        public async Task<List<PageTemplate>> GetAllWithCategoryAsync(CancellationToken cancellationToken)
        {
            return await dbContext.PageTemplates.Include(x => x.PageCategory).ToListAsync(cancellationToken);
        }
    }
}
