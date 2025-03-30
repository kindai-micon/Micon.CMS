using Micon.CMS.Models;
using Microsoft.EntityFrameworkCore;

namespace Micon.CMS.Repositories
{
    public interface IBaseRepository<T> where T : BaseModel , new()
    {
        public T Create();
        public Task<T> CreateAsync(CancellationToken cancellationToken);

        public T Create(T model);
        public Task<T> CreateAsync(T model, CancellationToken cancellationToken);

        public T Update(T model);
        public Task<T> UpdateAsync(T model, CancellationToken cancellationToken);

        public void Delete(T model);
        public Task DeleteAsync(T model, CancellationToken cancellationToken);

        public T? GetById(Guid guid);
        public Task<T?> GetByIdAsync(Guid guid, CancellationToken cancellationToken);
        public Task<List<T>> GetAllAsync(CancellationToken cancellationToken);
        public List<T> GetAllAsync();

    }
}
