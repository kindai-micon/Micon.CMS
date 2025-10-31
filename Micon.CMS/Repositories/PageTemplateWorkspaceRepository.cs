using Micon.CMS.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Micon.CMS.Repositories
{
    public interface IPageTemplateWorkspaceRepository
    {
        Task<PageTemplateWorkspace> GetByPageTemplateIdAsync(Guid pageTemplateId, CancellationToken cancellationToken);
        Task<PageTemplateWorkspace> CreateAsync(PageTemplateWorkspace workspace, CancellationToken cancellationToken);
        Task<PageTemplateWorkspace> UpdateAsync(PageTemplateWorkspace workspace, CancellationToken cancellationToken);
        Task DeleteAsync(PageTemplateWorkspace workspace, CancellationToken cancellationToken);
    }

    public class PageTemplateWorkspaceRepository(ApplicationDbContext dbContext) : BaseRepository<PageTemplateWorkspace>(dbContext), IPageTemplateWorkspaceRepository
    {
        public async Task<PageTemplateWorkspace> GetByPageTemplateIdAsync(Guid pageTemplateId, CancellationToken cancellationToken)
        {
            return await dbContext.PageTemplateWorkspaces
                .Where(w => w.PageTemplateId == pageTemplateId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<PageTemplateWorkspace> CreateAsync(PageTemplateWorkspace workspace, CancellationToken cancellationToken)
        {
            workspace.UpdatedAt = DateTime.UtcNow;
            dbContext.PageTemplateWorkspaces.Add(workspace);
            await dbContext.SaveChangesAsync(cancellationToken);
            return workspace;
        }

        public async Task<PageTemplateWorkspace> UpdateAsync(PageTemplateWorkspace workspace, CancellationToken cancellationToken)
        {
            workspace.UpdatedAt = DateTime.UtcNow;
            dbContext.PageTemplateWorkspaces.Update(workspace);
            await dbContext.SaveChangesAsync(cancellationToken);
            return workspace;
        }

        public async Task DeleteAsync(PageTemplateWorkspace workspace, CancellationToken cancellationToken)
        {
            dbContext.PageTemplateWorkspaces.Remove(workspace);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
