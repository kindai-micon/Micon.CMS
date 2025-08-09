using Micon.CMS.Models;
using Microsoft.EntityFrameworkCore;

namespace Micon.CMS.Repositories
{
    public class ComponentRelationRepository(ApplicationDbContext dbContext) : BaseRepository<ComponentRelation>(dbContext), IComponentRelationRepository
    {
    }
}
