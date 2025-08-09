using Micon.CMS.Models;
using Microsoft.EntityFrameworkCore;

namespace Micon.CMS.Repositories
{
    public class ComponentRelationRepository(ApplicationDbContext dbContext) : BaseRepository<ComponentRelation>(dbContext), IComponentRelationRepository
    {
        public Task<ComponentRelation?> GetChild(ComponentRelation componentRelation, Page page, CancellationToken cancellationToken)
        {
            return dbContext.ComponentRelations
                .Where(x => x.Id == componentRelation.Id)
                .Include(x => x.Child)
                .Include(x => x.Child.Children)
                .Include(x => x.ComponentSettings.Where(x => x.PageId == page.Id))
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
