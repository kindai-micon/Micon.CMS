using Micon.CMS.Models;
using Microsoft.EntityFrameworkCore;

namespace Micon.CMS.Repositories
{
    public class ComponentRepository(ApplicationDbContext dbContext) : BaseRepository<Component>(dbContext)
    {
        public async Task<ComponentSetting?> GetSettingAsync(Page page, Component component, string key, CancellationToken cancellationToken)
        {
            return await dbContext.ComponentSettings
                .Where(x => x.ComponentId == component.Id && x.PageId == page.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }
        public async Task<List<ComponentSetting>> GetSettingsAsync(Page page, Component component, CancellationToken cancellationToken)
        {
            return await dbContext.ComponentSettings
                .Where(x => x.ComponentId == component.Id && x.PageId == page.Id)
                .ToListAsync(cancellationToken);

        }
        public void UpdateSetting(ComponentSetting componentSetting)
        {
            dbContext.Update(componentSetting);
            dbContext.SaveChangesAsync();
        }
        public async Task<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Micon.CMS.Models.ComponentSetting>> CreateSetting(Page page, Component component, string key, CancellationToken cancellationToken)
        {
            ComponentSetting componentSetting = new ComponentSetting();
            componentSetting.Key = key;
            componentSetting.Page = page;
            componentSetting.PageId = page.Id;
            componentSetting.Component = component;
            componentSetting.ComponentId = component.Id;
            return await dbContext.AddAsync(componentSetting, cancellationToken);
        }
    }
}