using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using StackExchange.Redis;
using StackExchange.Redis.Profiling;
using StackExchange.Redis.Maintenance;

namespace ASPIRE.Tests.InProcess;

public partial class InProcessDistributedCacheStore
{
    private IConnectionMultiplexer? _multiplexer;

    public IConnectionMultiplexer Multiplexer => _multiplexer ??= new InProcessConnectionMultiplexer(this);

    private class InProcessConnectionMultiplexer(InProcessDistributedCacheStore store) : IConnectionMultiplexer
    {
        private readonly InProcessDistributedCacheStore _store = store;

        public string ClientName => "InProcess";
        public string Configuration => "InProcess";
        public int TimeoutMilliseconds => 0;
        public long OperationCount => 0;
        public bool PreserveAsyncOrder { get; set; }
        public bool IsConnected => true;
        public bool IsConnecting => false;
        public bool IncludeDetailInExceptions { get; set; }
        public int StormLogThreshold { get; set; }

#pragma warning disable CS0067
        public event EventHandler<RedisErrorEventArgs>? ErrorMessage;
        public event EventHandler<ConnectionFailedEventArgs>? ConnectionFailed;
        public event EventHandler<InternalErrorEventArgs>? InternalError;
        public event EventHandler<ConnectionFailedEventArgs>? ConnectionRestored;
        public event EventHandler<EndPointEventArgs>? ConfigurationChanged;
        public event EventHandler<EndPointEventArgs>? ConfigurationChangedBroadcast;
        public event EventHandler<HashSlotMovedEventArgs>? HashSlotMoved;
        public event EventHandler<ServerMaintenanceEvent>? ServerMaintenanceEvent;
#pragma warning restore CS0067

        public void AddLibraryNameSuffix(string suffix) { }
        public void Close(bool allowCommandsToComplete = true) { }
        public Task CloseAsync(bool allowCommandsToComplete = true) => Task.CompletedTask;
        public void Dispose() { }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        public EndPoint[] GetEndPoints(bool configuredOnly = false) => [new DnsEndPoint("localhost", 6379)];

        public void Wait(Task task) => task.Wait();
        public T Wait<T>(Task<T> task) => task.Result;
        public void WaitAll(params Task[] tasks) => Task.WaitAll(tasks);

        public int HashSlot(RedisKey key) => 0;
        public int GetHashSlot(RedisKey key) => 0;
        
        public ISubscriber GetSubscriber(object? asyncState = null) => throw new NotImplementedException();
        public IDatabase GetDatabase(int db = -1, object? asyncState = null) => _store;
        
        public IServer GetServer(string host, int port, object? asyncState = null) => new InProcessServer(_store);
        public IServer GetServer(string hostAndPort, object? asyncState = null) => new InProcessServer(_store);
        public IServer GetServer(IPAddress host, int port) => new InProcessServer(_store);
        public IServer GetServer(EndPoint endpoint, object? asyncState = null) => new InProcessServer(_store);
        public IServer GetServer(RedisKey key, object? asyncState = null, CommandFlags flags = CommandFlags.None) => new InProcessServer(_store);
        
        public IServer[] GetServers() => [new InProcessServer(_store)];

        public bool Configure(TextWriter? log = null) => true;
        public Task<bool> ConfigureAsync(TextWriter? log = null) => Task.FromResult(true);
        public string GetStatus() => "Connected";
        public void GetStatus(TextWriter log) => log.Write("Connected");
        public override string ToString() => "InProcessConnectionMultiplexer";

        public void RegisterProfiler(Func<ProfilingSession?> profilingSessionProvider) { }
        public ServerCounters GetCounters() => new(new DnsEndPoint("localhost", 6379));

        public void ExportConfiguration(Stream destination, ExportOptions options = ExportOptions.All) { }

        public string? GetStormLog() => null;
        public void ResetStormLog() { }
        public long PublishReconfigure(CommandFlags flags = CommandFlags.None) => 0;
        public Task<long> PublishReconfigureAsync(CommandFlags flags = CommandFlags.None) => Task.FromResult(0L);
    }

    private class InProcessServer(InProcessDistributedCacheStore store) : IServer
    {
        private readonly InProcessDistributedCacheStore _store = store;

        public IConnectionMultiplexer Multiplexer => _store.Multiplexer;

