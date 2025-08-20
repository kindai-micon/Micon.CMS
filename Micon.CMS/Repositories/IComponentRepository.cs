using Micon.CMS.Models;
using Microsoft.EntityFrameworkCore;

namespace Micon.CMS.Repositories
{
    public interface IComponentRepository : IBaseRepository<Component>
    {
        public Task<ComponentSetting?> GetSettingAsync(Page page, Component component, string key, CancellationToken cancellationToken);
        public Task<List<ComponentSetting>> GetSettingsAsync(Page page, Component component, CancellationToken cancellationToken);
        public Task UpdateSetting(ComponentSetting componentSetting, CancellationToken cancellationToken);
        public Task CreateSetting(Page page, Component component, string key, CancellationToken cancellationToken);

    }
}

