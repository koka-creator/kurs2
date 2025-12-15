using System.Collections.Generic;

namespace FreightLogistics.App.Repositories
{
    public interface IRepository<T>
    {
        T Add(T entity);
        void Update(T entity);
        void Delete(int id);
        T? GetById(int id);
        IEnumerable<T> GetAll();
    }
}
