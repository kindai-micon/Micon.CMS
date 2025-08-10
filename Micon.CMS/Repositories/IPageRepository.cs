using Micon.CMS.Models;

namespace Micon.CMS.Repositories
{
    public interface IPageRepository : IBaseRepository<Page>
    {
        Task<PageTemplate?> GetPageTemplateAsync(Page page, CancellationToken cancellationToken);
        Task<List<PageHistory>> GetPageHistoriesAsync(Page page,ApplicationUser user);
        Task<List<PageHistory>> GetPageHistoriesBetweenAsync(Page page,ApplicationUser user,DateTimeOffset StartTime,DateTimeOffset EndTime);
        Task<List<PageHistory>> GetPageHistoriesBeforeAsync(Page page,ApplicationUser user,DateTimeOffset dateTime);
        Task<List<PageHistory>> GetPageHistoriesAfterAsync(Page page,ApplicationUser user,DateTimeOffset dateTime);
        Task AddPageHistoryAsync(Page page, ApplicationUser user, string comment, CancellationToken cancellationToken);
        Task AddPageHistoryAsync(Page page, ApplicationUser user, CancellationToken cancellationToken);
        Task<List<Page>> GetPagesByTemplateAsync(PageTemplate pageTemplate, CancellationToken cancellationToken);

    }
}
