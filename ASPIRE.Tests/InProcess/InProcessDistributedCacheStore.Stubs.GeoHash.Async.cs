namespace ASPIRE.Tests.InProcess;

public partial class InProcessDistributedCacheStore
{
    // Geo Async
    public Task<bool> GeoAddAsync(RedisKey key, double longitude, double latitude, RedisValue member,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<bool> GeoAddAsync(RedisKey key, GeoEntry value, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> GeoAddAsync(RedisKey key, GeoEntry[] values, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<double?> GeoDistanceAsync(RedisKey key, RedisValue member1, RedisValue member2,
        GeoUnit unit = GeoUnit.Meters, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<string?[]> GeoHashAsync(RedisKey key, RedisValue[] members, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<string?> GeoHashAsync(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<GeoPosition?[]> GeoPositionAsync(RedisKey key, RedisValue[] members,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<GeoPosition?> GeoPositionAsync(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<GeoRadiusResult[]> GeoRadiusAsync(RedisKey key, RedisValue member, double radius,
        GeoUnit unit = GeoUnit.Meters, int count = -1, Order? order = null,
        GeoRadiusOptions options = GeoRadiusOptions.None, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<GeoRadiusResult[]> GeoRadiusAsync(RedisKey key, double longitude, double latitude, double radius,
        GeoUnit unit = GeoUnit.Meters, int count = -1, Order? order = null,
        GeoRadiusOptions options = GeoRadiusOptions.None, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<bool> GeoRemoveAsync(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> GeoSearchAndStoreAsync(RedisKey destination, RedisKey key, RedisValue member,
        GeoSearchShape shape, int count = -1, bool demand = false, Order? order = null, bool storeDist = false,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> GeoSearchAndStoreAsync(RedisKey destination, RedisKey key, double longitude, double latitude,
        GeoSearchShape shape, int count = -1, bool demand = false, Order? order = null, bool storeDist = false,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<GeoRadiusResult[]> GeoSearchAsync(RedisKey key, RedisValue member, GeoSearchShape shape, int count = -1,
        bool demand = false, Order? order = null, GeoRadiusOptions options = GeoRadiusOptions.None,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<GeoRadiusResult[]> GeoSearchAsync(RedisKey key, double longitude, double latitude, GeoSearchShape shape,
        int count = -1, bool demand = false, Order? order = null, GeoRadiusOptions options = GeoRadiusOptions.None,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    // Hash Async
    public Task<long> HashDecrementAsync(RedisKey key, RedisValue hashField, long value = 1,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<double> HashDecrementAsync(RedisKey key, RedisValue hashField, double value,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<bool> HashDeleteAsync(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> HashDeleteAsync(RedisKey key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<bool> HashExistsAsync(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    // Correcting parameter names (TimeSpan expiry, DateTime expiry)
    public Task<ExpireResult[]> HashFieldExpireAsync(RedisKey key, RedisValue[] hashFields, TimeSpan expiry,
        ExpireWhen when = ExpireWhen.Always, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<ExpireResult[]> HashFieldExpireAsync(RedisKey key, RedisValue[] hashFields, DateTime expiry,
        ExpireWhen when = ExpireWhen.Always, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue> HashFieldGetAndDeleteAsync(RedisKey key, RedisValue hashField,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue[]> HashFieldGetAndDeleteAsync(RedisKey key, RedisValue[] hashFields,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue> HashFieldGetAndSetExpiryAsync(RedisKey key, RedisValue hashField, TimeSpan? expiry,
        bool keepTtl = false, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue> HashFieldGetAndSetExpiryAsync(RedisKey key, RedisValue hashField, DateTime expiry,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue[]> HashFieldGetAndSetExpiryAsync(RedisKey key, RedisValue[] hashFields, TimeSpan? expiry,
        bool keepTtl = false, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue[]> HashFieldGetAndSetExpiryAsync(RedisKey key, RedisValue[] hashFields, DateTime expiry,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long[]> HashFieldGetExpireDateTimeAsync(RedisKey key, RedisValue[] hashFields,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<Lease<byte>?> HashFieldGetLeaseAndDeleteAsync(RedisKey key, RedisValue hashField,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<Lease<byte>?> HashFieldGetLeaseAndSetExpiryAsync(RedisKey key, RedisValue hashField, TimeSpan? expiry,
        bool keepTtl = false, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<Lease<byte>?> HashFieldGetLeaseAndSetExpiryAsync(RedisKey key, RedisValue hashField, DateTime expiry,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long[]> HashFieldGetTimeToLiveAsync(RedisKey key, RedisValue[] hashFields,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<PersistResult[]> HashFieldPersistAsync(RedisKey key, RedisValue[] hashFields,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue> HashFieldSetAndSetExpiryAsync(RedisKey key, RedisValue hashField, RedisValue value,
        TimeSpan? expiry, bool keepTtl = false, When when = When.Always, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue> HashFieldSetAndSetExpiryAsync(RedisKey key, RedisValue hashField, RedisValue value,
        DateTime expiry, When when = When.Always, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue> HashFieldSetAndSetExpiryAsync(RedisKey key, HashEntry[] hashFields, TimeSpan? expiry,
        bool keepTtl = false, When when = When.Always, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue> HashFieldSetAndSetExpiryAsync(RedisKey key, HashEntry[] hashFields, DateTime expiry,
        When when = When.Always, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    // Skipped HashGetAllAsync (Implemented)
    public Task<RedisValue[]> HashGetAsync(RedisKey key, RedisValue[] hashFields,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }
    // Skipped HashGetAsync singular (Implemented)

    public Task<Lease<byte>?> HashGetLeaseAsync(RedisKey key, RedisValue hashField,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> HashIncrementAsync(RedisKey key, RedisValue hashField, long value = 1,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<double> HashIncrementAsync(RedisKey key, RedisValue hashField, double value,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue[]> HashKeysAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> HashLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue> HashRandomFieldAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue[]> HashRandomFieldsAsync(RedisKey key, long count, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<HashEntry[]> HashRandomFieldsWithValuesAsync(RedisKey key, long count,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    // Skipped HashScanAsync (Implemented)
    public IAsyncEnumerable<RedisValue> HashScanNoValuesAsync(RedisKey key, RedisValue pattern, int pageSize,
        long cursor, int pageOffset, CommandFlags flags)
    {
        throw new NotImplementedException();
    }

    public Task<bool> HashSetAsync(RedisKey key, RedisValue hashField, RedisValue value, When when = When.Always,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }
    // Skipped HashSetAsync array (Implemented)

    public Task<long> HashStringLengthAsync(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue[]> HashValuesAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    // HyperLogLog Async
    public Task<bool> HyperLogLogAddAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<bool> HyperLogLogAddAsync(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> HyperLogLogLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> HyperLogLogLengthAsync(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task HyperLogLogMergeAsync(RedisKey destination, RedisKey source1, RedisKey source2,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task HyperLogLogMergeAsync(RedisKey destination, RedisKey[] sourceKeys,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }
}