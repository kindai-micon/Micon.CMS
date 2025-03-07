using Micon.CMS.Models;

namespace Micon.CMS.Repositories
{
    public class BaseRepository<T>(ApplicationDbContext dbContext):IBaseRepository<T> where T : BaseModel, new()
    {
        public T Create()
        {
            var model = new T();
            return Create(model);
        }

        public T Create(T model)
        {
            model.Created = DateTimeOffset.Now;
            model.Modified = DateTimeOffset.Now;
            model.Id = Guid.CreateVersion7();
            dbContext.Add(model);
            dbContext.SaveChanges();
            return model;
        }

        public void Delete(T model)
        {
            dbContext.Remove(model);
            dbContext.SaveChanges();
        }

        public T? GetById(Guid guid)
        {
           return dbContext.Set<T>()
                .Where(x=>x.Id == guid)
                .FirstOrDefault();
        }

        public T Update(T model)
        {
            model.Modified = DateTimeOffset.Now;
            var newModel = dbContext.Update(model).Entity;
            dbContext.SaveChanges();
            return newModel;
        }
    }
}
