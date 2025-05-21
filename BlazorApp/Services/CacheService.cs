public class CacheService
{
    private static readonly Dictionary<string, (object Data, DateTime ExpiryTime)> _cache = new();
    private static readonly Dictionary<string, DateTime> _lastModifiedTimestamps = new();
    private static readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(5);

    public void Set<T>(string key, T data, TimeSpan? expiration = null)
    {
        var expiryTime = DateTime.UtcNow.Add(expiration ?? _defaultExpiration);
        _cache[key] = (data, expiryTime);
    }

    public bool TryGetValue<T>(string key, out T value)
    {
        value = default;
        if (_cache.TryGetValue(key, out var cacheEntry))
        {
            if (DateTime.UtcNow < cacheEntry.ExpiryTime)
            {
                value = (T)cacheEntry.Data;
                return true;
            }
            _cache.Remove(key);
        }
        return false;
    }

    public void SetLastModified(string entityType, DateTime timestamp)
    {
        _lastModifiedTimestamps[entityType] = timestamp;
    }

    public bool HasNewerData(string entityType, DateTime serverTimestamp)
    {
        if (!_lastModifiedTimestamps.TryGetValue(entityType, out var cachedTimestamp))
            return true; // No cached timestamp means we should fetch
            
        return serverTimestamp > cachedTimestamp;
    }

    public void Clear(string key) => _cache.Remove(key);
    public void ClearAll() 
    {
        _cache.Clear();
        _lastModifiedTimestamps.Clear();
    }
}