using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Connor.Caching
{
    public interface ICache<T, Y>
    {
        Task<T> GetFirstOrDefault(Y key);
        Task<List<T>> GetList(Y key);
        Task Set(Y key, T item, TimeSpan? expiration = null);
        Task SetList(Y key, List<T> items, TimeSpan? expiration = null);
    }
}
