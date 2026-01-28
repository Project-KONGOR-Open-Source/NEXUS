namespace ASPIRE.Tests.InProcess;

public partial class InProcessDistributedCacheStore
{
    // SortedSet Async
    public Task<bool> SortedSetAddAsync(RedisKey key, RedisValue member, double score,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SortedSetAddAsync(RedisKey key, RedisValue member, double score, When when,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SortedSetAddAsync(RedisKey key, RedisValue member, double score, SortedSetWhen when,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> SortedSetAddAsync(RedisKey key, SortedSetEntry[] values, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> SortedSetAddAsync(RedisKey key, SortedSetEntry[] values, When when,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> SortedSetAddAsync(RedisKey key, SortedSetEntry[] values, SortedSetWhen when,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> SortedSetCombineAndStoreAsync(SetOperation operation, RedisKey destination, RedisKey first,
        RedisKey second, Aggregate aggregate = Aggregate.Sum, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> SortedSetCombineAndStoreAsync(SetOperation operation, RedisKey destination, RedisKey[] keys,
        double[]? weights = null, Aggregate aggregate = Aggregate.Sum, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue[]> SortedSetCombineAsync(SetOperation operation, RedisKey[] keys, double[]? weights = null,
        Aggregate aggregate = Aggregate.Sum, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<SortedSetEntry[]> SortedSetCombineWithScoresAsync(SetOperation operation, RedisKey[] keys,
        double[]? weights = null, Aggregate aggregate = Aggregate.Sum, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<double> SortedSetDecrementAsync(RedisKey key, RedisValue member, double value,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<double> SortedSetIncrementAsync(RedisKey key, RedisValue member, double value,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> SortedSetIntersectionLengthAsync(RedisKey[] keys, long limit = 0,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> SortedSetLengthAsync(RedisKey key, double min = double.NegativeInfinity,
        double max = double.PositiveInfinity, Exclude exclude = Exclude.None, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> SortedSetLengthByValueAsync(RedisKey key, RedisValue min, RedisValue max,
        Exclude exclude = Exclude.None, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<SortedSetEntry?> SortedSetPopAsync(RedisKey key, Order order = Order.Ascending,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<SortedSetEntry[]> SortedSetPopAsync(RedisKey key, long count, Order order = Order.Ascending,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<SortedSetPopResult> SortedSetPopAsync(RedisKey[] keys, long count, Order order = Order.Ascending,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue> SortedSetRandomMemberAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue[]> SortedSetRandomMembersAsync(RedisKey key, long count,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<SortedSetEntry[]> SortedSetRandomMembersWithScoresAsync(RedisKey key, long count,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> SortedSetRangeAndStoreAsync(RedisKey destination, RedisKey source, RedisValue min, RedisValue max,
        SortedSetOrder order = SortedSetOrder.ByScore, Exclude exclude = Exclude.None,
        Order direction = Order.Ascending, long skip = 0, long? take = null, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue[]> SortedSetRangeByRankAsync(RedisKey key, long start = 0, long stop = -1,
        Order order = Order.Ascending, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<SortedSetEntry[]> SortedSetRangeByRankWithScoresAsync(RedisKey key, long start = 0, long stop = -1,
        Order order = Order.Ascending, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue[]> SortedSetRangeByScoreAsync(RedisKey key, double start = double.NegativeInfinity,
        double stop = double.PositiveInfinity, Exclude exclude = Exclude.None, Order order = Order.Ascending,
        long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<SortedSetEntry[]> SortedSetRangeByScoreWithScoresAsync(RedisKey key,
        double start = double.NegativeInfinity, double stop = double.PositiveInfinity, Exclude exclude = Exclude.None,
        Order order = Order.Ascending, long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue[]> SortedSetRangeByValueAsync(RedisKey key, RedisValue min, RedisValue max,
        Exclude exclude = Exclude.None, long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue[]> SortedSetRangeByValueAsync(RedisKey key, RedisValue min, RedisValue max, Exclude exclude,
        Order order, long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long?> SortedSetRankAsync(RedisKey key, RedisValue member, Order order = Order.Ascending,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SortedSetRemoveAsync(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> SortedSetRemoveAsync(RedisKey key, RedisValue[] members, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> SortedSetRemoveRangeByRankAsync(RedisKey key, long start, long stop,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> SortedSetRemoveRangeByScoreAsync(RedisKey key, double start, double stop,
        Exclude exclude = Exclude.None, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> SortedSetRemoveRangeByValueAsync(RedisKey key, RedisValue min, RedisValue max,
        Exclude exclude = Exclude.None, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<SortedSetEntry> SortedSetScanAsync(RedisKey key, RedisValue pattern = default,
        int pageSize = 250, long cursor = 0, int pageOffset = 0, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<double?> SortedSetScoreAsync(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<double?[]> SortedSetScoresAsync(RedisKey key, RedisValue[] members,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SortedSetUpdateAsync(RedisKey key, RedisValue member, double score, SortedSetWhen when,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> SortedSetUpdateAsync(RedisKey key, SortedSetEntry[] values, SortedSetWhen when,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    // Stream Async
    public Task<StreamTrimResult> StreamAcknowledgeAndDeleteAsync(RedisKey key, RedisValue groupName,
        StreamTrimMode mode, RedisValue messageId, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<StreamTrimResult[]> StreamAcknowledgeAndDeleteAsync(RedisKey key, RedisValue groupName,
        StreamTrimMode mode, RedisValue[] messageIds, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> StreamAcknowledgeAsync(RedisKey key, RedisValue groupName, RedisValue messageId,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> StreamAcknowledgeAsync(RedisKey key, RedisValue groupName, RedisValue[] messageIds,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue> StreamAddAsync(RedisKey key, RedisValue field, RedisValue value,
        RedisValue? messageId = null, int? maxLength = null, bool useApproximateMaxLength = false,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue> StreamAddAsync(RedisKey key, NameValueEntry[] entries, RedisValue? messageId = null,
        int? maxLength = null, bool useApproximateMaxLength = false, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue> StreamAddAsync(RedisKey key, RedisValue field, RedisValue value, RedisValue? messageId,
        long? maxLength, bool useApproximateMaxLength, long? minId, StreamTrimMode trimMode,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue> StreamAddAsync(RedisKey key, NameValueEntry[] entries, RedisValue? messageId,
        long? maxLength, bool useApproximateMaxLength, long? minId, StreamTrimMode trimMode,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<StreamAutoClaimResult> StreamAutoClaimAsync(RedisKey key, RedisValue groupName, RedisValue consumerName,
        long minIdleTime, RedisValue startId, int? count = null, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<StreamAutoClaimIdsOnlyResult> StreamAutoClaimIdsOnlyAsync(RedisKey key, RedisValue groupName,
        RedisValue consumerName, long minIdleTime, RedisValue startId, int? count = null,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<StreamEntry[]> StreamClaimAsync(RedisKey key, RedisValue groupName, RedisValue consumerName,
        long minIdleTime, RedisValue[] messageIds, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue[]> StreamClaimIdsOnlyAsync(RedisKey key, RedisValue groupName, RedisValue consumerName,
        long minIdleTime, RedisValue[] messageIds, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<bool> StreamConsumerGroupSetPositionAsync(RedisKey key, RedisValue groupName, RedisValue messageId,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<StreamConsumerInfo[]> StreamConsumerInfoAsync(RedisKey key, RedisValue groupName,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<bool> StreamCreateConsumerGroupAsync(RedisKey key, RedisValue groupName, RedisValue? messageId = null,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<bool> StreamCreateConsumerGroupAsync(RedisKey key, RedisValue groupName, RedisValue? messageId,
        bool createStream, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> StreamDeleteAsync(RedisKey key, RedisValue[] messageIds, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<StreamTrimResult[]> StreamDeleteAsync(RedisKey key, RedisValue[] messageIds, StreamTrimMode mode,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> StreamDeleteConsumerAsync(RedisKey key, RedisValue groupName, RedisValue consumerName,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<bool> StreamDeleteConsumerGroupAsync(RedisKey key, RedisValue groupName,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<StreamGroupInfo[]> StreamGroupInfoAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<StreamInfo> StreamInfoAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> StreamLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<StreamPendingInfo> StreamPendingAsync(RedisKey key, RedisValue groupName,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<StreamPendingMessageInfo[]> StreamPendingMessagesAsync(RedisKey key, RedisValue groupName, int count,
        RedisValue consumerName, RedisValue? minId = null, RedisValue? maxId = null,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<StreamPendingMessageInfo[]> StreamPendingMessagesAsync(RedisKey key, RedisValue groupName, int count,
        RedisValue consumerName, RedisValue? minId, RedisValue? maxId, long? minIdleTime,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<StreamEntry[]> StreamRangeAsync(RedisKey key, RedisValue? minId = null, RedisValue? maxId = null,
        int? count = null, Order order = Order.Ascending, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<StreamEntry[]> StreamReadAsync(RedisKey key, RedisValue position, int? count = null,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisStream[]> StreamReadAsync(StreamPosition[] streams, int? count = null,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<StreamEntry[]> StreamReadGroupAsync(RedisKey key, RedisValue groupName, RedisValue consumerName,
        RedisValue? position = null, int? count = null, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<StreamEntry[]> StreamReadGroupAsync(RedisKey key, RedisValue groupName, RedisValue consumerName,
        RedisValue? position, int? count, bool noAck, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisStream[]> StreamReadGroupAsync(StreamPosition[] streams, RedisValue groupName,
        RedisValue consumerName, int? count = null, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisStream[]> StreamReadGroupAsync(StreamPosition[] streams, RedisValue groupName,
        RedisValue consumerName, int? count, bool noAck, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> StreamTrimAsync(RedisKey key, int maxLength, bool useApproximateMaxLength = false,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> StreamTrimAsync(RedisKey key, long maxLength, bool useApproximateMaxLength, long? minId,
        StreamTrimMode trimMode, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> StreamTrimByMinIdAsync(RedisKey key, RedisValue minId, bool useApproximateMaxLength, long? limit,
        StreamTrimMode trimMode, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    // String Async
    public Task<long> StringAppendAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> StringBitCountAsync(RedisKey key, long start = 0, long end = -1,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> StringBitCountAsync(RedisKey key, long start, long end, StringIndexType indexType,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> StringBitOperationAsync(Bitwise operation, RedisKey destination, RedisKey first, RedisKey second,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> StringBitOperationAsync(Bitwise operation, RedisKey destination, RedisKey[] keys,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> StringBitPositionAsync(RedisKey key, bool bit, long start = 0, long end = -1,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> StringBitPositionAsync(RedisKey key, bool bit, long start, long end, StringIndexType indexType,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> StringDecrementAsync(RedisKey key, long value = 1, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<double> StringDecrementAsync(RedisKey key, double value, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue[]> StringGetAsync(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    // Skipped StringGetAsync singular (Implemented)
    public Task<bool> StringGetBitAsync(RedisKey key, long offset, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue> StringGetDeleteAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<Lease<byte>?> StringGetLeaseAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue> StringGetRangeAsync(RedisKey key, long start, long end,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue> StringGetSetAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue> StringGetSetExpiryAsync(RedisKey key, TimeSpan? expiry,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue> StringGetSetExpiryAsync(RedisKey key, DateTime expiry,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValueWithExpiry> StringGetWithExpiryAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> StringIncrementAsync(RedisKey key, long value = 1, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<double> StringIncrementAsync(RedisKey key, double value, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> StringLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<string?> StringLongestCommonSubsequenceAsync(RedisKey key1, RedisKey key2,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> StringLongestCommonSubsequenceLengthAsync(RedisKey key1, RedisKey key2,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<LCSMatchResult> StringLongestCommonSubsequenceWithMatchesAsync(RedisKey key1, RedisKey key2,
        long minResultLength = 0, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue> StringSetAndGetAsync(RedisKey key, RedisValue value, TimeSpan? expiry = null,
        When when = When.Always, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue> StringSetAndGetAsync(RedisKey key, RedisValue value, TimeSpan? expiry, bool keepTtl,
        When when = When.Always, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }



    public Task<bool> StringSetAsync(KeyValuePair<RedisKey, RedisValue>[] values, When when = When.Always,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<bool> StringSetBitAsync(RedisKey key, long offset, bool bit, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue> StringSetRangeAsync(RedisKey key, long offset, RedisValue value,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    // IRedisAsync

    public Task<TimeSpan> PingAsync(CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public bool TryWait(Task task)
    {
        throw new NotImplementedException();
    }

    public void Wait(Task task)
    {
        throw new NotImplementedException();
    }

    public T Wait<T>(Task<T> task)
    {
        throw new NotImplementedException();
    }

    public void WaitAll(params Task[] tasks)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue> DebugObjectAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }



    // Vector Async
#pragma warning disable SER001
    public Task<bool> VectorSetAddAsync(RedisKey key, VectorSetAddRequest request,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<bool> VectorSetContainsAsync(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<int> VectorSetDimensionAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<Lease<float>?> VectorSetGetApproximateVectorAsync(RedisKey key, RedisValue member,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<string?> VectorSetGetAttributesJsonAsync(RedisKey key, RedisValue member,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<Lease<RedisValue>?> VectorSetGetLinksAsync(RedisKey key, RedisValue member,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<Lease<VectorSetLink>?> VectorSetGetLinksWithScoresAsync(RedisKey key, RedisValue member,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<VectorSetInfo?> VectorSetInfoAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> VectorSetLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue> VectorSetRandomMemberAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue[]> VectorSetRandomMembersAsync(RedisKey key, long count,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<bool> VectorSetRemoveAsync(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<bool> VectorSetSetAttributesJsonAsync(RedisKey key, RedisValue member, string json,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<Lease<VectorSetSimilaritySearchResult>?> VectorSetSimilaritySearchAsync(RedisKey key,
        VectorSetSimilaritySearchRequest request, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }
#pragma warning restore SER001
}