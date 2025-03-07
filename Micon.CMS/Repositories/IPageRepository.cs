using Micon.CMS.Models;

namespace Micon.CMS.Repositories
{
    public interface IPageRepository:IBaseRepository<Page>
    {
        PageTemplate? GetPageTemplate(Page page);
        List<PageHistory> GetPageHistories(Page page);
        void AddPageHistory(Page page, ApplicationUser user, string comment = null);

    }
}
