namespace ASPIRE.Tests.InProcess;

public partial class InProcessDistributedCacheStore
{
    // SortedSet
    public bool SortedSetAdd(RedisKey key, RedisValue member, double score, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public bool SortedSetAdd(RedisKey key, RedisValue member, double score, When when,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public bool SortedSetAdd(RedisKey key, RedisValue member, double score, SortedSetWhen when,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long SortedSetAdd(RedisKey key, SortedSetEntry[] values, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long SortedSetAdd(RedisKey key, SortedSetEntry[] values, When when, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long SortedSetAdd(RedisKey key, SortedSetEntry[] values, SortedSetWhen when,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue[] SortedSetCombine(SetOperation operation, RedisKey[] keys, double[]? weights = null,
        Aggregate aggregate = Aggregate.Sum, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public SortedSetEntry[] SortedSetCombineWithScores(SetOperation operation, RedisKey[] keys,
        double[]? weights = null, Aggregate aggregate = Aggregate.Sum, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long SortedSetCombineAndStore(SetOperation operation, RedisKey destination, RedisKey first, RedisKey second,
        Aggregate aggregate = Aggregate.Sum, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long SortedSetCombineAndStore(SetOperation operation, RedisKey destination, RedisKey[] keys,
        double[]? weights = null, Aggregate aggregate = Aggregate.Sum, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public double SortedSetDecrement(RedisKey key, RedisValue member, double value,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public double SortedSetIncrement(RedisKey key, RedisValue member, double value,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long SortedSetIntersectionLength(RedisKey[] keys, long limit = 0, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long SortedSetLength(RedisKey key, double min = double.NegativeInfinity,
        double max = double.PositiveInfinity, Exclude exclude = Exclude.None, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long SortedSetLengthByValue(RedisKey key, RedisValue min, RedisValue max, Exclude exclude = Exclude.None,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public SortedSetEntry? SortedSetPop(RedisKey key, Order order = Order.Ascending,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public SortedSetEntry[] SortedSetPop(RedisKey key, long count, Order order = Order.Ascending,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public SortedSetPopResult SortedSetPop(RedisKey[] keys, long count, Order order = Order.Ascending,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue SortedSetRandomMember(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue[] SortedSetRandomMembers(RedisKey key, long count, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public SortedSetEntry[] SortedSetRandomMembersWithScores(RedisKey key, long count,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long SortedSetRangeAndStore(RedisKey destination, RedisKey source, RedisValue min, RedisValue max,
        SortedSetOrder order = SortedSetOrder.ByScore, Exclude exclude = Exclude.None,
        Order direction = Order.Ascending, long skip = 0, long? take = null, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue[] SortedSetRangeByRank(RedisKey key, long start = 0, long stop = -1,
        Order order = Order.Ascending, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public SortedSetEntry[] SortedSetRangeByRankWithScores(RedisKey key, long start = 0, long stop = -1,
        Order order = Order.Ascending, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue[] SortedSetRangeByScore(RedisKey key, double start = double.NegativeInfinity,
        double stop = double.PositiveInfinity, Exclude exclude = Exclude.None, Order order = Order.Ascending,
        long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public SortedSetEntry[] SortedSetRangeByScoreWithScores(RedisKey key, double start = double.NegativeInfinity,
        double stop = double.PositiveInfinity, Exclude exclude = Exclude.None, Order order = Order.Ascending,
        long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue[] SortedSetRangeByValue(RedisKey key, RedisValue min, RedisValue max,
        Exclude exclude = Exclude.None, long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue[] SortedSetRangeByValue(RedisKey key, RedisValue min, RedisValue max, Exclude exclude,
        Order order, long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long? SortedSetRank(RedisKey key, RedisValue member, Order order = Order.Ascending,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public bool SortedSetRemove(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long SortedSetRemove(RedisKey key, RedisValue[] members, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long SortedSetRemoveRangeByRank(RedisKey key, long start, long stop, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long SortedSetRemoveRangeByScore(RedisKey key, double start, double stop, Exclude exclude = Exclude.None,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long SortedSetRemoveRangeByValue(RedisKey key, RedisValue min, RedisValue max,
        Exclude exclude = Exclude.None, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<SortedSetEntry> SortedSetScan(RedisKey key, RedisValue pattern = default, int pageSize = 250,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<SortedSetEntry> SortedSetScan(RedisKey key, RedisValue pattern, int pageSize, long cursor,
        int pageOffset, CommandFlags flags)
    {
        throw new NotImplementedException();
    }

    public double? SortedSetScore(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public double?[] SortedSetScores(RedisKey key, RedisValue[] members, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public bool SortedSetUpdate(RedisKey key, RedisValue member, double score, SortedSetWhen when,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long SortedSetUpdate(RedisKey key, SortedSetEntry[] values, SortedSetWhen when,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    // Stream
    public long StreamAcknowledge(RedisKey key, RedisValue groupName, RedisValue messageId,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long StreamAcknowledge(RedisKey key, RedisValue groupName, RedisValue[] messageIds,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public StreamTrimResult StreamAcknowledgeAndDelete(RedisKey key, RedisValue groupName, StreamTrimMode mode,
        RedisValue messageId, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public StreamTrimResult[] StreamAcknowledgeAndDelete(RedisKey key, RedisValue groupName, StreamTrimMode mode,
        RedisValue[] messageIds, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue StreamAdd(RedisKey key, RedisValue field, RedisValue value, RedisValue? messageId = null,
        int? maxLength = null, bool useApproximateMaxLength = false, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue StreamAdd(RedisKey key, NameValueEntry[] entries, RedisValue? messageId = null,
        int? maxLength = null, bool useApproximateMaxLength = false, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue StreamAdd(RedisKey key, RedisValue field, RedisValue value, RedisValue? messageId,
        long? maxLength, bool useApproximateMaxLength, long? minId, StreamTrimMode trimMode,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue StreamAdd(RedisKey key, NameValueEntry[] entries, RedisValue? messageId, long? maxLength,
        bool useApproximateMaxLength, long? minId, StreamTrimMode trimMode, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public StreamAutoClaimResult StreamAutoClaim(RedisKey key, RedisValue groupName, RedisValue consumerName,
        long minIdleTime, RedisValue startId, int? count = null, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public StreamAutoClaimIdsOnlyResult StreamAutoClaimIdsOnly(RedisKey key, RedisValue groupName,
        RedisValue consumerName, long minIdleTime, RedisValue startId, int? count = null,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public StreamEntry[] StreamClaim(RedisKey key, RedisValue groupName, RedisValue consumerName, long minIdleTime,
        RedisValue[] messageIds, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue[] StreamClaimIdsOnly(RedisKey key, RedisValue groupName, RedisValue consumerName,
        long minIdleTime, RedisValue[] messageIds, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public bool StreamConsumerGroupSetPosition(RedisKey key, RedisValue groupName, RedisValue messageId,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public StreamConsumerInfo[] StreamConsumerInfo(RedisKey key, RedisValue groupName,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public bool StreamCreateConsumerGroup(RedisKey key, RedisValue groupName, RedisValue? messageId = null,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public bool StreamCreateConsumerGroup(RedisKey key, RedisValue groupName, RedisValue? messageId, bool createStream,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long StreamDelete(RedisKey key, RedisValue[] messageIds, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public StreamTrimResult[] StreamDelete(RedisKey key, RedisValue[] messageIds, StreamTrimMode mode,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long StreamDeleteConsumer(RedisKey key, RedisValue groupName, RedisValue consumerName,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public bool StreamDeleteConsumerGroup(RedisKey key, RedisValue groupName, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public StreamGroupInfo[] StreamGroupInfo(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public StreamInfo StreamInfo(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long StreamLength(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public StreamPendingInfo StreamPending(RedisKey key, RedisValue groupName, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public StreamPendingMessageInfo[] StreamPendingMessages(RedisKey key, RedisValue groupName, int count,
        RedisValue consumerName, RedisValue? minId = null, RedisValue? maxId = null,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public StreamPendingMessageInfo[] StreamPendingMessages(RedisKey key, RedisValue groupName, int count,
        RedisValue consumerName, RedisValue? minId, RedisValue? maxId, long? minIdleTime,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public StreamEntry[] StreamRange(RedisKey key, RedisValue? minId = null, RedisValue? maxId = null,
        int? count = null, Order order = Order.Ascending, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public StreamEntry[] StreamRead(RedisKey key, RedisValue position, int? count = null,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisStream[] StreamRead(StreamPosition[] streams, int? count = null, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public StreamEntry[] StreamReadGroup(RedisKey key, RedisValue groupName, RedisValue consumerName,
        RedisValue? position = null, int? count = null, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public StreamEntry[] StreamReadGroup(RedisKey key, RedisValue groupName, RedisValue consumerName,
        RedisValue? position, int? count, bool noAck, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisStream[] StreamReadGroup(StreamPosition[] streams, RedisValue groupName, RedisValue consumerName,
        int? count = null, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisStream[] StreamReadGroup(StreamPosition[] streams, RedisValue groupName, RedisValue consumerName,
        int? count, bool noAck, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long StreamTrim(RedisKey key, int maxLength, bool useApproximateMaxLength = false,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long StreamTrim(RedisKey key, long maxLength, bool useApproximateMaxLength, long? minId,
        StreamTrimMode trimMode, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long StreamTrimByMinId(RedisKey key, RedisValue minId, bool useApproximateMaxLength, long? limit,
        StreamTrimMode trimMode, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    // String
    public long StringAppend(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long StringBitCount(RedisKey key, long start = 0, long end = -1, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long StringBitCount(RedisKey key, long start, long end, StringIndexType indexType,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long StringBitOperation(Bitwise operation, RedisKey destination, RedisKey first, RedisKey second,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long StringBitOperation(Bitwise operation, RedisKey destination, RedisKey[] keys,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long StringBitPosition(RedisKey key, bool bit, long start = 0, long end = -1,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long StringBitPosition(RedisKey key, bool bit, long start, long end, StringIndexType indexType,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long StringDecrement(RedisKey key, long value = 1, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public double StringDecrement(RedisKey key, double value, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue StringGet(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue[] StringGet(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public bool StringGetBit(RedisKey key, long offset, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue StringGetDelete(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Lease<byte>? StringGetLease(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue StringGetRange(RedisKey key, long start, long end, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue StringGetSet(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue StringGetSetExpiry(RedisKey key, TimeSpan? expiry, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue StringGetSetExpiry(RedisKey key, DateTime expiry, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValueWithExpiry StringGetWithExpiry(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long StringIncrement(RedisKey key, long value = 1, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public double StringIncrement(RedisKey key, double value, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long StringLength(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public string? StringLongestCommonSubsequence(RedisKey key1, RedisKey key2, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long StringLongestCommonSubsequenceLength(RedisKey key1, RedisKey key2,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public LCSMatchResult StringLongestCommonSubsequenceWithMatches(RedisKey key1, RedisKey key2,
        long minResultLength = 0, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public bool StringSet(RedisKey key, RedisValue value, TimeSpan? expiry = null, When when = When.Always,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public bool StringSet(KeyValuePair<RedisKey, RedisValue>[] values, When when = When.Always,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public bool StringSet(RedisKey key, RedisValue value, TimeSpan? expiry, bool keepTtl, When when = When.Always,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue StringSetAndGet(RedisKey key, RedisValue value, TimeSpan? expiry = null, When when = When.Always,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue StringSetAndGet(RedisKey key, RedisValue value, TimeSpan? expiry, bool keepTtl,
        When when = When.Always, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public bool StringSetBit(RedisKey key, long offset, bool bit, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue StringSetRange(RedisKey key, long offset, RedisValue value,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public TimeSpan Ping(CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public bool StringSet(RedisKey key, RedisValue value, TimeSpan? expiry, When when)
    {
        throw new NotImplementedException();
    }

    // Sort Sync (Missing members)
    public RedisValue[] Sort(RedisKey key, long skip = 0, long take = -1, Order order = Order.Ascending,
        SortType sortType = SortType.Numeric, RedisValue by = default, RedisValue[]? get = null,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long SortAndStore(RedisKey destination, RedisKey key, long skip = 0, long take = -1,
        Order order = Order.Ascending, SortType sortType = SortType.Numeric, RedisValue by = default,
        RedisValue[]? get = null, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    // Vector
#pragma warning disable SER001
    public bool VectorSetAdd(RedisKey key, VectorSetAddRequest request, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public bool VectorSetContains(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public int VectorSetDimension(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Lease<float>? VectorSetGetApproximateVector(RedisKey key, RedisValue member,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public string? VectorSetGetAttributesJson(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Lease<RedisValue>? VectorSetGetLinks(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Lease<VectorSetLink>? VectorSetGetLinksWithScores(RedisKey key, RedisValue member,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public VectorSetInfo? VectorSetInfo(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public long VectorSetLength(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue VectorSetRandomMember(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public RedisValue[] VectorSetRandomMembers(RedisKey key, long count, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public bool VectorSetRemove(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public bool VectorSetSetAttributesJson(RedisKey key, RedisValue member, string json,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Lease<VectorSetSimilaritySearchResult>? VectorSetSimilaritySearch(RedisKey key,
        VectorSetSimilaritySearchRequest request, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }
#pragma warning restore SER001
}