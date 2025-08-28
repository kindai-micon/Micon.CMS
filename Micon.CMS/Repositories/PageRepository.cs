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
        public Task<List<PageHistory>> GetPageHistoriesAtUserAsync(Page page,ApplicationUser user, CancellationToken cancellationToken)
        {
            return dbContext.PageHistories
                .Where(x => x.PageId == page.Id && x.ApplicationUserId == user.Id)
                .Include(x => x.ApplicationUser)
                .OrderByDescending(x => x.Modified)
                .ToListAsync(cancellationToken);
        }
        public Task<List<PageHistory>> GetPageHistoriesBetweenAsync(Page page,ApplicationUser user,DateTimeOffset StartTime,DateTimeOffset EndTime, CancellationToken cancellationToken)
        {
            return dbContext.PageHistories
                .Where(x => x.PageId == page.Id && x.ApplicationUserId == user.Id &&x.Modified > StartTime.UtcDateTime && x.Modified < EndTime.UtcDateTime)
                .Include(x => x.ApplicationUser)
                .OrderByDescending(x => x.Modified)
                .ToListAsync(cancellationToken);
        }
        public Task<List<PageHistory>> GetPageHistoriesBeforeAsync(Page page,ApplicationUser user,DateTimeOffset dateTime, CancellationToken cancellationToken)
        {
            return dbContext.PageHistories
                .Where(x => x.PageId == page.Id && x.ApplicationUserId == user.Id && x.Modified < dateTime.UtcDateTime)
                .Include(x => x.ApplicationUser)
                .OrderByDescending(x => x.Modified)
                .ToListAsync(cancellationToken);
        }
        public Task<List<PageHistory>> GetPageHistoriesAfterAsync(Page page,ApplicationUser user,DateTimeOffset dateTime, CancellationToken cancellationToken)
        {
            return dbContext.PageHistories
                .Where(x => x.PageId == page.Id && x.ApplicationUserId == user.Id && x.Modified > dateTime.UtcDateTime)
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

    }
}
