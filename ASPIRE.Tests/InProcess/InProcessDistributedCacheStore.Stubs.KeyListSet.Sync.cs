using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using StackExchange.Redis;

namespace ASPIRE.Tests.InProcess;

public partial class InProcessDistributedCacheStore
{
    // Key (Sync)
    public EndPoint? IdentifyEndpoint(RedisKey key, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();

    public bool KeyCopy(RedisKey sourceKey, RedisKey destinationKey, int destinationDatabase = -1, bool replace = false, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public bool KeyDelete(RedisKey key, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public long KeyDelete(RedisKey[] keys, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public byte[]? KeyDump(RedisKey key, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public string? KeyEncoding(RedisKey key, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public bool KeyExists(RedisKey key, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public long KeyExists(RedisKey[] keys, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public bool KeyExpire(RedisKey key, TimeSpan? expiry, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public bool KeyExpire(RedisKey key, TimeSpan? expiry, ExpireWhen when, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public bool KeyExpire(RedisKey key, DateTime? expiry, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public bool KeyExpire(RedisKey key, DateTime? expiry, ExpireWhen when, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public DateTime? KeyExpireTime(RedisKey key, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public long? KeyFrequency(RedisKey key, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public TimeSpan? KeyIdleTime(RedisKey key, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public void KeyMigrate(RedisKey key, EndPoint toEndpoint, int toDatabase = 0, int timeoutMilliseconds = 0, MigrateOptions options = MigrateOptions.None, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public bool KeyMove(RedisKey key, int database, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public bool KeyPersist(RedisKey key, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public RedisKey KeyRandom(CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public long? KeyRefCount(RedisKey key, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public bool KeyRename(RedisKey key, RedisKey newKey, When when = When.Always, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public void KeyRestore(RedisKey key, byte[] value, TimeSpan? expiry = null, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public TimeSpan? KeyTimeToLive(RedisKey key, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public bool KeyTouch(RedisKey key, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public long KeyTouch(RedisKey[] keys, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public RedisType KeyType(RedisKey key, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();

    // List
    public RedisValue ListGetByIndex(RedisKey key, long index, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public long ListInsertAfter(RedisKey key, RedisValue pivot, RedisValue value, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public long ListInsertBefore(RedisKey key, RedisValue pivot, RedisValue value, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public RedisValue ListLeftPop(RedisKey key, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public RedisValue[] ListLeftPop(RedisKey key, long count, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public ListPopResult ListLeftPop(RedisKey[] keys, long count, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public long ListLeftPush(RedisKey key, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public long ListLeftPush(RedisKey key, RedisValue[] values, When when = When.Always, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public long ListLeftPush(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public long ListLength(RedisKey key, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public RedisValue ListMove(RedisKey source, RedisKey destination, ListSide sourceSide, ListSide destinationSide, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public long ListPosition(RedisKey key, RedisValue value, long rank = 1, long maxLength = 0, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public long[] ListPositions(RedisKey key, RedisValue value, long rank = 1, long count = 1, long maxLength = 0, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public RedisValue[] ListRange(RedisKey key, long start = 0, long stop = -1, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public long ListRemove(RedisKey key, RedisValue value, long count = 0, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public RedisValue ListRightPop(RedisKey key, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public RedisValue[] ListRightPop(RedisKey key, long count, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public ListPopResult ListRightPop(RedisKey[] keys, long count, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public RedisValue ListRightPopLeftPush(RedisKey source, RedisKey destination, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public long ListRightPush(RedisKey key, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public long ListRightPush(RedisKey key, RedisValue[] values, When when = When.Always, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public long ListRightPush(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public void ListSetByIndex(RedisKey key, long index, RedisValue value, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public void ListTrim(RedisKey key, long start, long stop, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();

    // Lock
    public bool LockExtend(RedisKey key, RedisValue value, TimeSpan expiry, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public RedisValue LockQuery(RedisKey key, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public bool LockRelease(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public bool LockTake(RedisKey key, RedisValue value, TimeSpan expiry, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();

    // Publish
    public long Publish(RedisChannel channel, RedisValue message, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();

    // Script
    public RedisResult ScriptEvaluate(string script, RedisKey[]? keys = null, RedisValue[]? values = null, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public RedisResult ScriptEvaluate(byte[] script, RedisKey[]? keys = null, RedisValue[]? values = null, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public RedisResult ScriptEvaluate(LuaScript script, object? parameters = null, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public RedisResult ScriptEvaluate(LoadedLuaScript script, object? parameters = null, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public RedisResult ScriptEvaluateReadOnly(string script, RedisKey[]? keys = null, RedisValue[]? values = null, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public RedisResult ScriptEvaluateReadOnly(byte[] script, RedisKey[]? keys = null, RedisValue[]? values = null, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();

    // Set
    public bool SetAdd(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public long SetAdd(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public RedisValue[] SetCombine(SetOperation operation, RedisKey first, RedisKey second, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public RedisValue[] SetCombine(SetOperation operation, RedisKey[] keys, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public long SetCombineAndStore(SetOperation operation, RedisKey destination, RedisKey first, RedisKey second, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public long SetCombineAndStore(SetOperation operation, RedisKey destination, RedisKey[] keys, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public bool SetContains(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public bool[] SetContains(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public long SetIntersectionLength(RedisKey[] keys, long limit = 0, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public long SetLength(RedisKey key, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public RedisValue[] SetMembers(RedisKey key, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public bool SetMove(RedisKey source, RedisKey destination, RedisValue value, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public RedisValue SetPop(RedisKey key, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public RedisValue[] SetPop(RedisKey key, long count, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public RedisValue SetRandomMember(RedisKey key, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public RedisValue[] SetRandomMembers(RedisKey key, long count, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public bool SetRemove(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public long SetRemove(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public IEnumerable<RedisValue> SetScan(RedisKey key, RedisValue pattern = default, int pageSize = 250, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
    public IEnumerable<RedisValue> SetScan(RedisKey key, RedisValue pattern, int pageSize, long cursor, int pageOffset, CommandFlags flags) => throw new NotImplementedException();
}