        public ClusterConfiguration? ClusterConfiguration => null;
        public EndPoint EndPoint => new DnsEndPoint("localhost", 6379);
        public RedisFeatures Features => new(new Version(6, 0));
        public bool IsConnected => true;
        public bool IsReplica => false;
        public bool IsSlave => false;
        public bool AllowSlaveWrites { get; set; }
        public bool AllowReplicaWrites { get; set; }
        public int DatabaseCount => 1;
        public ServerType ServerType => ServerType.Standalone;
        public Version Version => new(6, 0, 0);
        public RedisProtocol Protocol => RedisProtocol.Resp2;

        public TimeSpan Ping(CommandFlags flags = CommandFlags.None) => TimeSpan.Zero;
        public Task<TimeSpan> PingAsync(CommandFlags flags = CommandFlags.None) => Task.FromResult(TimeSpan.Zero);
        public void FlushDatabase(int database = -1, CommandFlags flags = CommandFlags.None) => _store.StoreItems.Clear();
        public Task FlushDatabaseAsync(int database = -1, CommandFlags flags = CommandFlags.None)
        {
            _store.StoreItems.Clear();
            return Task.CompletedTask;
        }

        public IEnumerable<RedisKey> Keys(int database = -1, RedisValue pattern = default, int pageSize = 250, long cursor = 0, int pageOffset = 0, CommandFlags flags = CommandFlags.None)
        {
            string patternString = pattern.IsNullOrEmpty ? "*" : pattern.ToString();
            string regexPattern = "^" + Regex.Escape(patternString).Replace(@"\*", ".*") + "$";
            Regex regex = new(regexPattern, RegexOptions.IgnoreCase);

            foreach (string key in _store.StoreItems.Keys)
            {
                if (regex.IsMatch(key))
                {
                    yield return key;
                }
            }
        }

        public IEnumerable<RedisKey> Keys(int database, RedisValue pattern, int pageSize, CommandFlags flags)
             => Keys(database, pattern, pageSize, 0, 0, flags);

        public IAsyncEnumerable<RedisKey> KeysAsync(int database = -1, RedisValue pattern = default, int pageSize = 250, long cursor = 0, int pageOffset = 0, CommandFlags flags = CommandFlags.None)
             => Keys(database, pattern, pageSize, cursor, pageOffset, flags).ToAsyncEnumerable();

