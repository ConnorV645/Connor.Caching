using Newtonsoft.Json;
using StackExchange.Redis;

namespace Connor.Caching
{
    public abstract class CacheBase<T, Y>(IConnectionMultiplexer connectionMultiplexer, TimeSpan? defaultExpiration = null) : ICache<T, Y> where Y : notnull
    {
        protected readonly TimeSpan defaultExpiration = defaultExpiration ?? TimeSpan.FromHours(1);
        protected readonly IConnectionMultiplexer connectionMultiplexer = connectionMultiplexer;

        public abstract string GetKey(Y key);
        public abstract Y GetValueFromKey(string fullKey);

        public async Task<T?> GetFirstOrDefaultAsync(Y key)
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
                    return JsonConvert.DeserializeObject<T>(result!);
                }
            }
            else
            {
                return default;
            }
        }

        public async Task<(T? item, TimeSpan? expiration)> GetFirstOrDefaultWithExpiryAsync(Y key)
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
                    return (JsonConvert.DeserializeObject<T>(result.Value!), result.Expiry);
                }
            }
            else
            {
                return default;
            }
        }

        public async Task<List<T>?> GetListAsync(Y key)
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
                    return JsonConvert.DeserializeObject<List<T>>(result!);
                }
            }
            else
            {
                return default;
            }
        }

        public async Task<Dictionary<Y, T>?> GetMultipleFromPatternWithValuesAsync(string fullKeyPattern)
        {
            if (connectionMultiplexer.IsConnected)
            {
                var finalResult = new Dictionary<Y, T>();
                foreach (var key in connectionMultiplexer.GetServers().First().Keys(pattern: fullKeyPattern))
                {
                    var result = await connectionMultiplexer.GetDatabase().StringGetAsync(key);
                    if (!result.IsNull)
                    {
                        finalResult.TryAdd(GetValueFromKey(key.ToString()), JsonConvert.DeserializeObject<T>(result!)!);
                    }
                }
                return finalResult;
            }
            return default;
        }

        public List<Y>? GetKeysThatHaveValuesFromPattern(string redisPattern)
        {
            if (connectionMultiplexer.IsConnected)
            {
                var rawKeys = connectionMultiplexer.GetServers().First().Keys(pattern: redisPattern);
                return [.. rawKeys.Select(x => GetValueFromKey(x.ToString()))];
            }
            return default;
        }

        public async Task SetAsync(Y key, T item, TimeSpan? expiration = null)
        {
            if (connectionMultiplexer.IsConnected)
            {
                var json = JsonConvert.SerializeObject(item);
                await connectionMultiplexer.GetDatabase().StringSetAsync(GetKey(key), json, expiration ?? defaultExpiration);
            }
        }

        public async Task SetListAsync(Y key, List<T> items, TimeSpan? expiration = null)
        {
            if (connectionMultiplexer.IsConnected)
            {
                var json = JsonConvert.SerializeObject(items);
                await connectionMultiplexer.GetDatabase().StringSetAsync(GetKey(key), json, expiration ?? defaultExpiration);
            }
        }
    }
}
