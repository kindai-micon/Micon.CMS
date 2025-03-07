using Micon.CMS.Models;

namespace Micon.CMS.Repositories
{
    public interface IBaseRepository<T> where T : BaseModel , new()
    {
        public T Create();
        public T Create(T model);

        public T Update(T model);

        public void Delete(T model);
        public T? GetById(Guid guid);
        
    }
}
