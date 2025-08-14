using Micon.CMS.Models;

namespace Micon.CMS.Repositories
{
    public interface IPageRepository : IBaseRepository<Page>
    {
        Task<Page?> GetPageByNameAsync(string pageName, CancellationToken cancellationToken = default);
        Task<PageTemplate?> GetPageTemplateAsync(Page page, CancellationToken cancellationToken);
        Task<List<PageHistory>> GetPageHistoriesAsync(Page page, CancellationToken cancellationToken);
        Task AddPageHistoryAsync(Page page, ApplicationUser user, string comment, CancellationToken cancellationToken);
        Task AddPageHistoryAsync(Page page, ApplicationUser user, CancellationToken cancellationToken);
        Task<List<Page>> GetPagesByTemplateAsync(PageTemplate pageTemplate, CancellationToken cancellationToken);
    }
}

