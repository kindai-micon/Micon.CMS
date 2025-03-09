using Micon.CMS.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Micon.CMS.Repositories
{
    public class PageRepository(ApplicationDbContext dbContext) : BaseRepository<Page>(dbContext), IPageRepository
    {

        public override Page Create(Page page)
        {
            return base.Create(page);
        }

        public override Page Create()
        {
            return base.Create();
        }
        public override Task<Page> CreateAsync(CancellationToken cancellationToken)
        {
            return base.CreateAsync(cancellationToken);
        }
        public override Task<Page> CreateAsync(Page model, CancellationToken cancellationToken)
        {
            return base.CreateAsync(model, cancellationToken);
        }
        public override void Delete(Page model)
        {
            base.Delete(model);
        }
        public override Task DeleteAsync(Page model, CancellationToken cancellationToken)
        {
            return base.DeleteAsync(model, cancellationToken);
        }
        public override Page? GetById(Guid guid)
        {
            return dbContext.Pages.Where(x => x.Id == guid).FirstOrDefault();
        }
        public override Task<Page?> GetByIdAsync(Guid guid, CancellationToken cancellationToken)
        {
            return dbContext.Pages.Where(x => x.Id == guid).FirstOrDefaultAsync(cancellationToken);
        }
        public override Page Update(Page model)
        {
            return Update(model);
        }
        public override Task<Page> UpdateAsync(Page model, CancellationToken cancellationToken)
        {
            return UpdateAsync(model, cancellationToken);
        }
        public Task<PageTemplate?> GetPageTemplateAsync(Page page, CancellationToken cancellationToken)
        {
            return dbContext.PageTemplates
                .Where(x => x.Id == page.PageTemplateId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public Task<List<PageHistory>> GetPageHistoriesAsync(Page page, CancellationToken cancellationToken)
        {
            return dbContext.PageHistories
                .Where(x => x.PageId == page.Id)
                .Include(x => x.ApplicationUser)
                .OrderByDescending(x => x.Modified)
                .ToListAsync(cancellationToken);
        }

        public async Task AddPageHistoryAsync(Page page, ApplicationUser user, string comment, CancellationToken cancellationToken)
        {
            var pageHistory = new PageHistory
            {
                PageId = page.Id,
                Page = page,
                ApplicationUser = user,
                Modified = DateTimeOffset.Now,
                Created = DateTimeOffset.Now,
                Comment = comment
            };
            dbContext.PageHistories.Add(pageHistory);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        public async Task AddPageHistoryAsync(Page page, ApplicationUser user, CancellationToken cancellationToken)
        {
            var pageHistory = new PageHistory
            {
                PageId = page.Id,
                Page = page,
                ApplicationUser = user,
                Modified = DateTimeOffset.Now,
                Created = DateTimeOffset.Now,
            };
            dbContext.PageHistories.Add(pageHistory);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public Task<List<Page>> GetPagesByTemplateAsync(PageTemplate pageTemplate, CancellationToken cancellationToken)
        {
            return dbContext.Pages
                .Where(x => x.PageTemplateId == pageTemplate.Id)
                .ToListAsync(cancellationToken);
        }

        //public Task<Page?> GetByIdWithComponents(Guid guid)
        //{
        //    dbContext.Database.SqlQueryRaw<ComponentRelationHierarchyDto>(query,new NpgsqlParameter(,))
        //}
        string query = @"
        WITH RECURSIVE ComponentHierarchy AS (
            -- 基点となるコンポーネント（RelationはNULL）
            SELECT 
                c.""Id"" as ComponentId,
                NULL::uuid as RelationId,
                NULL::uuid as ParentId,
                c.""PackageId"",
                0 as ""Level"",
                0 as ""Order""
            FROM ""Components"" c
            WHERE c.""Id"" = @startId

            UNION ALL

            -- 子コンポーネントとその関係を再帰的に取得
            SELECT 
                child.""Id"" as ComponentId,
                cr.""Id"" as RelationId,
                cr.""ParentId"",
                child.""PackageId"",
                ch.""Level"" + 1,
                cr.""Order""
            FROM ""ComponentRelations"" cr
            INNER JOIN ""Components"" child ON cr.""ChildId"" = child.""Id""
            INNER JOIN ComponentHierarchy ch ON cr.""ParentId"" = ch.ComponentId
        )
        SELECT 
            h.ComponentId,
            h.RelationId,
            h.ParentId,
            h.""Level"",
            h.""Order"",
            c.*,
            r.*
        FROM ComponentHierarchy h
        JOIN ""Components"" c ON h.ComponentId = c.""Id""
        LEFT JOIN ""ComponentRelations"" r ON h.RelationId = r.""Id""
        ORDER BY h.""Level"", h.""Order"";";
    }
}
