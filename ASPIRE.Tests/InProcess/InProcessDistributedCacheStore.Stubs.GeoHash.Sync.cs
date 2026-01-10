namespace ASPIRE.Tests.InProcess;

public partial class InProcessDistributedCacheStore
{
    public int Database => throw new NotImplementedException();

    public IBatch CreateBatch(object? asyncState = null)
    {
        throw new NotImplementedException();
    }

    public ITransaction CreateTransaction(object? asyncState = null)
    {
        throw new NotImplementedException();
    }

    public RedisValue DebugObject(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisResult Execute(string command, params object[] args)
    {
        throw new NotImplementedException();
    }

    public RedisResult Execute(string command, ICollection<object> args, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    // Geo
    public bool GeoAdd(RedisKey key, double longitude, double latitude, RedisValue member,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public bool GeoAdd(RedisKey key, GeoEntry value, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long GeoAdd(RedisKey key, GeoEntry[] values, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public double? GeoDistance(RedisKey key, RedisValue member1, RedisValue member2, GeoUnit unit = GeoUnit.Meters,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public string?[] GeoHash(RedisKey key, RedisValue[] members, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public string? GeoHash(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public GeoPosition?[] GeoPosition(RedisKey key, RedisValue[] members, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public GeoPosition? GeoPosition(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public GeoRadiusResult[] GeoRadius(RedisKey key, RedisValue member, double radius, GeoUnit unit = GeoUnit.Meters,
        int count = -1, Order? order = null, GeoRadiusOptions options = GeoRadiusOptions.None,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public GeoRadiusResult[] GeoRadius(RedisKey key, double longitude, double latitude, double radius,
        GeoUnit unit = GeoUnit.Meters, int count = -1, Order? order = null,
        GeoRadiusOptions options = GeoRadiusOptions.None, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public bool GeoRemove(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public GeoRadiusResult[] GeoSearch(RedisKey key, RedisValue member, GeoSearchShape shape, int count = -1,
        bool demand = false, Order? order = null, GeoRadiusOptions options = GeoRadiusOptions.None,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public GeoRadiusResult[] GeoSearch(RedisKey key, double longitude, double latitude, GeoSearchShape shape,
        int count = -1, bool demand = false, Order? order = null, GeoRadiusOptions options = GeoRadiusOptions.None,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long GeoSearchAndStore(RedisKey destination, RedisKey key, RedisValue member, GeoSearchShape shape,
        int count = -1, bool demand = false, Order? order = null, bool storeDist = false,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long GeoSearchAndStore(RedisKey destination, RedisKey key, double longitude, double latitude,
        GeoSearchShape shape, int count = -1, bool demand = false, Order? order = null, bool storeDist = false,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    // Hash
    public long HashDecrement(RedisKey key, RedisValue hashField, long value = 1,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public double HashDecrement(RedisKey key, RedisValue hashField, double value,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public bool HashDelete(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long HashDelete(RedisKey key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public bool HashExists(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public ExpireResult[] HashFieldExpire(RedisKey key, RedisValue[] hashFields, TimeSpan expiry,
        ExpireWhen when = ExpireWhen.Always, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public ExpireResult[] HashFieldExpire(RedisKey key, RedisValue[] hashFields, DateTime expiry,
        ExpireWhen when = ExpireWhen.Always, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue HashFieldGetAndDelete(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue[] HashFieldGetAndDelete(RedisKey key, RedisValue[] hashFields,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue HashFieldGetAndSetExpiry(RedisKey key, RedisValue hashField, TimeSpan? expiry,
        bool keepTtl = false, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue HashFieldGetAndSetExpiry(RedisKey key, RedisValue hashField, DateTime expiry,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue[] HashFieldGetAndSetExpiry(RedisKey key, RedisValue[] hashFields, TimeSpan? expiry,
        bool keepTtl = false, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue[] HashFieldGetAndSetExpiry(RedisKey key, RedisValue[] hashFields, DateTime expiry,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long[] HashFieldGetExpireDateTime(RedisKey key, RedisValue[] hashFields,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Lease<byte>? HashFieldGetLeaseAndDelete(RedisKey key, RedisValue hashField,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Lease<byte>? HashFieldGetLeaseAndSetExpiry(RedisKey key, RedisValue hashField, TimeSpan? expiry,
        bool keepTtl = false, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Lease<byte>? HashFieldGetLeaseAndSetExpiry(RedisKey key, RedisValue hashField, DateTime expiry,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long[] HashFieldGetTimeToLive(RedisKey key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public PersistResult[] HashFieldPersist(RedisKey key, RedisValue[] hashFields,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue HashFieldSetAndSetExpiry(RedisKey key, RedisValue hashField, RedisValue value, TimeSpan? expiry,
        bool keepTtl = false, When when = When.Always, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue HashFieldSetAndSetExpiry(RedisKey key, RedisValue hashField, RedisValue value, DateTime expiry,
        When when = When.Always, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue HashFieldSetAndSetExpiry(RedisKey key, HashEntry[] hashFields, TimeSpan? expiry,
        bool keepTtl = false, When when = When.Always, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue HashFieldSetAndSetExpiry(RedisKey key, HashEntry[] hashFields, DateTime expiry,
        When when = When.Always, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue HashGet(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue[] HashGet(RedisKey key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public HashEntry[] HashGetAll(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Lease<byte>? HashGetLease(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long HashIncrement(RedisKey key, RedisValue hashField, long value = 1,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public double HashIncrement(RedisKey key, RedisValue hashField, double value,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue[] HashKeys(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long HashLength(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue HashRandomField(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue[] HashRandomFields(RedisKey key, long count, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public HashEntry[] HashRandomFieldsWithValues(RedisKey key, long count, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<HashEntry> HashScan(RedisKey key, RedisValue pattern = default, int pageSize = 250,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<HashEntry> HashScan(RedisKey key, RedisValue pattern, int pageSize, long cursor, int pageOffset,
        CommandFlags flags)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<RedisValue> HashScanNoValues(RedisKey key, RedisValue pattern, int pageSize, long cursor,
        int pageOffset, CommandFlags flags)
    {
        throw new NotImplementedException();
    }

    public void HashSet(RedisKey key, HashEntry[] hashFields, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public bool HashSet(RedisKey key, RedisValue hashField, RedisValue value, When when = When.Always,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long HashStringLength(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue[] HashValues(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    // HyperLogLog
    public bool HyperLogLogAdd(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public bool HyperLogLogAdd(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long HyperLogLogLength(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long HyperLogLogLength(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public void HyperLogLogMerge(RedisKey destination, RedisKey source1, RedisKey source2,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public void HyperLogLogMerge(RedisKey destination, RedisKey[] sourceKeys, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public ExpireResult[] HashFieldExpire(RedisKey key, RedisValue[] hashFields, TimeSpan? expiry,
        ExpireWhen when = ExpireWhen.Always, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public ExpireResult[] HashFieldExpire(RedisKey key, RedisValue[] hashFields, DateTime? expiry,
        ExpireWhen when = ExpireWhen.Always, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }
}