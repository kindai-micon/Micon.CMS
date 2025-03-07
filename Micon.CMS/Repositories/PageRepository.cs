using Micon.CMS.Models;
using Microsoft.EntityFrameworkCore;

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
            return dbContext.Pages.Where(x=>x.Id == guid).FirstOrDefault();
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
        public PageTemplate? GetPageTemplate(Page page)
        {
            return dbContext.PageTemplates
                .Where(x => x.Id == page.PageTemplateId)
                .FirstOrDefault();
        }

        public List<PageHistory> GetPageHistories(Page page)
        {
            return dbContext.PageHistories
                .Where(x => x.PageId == page.Id)
                .Include(x=>x.ApplicationUser)
                .OrderByDescending(x=>x.Modified)
                .ToList();
        }

        public void AddPageHistory(Page page, ApplicationUser user, string comment = null)
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
            dbContext.SaveChanges();
        }
    }
}
