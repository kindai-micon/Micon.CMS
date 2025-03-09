using Micon.CMS.Models;
using Microsoft.EntityFrameworkCore;

namespace Micon.CMS.Repositories
{
    public class PageTemplateRepository(ApplicationDbContext dbContext) : BaseRepository<PageTemplate>(dbContext) , IPageTemplateRepository
    {

        public Task<List<PageCategory>> GetPageCategoriesAsync(PageTemplate pageTemplate, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }


    }
}
