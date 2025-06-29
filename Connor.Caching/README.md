
# Connor.Caching

This is a project that allows for a quick Redis Cache setup for a given type.




## Create Your Cache(s)

```
// This is what you want to cache and retreive
public class ObjectToCache 
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

// CacheBase<T, Y>
// T = the type of the value you want to cache
// Y = the type of the key you want to use to cache T
public class SomethingCache(IConnectionMultiplexer c) : CacheBase<ObjectToCache, int>(c)
{
    public override string GetKey(int key)
    {
        return $"SOME_PREFIX_{key}";
    }

    public override int GetValueFromKey(string key)
    {
        return int.Parse(key.Replace("SOME_PREFIX_", string.Empty));
    }
}
```

## Reference Your Cache

```
public class LogicClass(SomethingCache cache, DbContext db)
{
    public async Task<string> GetNameFromIdAsync(int id)
    {
        var fullObj = cache.GetFirstOrDefaultAsync(id);
        if (fullObj == null)
        {
            fullObj = db.Somethings.First(x => x.Id == id);
            cache.SetAsync(fullObj)
        }
        return fullObj.Name;
    }
}
```