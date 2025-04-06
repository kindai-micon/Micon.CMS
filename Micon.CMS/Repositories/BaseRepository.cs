using Micon.CMS.Models;
using Microsoft.EntityFrameworkCore;

namespace Micon.CMS.Repositories
{
    public class BaseRepository<T>(ApplicationDbContext dbContext) : IBaseRepository<T> where T : BaseModel, new()
    {
        public virtual T Create()
        {
            var model = new T();
            return Create(model);
        }

        public virtual T Create(T model)
        {
            model.Created = DateTimeOffset.Now;
            model.Modified = DateTimeOffset.Now;
            model.Id = Guid.CreateVersion7();
            dbContext.Add(model);
            dbContext.SaveChanges();
            return model;
        }

        public virtual Task<T> CreateAsync(CancellationToken cancellationToken)
        {
            var model = new T();
            return CreateAsync(model, cancellationToken);
        }

        public virtual async Task<T> CreateAsync(T model, CancellationToken cancellationToken)
        {
            model.Created = DateTimeOffset.Now;
            model.Modified = DateTimeOffset.Now;
            model.Id = Guid.CreateVersion7();
            await dbContext.AddAsync(model,cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return model;
        }

        public virtual void Delete(T model)
        {
            dbContext.Remove(model);
            dbContext.SaveChanges();
        }

        public virtual async Task DeleteAsync(T model, CancellationToken cancellationToken)
        {
            dbContext.Remove(model);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual T? GetById(Guid guid)
        {
           return dbContext.Set<T>()
                .Where(x=>x.Id == guid)
                .FirstOrDefault();
        }

        public virtual Task<T?> GetByIdAsync(Guid guid, CancellationToken cancellationToken)
        {
            return dbContext.Set<T>()
                .Where(x => x.Id == guid)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public virtual T Update(T model)
        {
            model.Modified = DateTimeOffset.Now;
            var newModel = dbContext.Update(model).Entity;
            dbContext.SaveChanges();
            return newModel;
        }

        public virtual async Task<T> UpdateAsync(T model, CancellationToken cancellationToken)
        {
            model.Modified = DateTimeOffset.Now;
            var newModel = dbContext.Update(model).Entity;
            await dbContext.SaveChangesAsync(cancellationToken);
            return newModel;
        }

        public virtual Task<List<T>> GetAllAsync(CancellationToken cancellationToken)
        {
            return dbContext.Set<T>().ToListAsync(cancellationToken);
        }
        public virtual List<T> GetAllAsync()
        {
            return dbContext.Set<T>().ToList();
        }
    }
}
