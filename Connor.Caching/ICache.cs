using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Connor.Caching
{
    public interface ICache<T, Y> where Y : notnull
    {
        Task<T?> GetFirstOrDefaultAsync(Y key);
        Task<(T? item, TimeSpan? expiration)> GetFirstOrDefaultWithExpiryAsync(Y key);
        Task<List<T>?> GetListAsync(Y key);
        Task<Dictionary<Y, T>?> GetMultipleFromPatternWithValuesAsync(string fullKeyPattern);
        List<Y>? GetKeysThatHaveValuesFromPattern(string redisPattern);
        Task SetAsync(Y key, T item, TimeSpan? expiration = null);
        Task SetListAsync(Y key, List<T> items, TimeSpan? expiration = null);
    }
}
