using System.Net;

namespace ASPIRE.Tests.InProcess;

public partial class InProcessDistributedCacheStore
{
    // Key Async
    public Task<EndPoint?> IdentifyEndpointAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public bool IsConnected(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<bool> KeyCopyAsync(RedisKey sourceKey, RedisKey destinationKey, int destinationDatabase = -1,
        bool replace = false, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    // Skipped KeyDeleteAsync singular (Implemented)
    public Task<long> KeyDeleteAsync(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<byte[]?> KeyDumpAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<string?> KeyEncodingAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<bool> KeyExistsAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        return Task.FromResult(StoreItems.ContainsKey(key.ToString()));
    }

    public Task<long> KeyExistsAsync(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<bool> KeyExpireAsync(RedisKey key, TimeSpan? expiry, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<bool> KeyExpireAsync(RedisKey key, TimeSpan? expiry, ExpireWhen when,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<bool> KeyExpireAsync(RedisKey key, DateTime? expiry, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<bool> KeyExpireAsync(RedisKey key, DateTime? expiry, ExpireWhen when,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<DateTime?> KeyExpireTimeAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long?> KeyFrequencyAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<TimeSpan?> KeyIdleTimeAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task KeyMigrateAsync(RedisKey key, EndPoint toEndpoint, int toDatabase = 0, int timeoutMilliseconds = 0,
        MigrateOptions options = MigrateOptions.None, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<bool> KeyMoveAsync(RedisKey key, int database, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<bool> KeyPersistAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisKey> KeyRandomAsync(CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long?> KeyRefCountAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<bool> KeyRenameAsync(RedisKey key, RedisKey newKey, When when = When.Always,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task KeyRestoreAsync(RedisKey key, byte[] value, TimeSpan? expiry = null,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<TimeSpan?> KeyTimeToLiveAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<bool> KeyTouchAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> KeyTouchAsync(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisType> KeyTypeAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    // List Async
    public Task<RedisValue> ListGetByIndexAsync(RedisKey key, long index, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> ListInsertAfterAsync(RedisKey key, RedisValue pivot, RedisValue value,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> ListInsertBeforeAsync(RedisKey key, RedisValue pivot, RedisValue value,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue> ListLeftPopAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue[]> ListLeftPopAsync(RedisKey key, long count, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<ListPopResult> ListLeftPopAsync(RedisKey[] keys, long count, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> ListLeftPushAsync(RedisKey key, RedisValue value, When when = When.Always,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> ListLeftPushAsync(RedisKey key, RedisValue[] values, When when = When.Always,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> ListLeftPushAsync(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> ListLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue> ListMoveAsync(RedisKey source, RedisKey destination, ListSide sourceSide,
        ListSide destinationSide, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> ListPositionAsync(RedisKey key, RedisValue value, long rank = 1, long maxLength = 0,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long[]> ListPositionsAsync(RedisKey key, RedisValue value, long rank = 1, long count = 1,
        long maxLength = 0, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue[]> ListRangeAsync(RedisKey key, long start = 0, long stop = -1,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> ListRemoveAsync(RedisKey key, RedisValue value, long count = 0,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue> ListRightPopAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue[]> ListRightPopAsync(RedisKey key, long count, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<ListPopResult> ListRightPopAsync(RedisKey[] keys, long count, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue> ListRightPopLeftPushAsync(RedisKey source, RedisKey destination,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> ListRightPushAsync(RedisKey key, RedisValue value, When when = When.Always,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> ListRightPushAsync(RedisKey key, RedisValue[] values, When when = When.Always,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> ListRightPushAsync(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task ListSetByIndexAsync(RedisKey key, long index, RedisValue value, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task ListTrimAsync(RedisKey key, long start, long stop, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    // Lock Async
    public Task<bool> LockExtendAsync(RedisKey key, RedisValue value, TimeSpan expiry,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue> LockQueryAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<bool> LockReleaseAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<bool> LockTakeAsync(RedisKey key, RedisValue value, TimeSpan expiry,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    // Publish Async
    public Task<long> PublishAsync(RedisChannel channel, RedisValue message, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    // Script Async
    public Task<RedisResult> ScriptEvaluateAsync(string script, RedisKey[]? keys = null, RedisValue[]? values = null,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisResult> ScriptEvaluateAsync(byte[] script, RedisKey[]? keys = null, RedisValue[]? values = null,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisResult> ScriptEvaluateAsync(LuaScript script, object? parameters = null,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisResult> ScriptEvaluateAsync(LoadedLuaScript script, object? parameters = null,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisResult> ScriptEvaluateReadOnlyAsync(string script, RedisKey[]? keys = null,
        RedisValue[]? values = null, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisResult> ScriptEvaluateReadOnlyAsync(byte[] script, RedisKey[]? keys = null,
        RedisValue[]? values = null, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    // Set Async
    public Task<bool> SetAddAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> SetAddAsync(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> SetCombineAndStoreAsync(SetOperation operation, RedisKey destination, RedisKey first,
        RedisKey second, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> SetCombineAndStoreAsync(SetOperation operation, RedisKey destination, RedisKey[] keys,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue[]> SetCombineAsync(SetOperation operation, RedisKey first, RedisKey second,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue[]> SetCombineAsync(SetOperation operation, RedisKey[] keys,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SetContainsAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<bool[]> SetContainsAsync(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> SetIntersectionLengthAsync(RedisKey[] keys, long limit = 0,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> SetLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue[]> SetMembersAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SetMoveAsync(RedisKey source, RedisKey destination, RedisValue value,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue> SetPopAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue[]> SetPopAsync(RedisKey key, long count, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue> SetRandomMemberAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue[]> SetRandomMembersAsync(RedisKey key, long count, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SetRemoveAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<long> SetRemoveAsync(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<RedisValue> SetScanAsync(RedisKey key, RedisValue pattern = default, int pageSize = 250,
        long cursor = 0, int pageOffset = 0, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    // Sort Async (Adding here as it was missed in Sync2/3 split boundary or belongs here?)
    // Actually Sort operations are IDatabase members, can be anywhere. Adding here.
    public Task<long> SortAndStoreAsync(RedisKey destination, RedisKey key, long skip = 0, long take = -1,
        Order order = Order.Ascending, SortType sortType = SortType.Numeric, RedisValue by = default,
        RedisValue[]? get = null, CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }

    public Task<RedisValue[]> SortAsync(RedisKey key, long skip = 0, long take = -1, Order order = Order.Ascending,
        SortType sortType = SortType.Numeric, RedisValue by = default, RedisValue[]? get = null,
        CommandFlags flags = CommandFlags.None)
    {
        throw new NotImplementedException();
    }
}