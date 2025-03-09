using Micon.CMS.Models;

namespace Micon.CMS.Repositories
{
    public interface IPageTemplateRepository : IBaseRepository<PageTemplate>
    {
        Task<List<PageCategory>> GetPageCategoriesAsync(PageTemplate pageTemplate, CancellationToken cancellationToken);
    }
}
