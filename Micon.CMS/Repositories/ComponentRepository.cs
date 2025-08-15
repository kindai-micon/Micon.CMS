using Micon.CMS.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace Micon.CMS.Repositories
{
    public class ComponentRepository(ApplicationDbContext dbContext) : BaseRepository<Component>(dbContext), IComponentRelationRepository
    {
        public override Component GetSetting(ComponentSetting page, ComponentSetting component, ComponentSetting key)
        {
            return dbContext.ComponentSettings.
                Where(x => x.Component.Id == component.ComponentId && x.PageId == page.PageId).
                ToList();
        }
    }
}
