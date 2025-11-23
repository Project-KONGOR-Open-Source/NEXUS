namespace ASPIRE.Tests.InProcess;

/// <summary>
///     In-memory test double for Redis IDatabase using ConcurrentDictionary for simple storage.
/// </summary>
/// <remarks>
///     This implementation provides basic Redis-like operations for unit testing purposes.
///     It does not replicate full Redis semantics (e.g. expiration is ignored, atomic operations are simplified).
///     Hash operations use composite keys in the format "hashKey:field" for storage.
/// </remarks>
[AutoImplementMissingMembers]
public partial class InProcessDistributedCacheStore : IDatabase
{
    private ConcurrentDictionary<string, string> StoreItems { get; set; } = new ();

    // String Operations

    public Task<bool> StringSetAsync(RedisKey key, RedisValue value, TimeSpan? expiry = null, When when = When.Always, CommandFlags flags = CommandFlags.None)
    {
        StoreItems[key.ToString()] = value.ToString();

        return Task.FromResult(true);
    }

    public Task<bool> StringSetAsync(RedisKey key, RedisValue value, TimeSpan? expiry, bool keepTtl, When when = When.Always, CommandFlags flags = CommandFlags.None)
    {
        StoreItems[key.ToString()] = value.ToString();

        return Task.FromResult(true);
    }

    public Task<RedisValue> StringGetAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        bool found = StoreItems.TryGetValue(key.ToString(), out string? value);

        return Task.FromResult(found ? (RedisValue) value : RedisValue.Null);
    }

    // Key Operations

    public Task<bool> KeyDeleteAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        string keyString = key.ToString();
        string hashPrefix = keyString + ":";

        bool keyDeleted = StoreItems.TryRemove(keyString, out _);

        List<string> keysToDelete = [.. StoreItems.Keys.Where(storageKey => storageKey.StartsWith(hashPrefix))];

        int hashFieldsDeleted = 0;

        foreach (string storageKey in keysToDelete)
        {
            if (StoreItems.TryRemove(storageKey, out _))
            {
                hashFieldsDeleted++;
            }
        }

        return Task.FromResult(keyDeleted || hashFieldsDeleted > 0);
    }

    // Hash Operations

    public Task HashSetAsync(RedisKey key, HashEntry[] hashFields, CommandFlags flags = CommandFlags.None)
    {
        string keyString = key.ToString();

        foreach (HashEntry entry in hashFields)
        {
            string compositeKey = $"{keyString}:{entry.Name}";

            StoreItems[compositeKey] = entry.Value.ToString();
        }

        return Task.CompletedTask;
    }

    public Task<RedisValue> HashGetAsync(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
    {
        string compositeKey = $"{key}:{hashField}";

        bool found = StoreItems.TryGetValue(compositeKey, out string? value);

        return Task.FromResult(found ? (RedisValue) value : RedisValue.Null);
    }

    public Task<HashEntry[]> HashGetAllAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        string keyString = key.ToString();
        string hashPrefix = keyString + ":";

        List<HashEntry> entries = [];

        foreach (KeyValuePair<string, string> kvp in StoreItems)
        {
            if (kvp.Key.StartsWith(hashPrefix))
            {
                string fieldName = kvp.Key[hashPrefix.Length ..];

                entries.Add(new HashEntry(fieldName, kvp.Value));
            }
        }

        return Task.FromResult(entries.ToArray());
    }

    public IAsyncEnumerable<HashEntry> HashScanAsync(RedisKey key, RedisValue pattern = default, int pageSize = 250, long cursor = 0, int pageOffset = 0, CommandFlags flags = CommandFlags.None)
    {
        string keyString = key.ToString();
        string hashPrefix = keyString + ":";
        string patternString = pattern.ToString();

        string regexPattern = "^" + Regex.Escape(patternString).Replace(@"\*", ".*") + "$";

        Regex regex = new (regexPattern);

        List<HashEntry> matchingEntries = [];

        foreach (KeyValuePair<string, string> kvp in StoreItems)
        {
            if (kvp.Key.StartsWith(hashPrefix))
            {
                string fullField = kvp.Key[hashPrefix.Length ..];

                if (string.IsNullOrEmpty(patternString) || regex.IsMatch(fullField))
                {
                    matchingEntries.Add(new HashEntry(fullField, kvp.Value));
                }
            }
        }

        return matchingEntries.ToAsyncEnumerable();
    }
}