        public void ClientKill(EndPoint endpoint, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public long ClientKill(long? id = null, ClientType? type = null, EndPoint? addr = null, bool skipMe = true, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public long ClientKill(ClientKillFilter filter, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        
        public Task<long> ClientKillAsync(long? id = null, ClientType? type = null, EndPoint? addr = null, bool skipMe = true, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public Task<long> ClientKillAsync(ClientKillFilter filter, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public Task ClientKillAsync(EndPoint endpoint, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        
        public StackExchange.Redis.ClientInfo[] ClientList(CommandFlags flags = CommandFlags.None) => [];
        public Task<StackExchange.Redis.ClientInfo[]> ClientListAsync(CommandFlags flags = CommandFlags.None) => Task.FromResult(Array.Empty<StackExchange.Redis.ClientInfo>());
        public StackExchange.Redis.ClientInfo[] ClientList(ClientType? type = null, CommandFlags flags = CommandFlags.None) => [];
        public Task<StackExchange.Redis.ClientInfo[]> ClientListAsync(ClientType? type = null, CommandFlags flags = CommandFlags.None) => Task.FromResult(Array.Empty<StackExchange.Redis.ClientInfo>());
        
        public KeyValuePair<string, string>[] ConfigGet(RedisValue pattern = default, CommandFlags flags = CommandFlags.None) => [];
        public Task<KeyValuePair<string, string>[]> ConfigGetAsync(RedisValue pattern = default, CommandFlags flags = CommandFlags.None) => Task.FromResult(Array.Empty<KeyValuePair<string, string>>());
        
        public void ConfigResetStat(CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public Task ConfigResetStatAsync(CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public void ConfigResetStatistics(CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public Task ConfigResetStatisticsAsync(CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();

        public void ConfigRewrite(CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public Task ConfigRewriteAsync(CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public void ConfigSet(RedisValue setting, RedisValue value, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public Task ConfigSetAsync(RedisValue setting, RedisValue value, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        
        public long DatabaseSize(int database = -1, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public Task<long> DatabaseSizeAsync(int database = -1, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        
        public RedisValue Echo(RedisValue message, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public Task<RedisValue> EchoAsync(RedisValue message, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        
        public RedisResult Execute(string command, params object[]? args) => RedisResult.Create((RedisValue)"OK");
        public Task<RedisResult> ExecuteAsync(string command, params object[]? args) => Task.FromResult(RedisResult.Create((RedisValue)"OK"));
        public RedisResult Execute(string command, ICollection<object>? args, CommandFlags flags = CommandFlags.None) => RedisResult.Create((RedisValue)"OK");
        public Task<RedisResult> ExecuteAsync(string command, ICollection<object>? args, CommandFlags flags = CommandFlags.None) => Task.FromResult(RedisResult.Create((RedisValue)"OK"));
        public RedisResult Execute(int? database, string command, ICollection<object>? args, CommandFlags flags = CommandFlags.None) => RedisResult.Create((RedisValue)"OK");
        public Task<RedisResult> ExecuteAsync(int? database, string command, ICollection<object>? args, CommandFlags flags = CommandFlags.None) => Task.FromResult(RedisResult.Create((RedisValue)"OK"));

        public void FlushAllDatabases(CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public Task FlushAllDatabasesAsync(CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        
        public ServerCounters GetCounters() => new(new DnsEndPoint("localhost", 6379));
        
        public IGrouping<string, KeyValuePair<string, string>>[] Info(RedisValue section = default, CommandFlags flags = CommandFlags.None) => [];
        public Task<IGrouping<string, KeyValuePair<string, string>>[]> InfoAsync(RedisValue section = default, CommandFlags flags = CommandFlags.None) => Task.FromResult(Array.Empty<IGrouping<string, KeyValuePair<string, string>>>());
        public string? InfoRaw(RedisValue section = default, CommandFlags flags = CommandFlags.None) => null;
        public Task<string?> InfoRawAsync(RedisValue section = default, CommandFlags flags = CommandFlags.None) => Task.FromResult((string?)null);

        public DateTime LastSave(CommandFlags flags = CommandFlags.None) => DateTime.UtcNow;
        public Task<DateTime> LastSaveAsync(CommandFlags flags = CommandFlags.None) => Task.FromResult(DateTime.UtcNow);
        public void Save(SaveType type, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public Task SaveAsync(SaveType type, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        
        public bool ScriptExists(string sha1, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public Task<bool> ScriptExistsAsync(string sha1, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public bool[] ScriptExists(RedisValue[] sha1s, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public Task<bool[]> ScriptExistsAsync(RedisValue[] sha1s, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public bool ScriptExists(byte[] sha1, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public Task<bool> ScriptExistsAsync(byte[] sha1, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();

        public void ScriptFlush(CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public Task ScriptFlushAsync(CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        
        public byte[] ScriptLoad(string script, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public Task<byte[]> ScriptLoadAsync(string script, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public byte[] ScriptLoad(byte[] script, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public Task<byte[]> ScriptLoadAsync(byte[] script, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public LoadedLuaScript ScriptLoad(LuaScript script, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public Task<LoadedLuaScript> ScriptLoadAsync(LuaScript script, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();

        // Using ShutdownMode.Default to avoid "None" ambiguity if Default exists
        public void Shutdown(ShutdownMode mode = ShutdownMode.Default, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public Task ShutdownAsync(ShutdownMode mode = ShutdownMode.Default, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        
        public void SlaveOf(EndPoint master, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public Task SlaveOfAsync(EndPoint master, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public void SlaveOfNoOne(CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public Task SlaveOfNoOneAsync(CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        
        public void SwapDB(int db1, int db2, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public Task SwapDBAsync(int db1, int db2, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public void SwapDatabases(int db1, int db2, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public Task SwapDatabasesAsync(int db1, int db2, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();

        public DateTime Time(CommandFlags flags = CommandFlags.None) => DateTime.UtcNow;
        public Task<DateTime> TimeAsync(CommandFlags flags = CommandFlags.None) => Task.FromResult(DateTime.UtcNow);

        public string LatencyDoctor(CommandFlags flags = CommandFlags.None) => string.Empty;
        public Task<string> LatencyDoctorAsync(CommandFlags flags = CommandFlags.None) => Task.FromResult(string.Empty);
        public long LatencyReset(string[]? eventNames = null, CommandFlags flags = CommandFlags.None) => 0;
        public Task<long> LatencyResetAsync(string[]? eventNames = null, CommandFlags flags = CommandFlags.None) => Task.FromResult(0L);
        public LatencyHistoryEntry[] LatencyHistory(string eventName, CommandFlags flags = CommandFlags.None) => [];
        public Task<LatencyHistoryEntry[]> LatencyHistoryAsync(string eventName, CommandFlags flags = CommandFlags.None) => Task.FromResult(Array.Empty<LatencyHistoryEntry>());
        public LatencyLatestEntry[] LatencyLatest(CommandFlags flags = CommandFlags.None) => [];
        public Task<LatencyLatestEntry[]> LatencyLatestAsync(CommandFlags flags = CommandFlags.None) => Task.FromResult(Array.Empty<LatencyLatestEntry>());

        public string MemoryDoctor(CommandFlags flags = CommandFlags.None) => string.Empty;
        public Task<string> MemoryDoctorAsync(CommandFlags flags = CommandFlags.None) => Task.FromResult(string.Empty);
        public void MemoryPurge(CommandFlags flags = CommandFlags.None) { }
        public Task MemoryPurgeAsync(CommandFlags flags = CommandFlags.None) => Task.CompletedTask;
        public RedisResult MemoryStats(CommandFlags flags = CommandFlags.None) => RedisResult.Create((RedisValue)"OK");
        public Task<RedisResult> MemoryStatsAsync(CommandFlags flags = CommandFlags.None) => Task.FromResult(RedisResult.Create((RedisValue)"OK"));
        public string? MemoryAllocatorStats(CommandFlags flags = CommandFlags.None) => null;
        public Task<string?> MemoryAllocatorStatsAsync(CommandFlags flags = CommandFlags.None) => Task.FromResult((string?)null);

        public RedisResult ModuleLoad(string path, string[]? arguments = null, CommandFlags flags = CommandFlags.None) => RedisResult.Create((RedisValue)"OK");
        public Task<RedisResult> ModuleLoadAsync(string path, string[]? arguments = null, CommandFlags flags = CommandFlags.None) => Task.FromResult(RedisResult.Create((RedisValue)"OK"));
        public void ModuleUnload(string name, CommandFlags flags = CommandFlags.None) { }
        public Task ModuleUnloadAsync(string name, CommandFlags flags = CommandFlags.None) => Task.CompletedTask;
        public RedisResult[] ModuleList(CommandFlags flags = CommandFlags.None) => [];
        public Task<RedisResult[]> ModuleListAsync(CommandFlags flags = CommandFlags.None) => Task.FromResult(Array.Empty<RedisResult>());
        
        public void ReplicaOf(EndPoint master, CommandFlags flags = CommandFlags.None) { }
        public Task ReplicaOfAsync(EndPoint master, CommandFlags flags = CommandFlags.None) => Task.CompletedTask;
        public void ReplicaOfNoOne(CommandFlags flags = CommandFlags.None) { }
        public Task ReplicaOfNoOneAsync(CommandFlags flags = CommandFlags.None) => Task.CompletedTask;
        
        public void MakeMaster(ReplicationChangeOptions options, TextWriter? log = null) { }
        public Task MakePrimaryAsync(ReplicationChangeOptions options, TextWriter? log = null) => Task.CompletedTask;

        // Correctly using explicit interface implementation for Role and returning null, as Role.Master is a Type not a Value
        StackExchange.Redis.Role IServer.Role(CommandFlags flags) => null!;
        Task<StackExchange.Redis.Role> IServer.RoleAsync(CommandFlags flags) => Task.FromResult<StackExchange.Redis.Role>(null!);
        
        public void Reset(CommandFlags flags = CommandFlags.None) { }
        public Task ResetAsync(CommandFlags flags = CommandFlags.None) => Task.CompletedTask;
        
        public void Wait(Task task) => task.Wait();
        public T Wait<T>(Task<T> task) => task.Result;
        public void WaitAll(params Task[] tasks) => Task.WaitAll(tasks);
        public bool TryWait(Task task) => task.Wait(0);

        public ClusterConfiguration ClusterNodes(CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public Task<ClusterConfiguration?> ClusterNodesAsync(CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        
        // Correcting return type to match typical interface requirements (string?)
        public string? ClusterNodesRaw(CommandFlags flags = CommandFlags.None) => string.Empty;
        public Task<string?> ClusterNodesRawAsync(CommandFlags flags = CommandFlags.None) => Task.FromResult((string?)string.Empty);

        public long CommandCount(CommandFlags flags = CommandFlags.None) => 0;
        public Task<long> CommandCountAsync(CommandFlags flags = CommandFlags.None) => Task.FromResult(0L);
        public RedisKey[] CommandGetKeys(RedisValue[] input, CommandFlags flags = CommandFlags.None) => [];
        public Task<RedisKey[]> CommandGetKeysAsync(RedisValue[] input, CommandFlags flags = CommandFlags.None) => Task.FromResult(Array.Empty<RedisKey>());
        public string[] CommandList(RedisValue? filterBy = null, RedisValue? filterValue = null, RedisValue? filterType = null, CommandFlags flags = CommandFlags.None) => [];
        public Task<string[]> CommandListAsync(RedisValue? filterBy = null, RedisValue? filterValue = null, RedisValue? filterType = null, CommandFlags flags = CommandFlags.None) => Task.FromResult(Array.Empty<string>());
        
        public void SentinelFailover(string serviceName, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public Task SentinelFailoverAsync(string serviceName, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public EndPoint? SentinelGetMasterAddressByName(string serviceName, CommandFlags flags = CommandFlags.None) => null;
        public Task<EndPoint?> SentinelGetMasterAddressByNameAsync(string serviceName, CommandFlags flags = CommandFlags.None) => Task.FromResult((EndPoint?)null);
        public EndPoint[] SentinelGetReplicaAddresses(string serviceName, CommandFlags flags = CommandFlags.None) => [];
        public Task<EndPoint[]> SentinelGetReplicaAddressesAsync(string serviceName, CommandFlags flags = CommandFlags.None) => Task.FromResult(Array.Empty<EndPoint>());
        public EndPoint[] SentinelGetSentinelAddresses(string serviceName, CommandFlags flags = CommandFlags.None) => [];
        public Task<EndPoint[]> SentinelGetSentinelAddressesAsync(string serviceName, CommandFlags flags = CommandFlags.None) => Task.FromResult(Array.Empty<EndPoint>());
        public KeyValuePair<string, string>[] SentinelMaster(string serviceName, CommandFlags flags = CommandFlags.None) => [];
        public Task<KeyValuePair<string, string>[]> SentinelMasterAsync(string serviceName, CommandFlags flags = CommandFlags.None) => Task.FromResult(Array.Empty<KeyValuePair<string, string>>());
        public KeyValuePair<string, string>[][] SentinelMasters(CommandFlags flags = CommandFlags.None) => [];
        public Task<KeyValuePair<string, string>[][]> SentinelMastersAsync(CommandFlags flags = CommandFlags.None) => Task.FromResult(Array.Empty<KeyValuePair<string, string>[]>());
        public KeyValuePair<string, string>[][] SentinelReplicas(string serviceName, CommandFlags flags = CommandFlags.None) => [];
        public Task<KeyValuePair<string, string>[][]> SentinelReplicasAsync(string serviceName, CommandFlags flags = CommandFlags.None) => Task.FromResult(Array.Empty<KeyValuePair<string, string>[]>());
        public KeyValuePair<string, string>[][] SentinelSentinels(string serviceName, CommandFlags flags = CommandFlags.None) => [];
        public Task<KeyValuePair<string, string>[][]> SentinelSentinelsAsync(string serviceName, CommandFlags flags = CommandFlags.None) => Task.FromResult(Array.Empty<KeyValuePair<string, string>[]>());
        public KeyValuePair<string, string>[][] SentinelSlaves(string serviceName, CommandFlags flags = CommandFlags.None) => [];
        public Task<KeyValuePair<string, string>[][]> SentinelSlavesAsync(string serviceName, CommandFlags flags = CommandFlags.None) => Task.FromResult(Array.Empty<KeyValuePair<string, string>[]>());

        public CommandTrace[] SlowlogGet(int count = 0, CommandFlags flags = CommandFlags.None) => [];
        public Task<CommandTrace[]> SlowlogGetAsync(int count = 0, CommandFlags flags = CommandFlags.None) => Task.FromResult(Array.Empty<CommandTrace>());
        public void SlowlogReset(CommandFlags flags = CommandFlags.None) { }
        public Task SlowlogResetAsync(CommandFlags flags = CommandFlags.None) => Task.CompletedTask;

        public RedisChannel[] SubscriptionChannels(RedisChannel pattern = default, CommandFlags flags = CommandFlags.None) => [];
        public Task<RedisChannel[]> SubscriptionChannelsAsync(RedisChannel pattern = default, CommandFlags flags = CommandFlags.None) => Task.FromResult(Array.Empty<RedisChannel>());
        public long SubscriptionPatternCount(CommandFlags flags = CommandFlags.None) => 0;
        public Task<long> SubscriptionPatternCountAsync(CommandFlags flags = CommandFlags.None) => Task.FromResult(0L);
        public long SubscriptionSubscriberCount(RedisChannel channel, CommandFlags flags = CommandFlags.None) => 0;
        public Task<long> SubscriptionSubscriberCountAsync(RedisChannel channel, CommandFlags flags = CommandFlags.None) => Task.FromResult(0L);
    }
}
