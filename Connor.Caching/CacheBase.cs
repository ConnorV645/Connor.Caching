using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Connor.Caching
{
    public abstract class CacheBase<T, Y> : ICache<T, Y> where T : class
    {
        protected readonly TimeSpan defaultExpiration;
        protected readonly IConnectionMultiplexer connectionMultiplexer;

        public CacheBase(IConnectionMultiplexer connectionMultiplexer, TimeSpan? defaultExpiration = null)
        {
            this.connectionMultiplexer = connectionMultiplexer;
            this.defaultExpiration = defaultExpiration ?? TimeSpan.FromHours(1);
        }

        public abstract string GetKey(Y key);

        public async Task<T> GetFirstOrDefault(Y key)
        {
            if (connectionMultiplexer.IsConnected)
            {
                var result = await connectionMultiplexer.GetDatabase().StringGetAsync(GetKey(key));
                if (result.IsNull)
                {
                    return default;
                }
                else
                {
                    return JsonConvert.DeserializeObject<T>(result);
                }
            }
            else
            {
                return default;
            }
        }

        public async Task<(T item, TimeSpan? expiration)> GetFirstOrDefaultWithExpiry(Y key)
        {
            if (connectionMultiplexer.IsConnected)
            {
                var result = await connectionMultiplexer.GetDatabase().StringGetWithExpiryAsync(GetKey(key));
                if (result.Value.IsNull)
                {
                    return (default, null);
                }
                else
                {
                    return (JsonConvert.DeserializeObject<T>(result.Value), result.Expiry);
                }
            }
            else
            {
                return default;
            }
        }

        public async Task<List<T>> GetList(Y key)
        {
            if (connectionMultiplexer.IsConnected)
            {
                var result = await connectionMultiplexer.GetDatabase().StringGetAsync(GetKey(key));
                if (result.IsNull)
                {
                    return default;
                }
                else
                {
                    return JsonConvert.DeserializeObject<List<T>>(result);
                }
            }
            else
            {
                return default;
            }
        }

        public async Task Set(Y key, T item, TimeSpan? expiration = null)
        {
            if (connectionMultiplexer.IsConnected)
            {
                var json = JsonConvert.SerializeObject(item);
                await connectionMultiplexer.GetDatabase().StringSetAsync(GetKey(key), json, expiration ?? defaultExpiration);
            }
        }

        public async Task SetList(Y key, List<T> items, TimeSpan? expiration = null)
        {
            if (connectionMultiplexer.IsConnected)
            {
                var json = JsonConvert.SerializeObject(items);
                await connectionMultiplexer.GetDatabase().StringSetAsync(GetKey(key), json, expiration ?? defaultExpiration);
            }
        }
    }
}
