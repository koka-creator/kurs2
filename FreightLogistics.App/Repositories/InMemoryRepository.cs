using System;
using System.Collections.Generic;
using System.Linq;

namespace FreightLogistics.App.Repositories
{
    public class InMemoryRepository<T> : IRepository<T> where T : class
    {
        private readonly Func<T, int> _getId;
        private readonly Action<T, int> _setId;
        private readonly Dictionary<int, T> _storage = new Dictionary<int, T>();
        private int _nextId = 1;

        public InMemoryRepository(Func<T, int> getId, Action<T, int> setId)
        {
            _getId = getId;
            _setId = setId;
        }

        public T Add(T entity)
        {
            var id = _getId(entity);
            if (id == 0)
            {
                id = _nextId++;
                _setId(entity, id);
            }
            _storage[id] = entity;
            return entity;
        }

        public void Update(T entity)
        {
            var id = _getId(entity);
            if (id == 0 || !_storage.ContainsKey(id))
                throw new InvalidOperationException("Entity does not exist in repository");
            _storage[id] = entity;
        }

        public void Delete(int id)
        {
            _storage.Remove(id);
        }

        public T? GetById(int id)
        {
            return _storage.TryGetValue(id, out var entity) ? entity : null;
        }

        public IEnumerable<T> GetAll()
        {
            return _storage.Values.ToList();
        }

        public void LoadData(IEnumerable<T> data, int maxId)
        {
            _storage.Clear();
            foreach (var item in data)
            {
                var id = _getId(item);
                _storage[id] = item;
            }
            _nextId = maxId + 1;
        }
    }
}
