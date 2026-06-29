namespace TRANSMUTANSTEIN.ChatServer.Domain.Core;

public class MatchServerManagerChatSession(ConnectionContext connection, IServiceProvider serviceProvider) : ChatSession(connection, serviceProvider)
{
    /// <summary>
    ///     Gets set after a successful match server manager handshake.
    ///     Contains metadata about the match server manager connected to this chat session.
    /// </summary>
    public MatchServerManagerChatSessionMetadata Metadata { get; set; } = null!;

    /// <summary>
    ///     Gets set after a successful match server manager handshake.
    ///     Contains the account information of the match server manager connected to this chat session.
    /// </summary>
    public Account Account { get; set; } = null!;

    /// <summary>
    ///     Completes the connection process by sending the accept packet.
    /// </summary>
    public MatchServerManagerChatSession SetOnline()
    {
        Metadata.LastStatusUpdate = DateTimeOffset.UtcNow;

        ChatBuffer accept = new ();

        accept.WriteCommand(ChatProtocol.ChatServerToServerManager.NET_CHAT_SM_ACCEPT);

        Send(accept);

        return this;
    }

    /// <summary>
    ///     Sends server manager configuration options.
    /// </summary>
    /// <param name="remoteCommands">Additional remote commands to execute.</param>
    public MatchServerManagerChatSession SendOptions(IEnumerable<string>? remoteCommands = null)
    {
        ChatBuffer options = new ();

        options.WriteCommand(ChatProtocol.ChatServerToServerManager.NET_CHAT_SM_OPTIONS);

        // Submit Stats Enabled
        options.WriteInt8(Convert.ToByte(true));

        // Upload Replays Enabled
        options.WriteInt8(Convert.ToByte(true));

        // Upload To FTP On Demand Enabled
        options.WriteInt8(Convert.ToByte(false));

        // Upload To HTTP On Demand Enabled
        options.WriteInt8(Convert.ToByte(true));

        // Resubmit Stats Enabled
        options.WriteInt8(Convert.ToByte(true));

        // Stats Resubmit Match ID Cut-Off (Minimum Match ID For Resubmission)
        // TODO: Set To Highest Match ID Stored In The Database
        options.WriteInt32(1);

        Send(options);

        // Send Remote Commands If Provided
        if (remoteCommands is not null)
        {
            foreach (string command in remoteCommands)
                SendRemoteCommand(command);
        }

        return this;
    }

    /// <summary>
    ///     Sends a remote command to the server manager for execution.
    /// </summary>
    /// <param name="command">The command string to execute.</param>
    public MatchServerManagerChatSession SendRemoteCommand(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
            return this;

        ChatBuffer remoteCommand = new ();

        remoteCommand.WriteCommand(ChatProtocol.ChatServerToServerManager.NET_CHAT_SM_REMOTE_COMMAND);
        remoteCommand.WriteString(Metadata.SessionCookie);
        remoteCommand.WriteString(command);

        Send(remoteCommand);

        return this;
    }

    /// <summary>
    ///     Sends multiple remote commands to the server manager.
    /// </summary>
    /// <param name="commands">The commands to execute.</param>
    public MatchServerManagerChatSession SendRemoteCommands(IEnumerable<string> commands)
    {
        foreach (string command in commands)
            SendRemoteCommand(command);

        return this;
    }

    /// <summary>
    ///     Requests that the server manager begin a graceful shutdown of itself and the match servers it manages.
    /// </summary>
    public MatchServerManagerChatSession ScheduleShutdown()
    {
        return SendRemoteCommand("ManagerStartShutdown");
    }

    /// <summary>
    ///     Requests that the server manager cancel a graceful shutdown which has been scheduled but has not yet completed.
    /// </summary>
    public MatchServerManagerChatSession CancelShutdown()
    {
        return SendRemoteCommand("ManagerCancelShutdown");
    }

    /// <summary>
    ///     Requests that the server manager begin a graceful reset, restarting the match servers it manages once they are no longer hosting matches.
    /// </summary>
    public MatchServerManagerChatSession ScheduleReset()
    {
        return SendRemoteCommand("ManagerStartReset");
    }

    /// <summary>
    ///     Gets the live chat sessions of this manager's match servers, by reading the manager's server IDs from the distributed cache (the source of truth) and resolving those which currently hold a live chat session in the pool.
    /// </summary>
    public async Task<List<MatchServerChatSession>> GetMatchServerSessions(IDatabase distributedCacheStore)
    {
        MatchServerManager? matchServerManager = await distributedCacheStore.GetMatchServerManagerByID(Metadata.ServerManagerID);

        List<MatchServerChatSession> matchServerSessions = [];

        if (matchServerManager is null)
            return matchServerSessions;

        foreach (int matchServerID in matchServerManager.MatchServerIDs)
        {
            if (Context.MatchServerChatSessions.TryGetValue(matchServerID, out MatchServerChatSession? matchServerSession))
                matchServerSessions.Add(matchServerSession);
        }

        return matchServerSessions;
    }

    /// <summary>
    ///     Relays a remote command to every one of this manager's match servers which currently holds a live chat session.
    /// </summary>
    /// <param name="command">The command string to execute on each match server.</param>
    public async Task BroadcastToMatchServers(IDatabase distributedCacheStore, string command)
    {
        foreach (MatchServerChatSession matchServerSession in await GetMatchServerSessions(distributedCacheStore))
            matchServerSession.SendRemoteCommand(command);
    }

    // TODO: Implement On-Demand Replay And Log Uploads Via "NET_CHAT_SM_UPLOAD_REQUEST"
    // The Server Manager Uploads A Completed Match's Replay Or Log On Request, So That A Client Can Obtain A File Which Is Not Already Available For Download
    // The Flow Is: A Client Sends "CHAT_CMD_UPLOAD_REQUEST" (Match ID And File Extension), The Chat Server Resolves Which Host Manager Holds That Match, Sends That Manager A "NET_CHAT_SM_UPLOAD_REQUEST", The Manager Uploads And Replies With "NET_CHAT_SM_UPLOAD_UPDATE", And The Chat Server Forwards A "CHAT_CMD_UPLOAD_STATUS" Back To The Client
    // The "NET_CHAT_SM_UPLOAD_REQUEST" Payload Is The Requesting Account ID, The Match ID, The File Extension, The Destination Host, The Destination Directory, A Flag For FTP Upload, A Flag For S3 Upload, And A Download Link
    // For The Authoritative Model See "CClient::HandleRequestSMUpload" With "CClientManager::RequestSMUpload" In The HON Chat Server Source, And "UploadRequest" With "UploadRequestResponse" In The Legacy KONGOR Source

    /// <summary>
    ///     Tracks whether the cleanup has already run for this session.
    ///     Required because the graceful (<see cref="Terminate"/>), dropped (<see cref="OnDisconnected"/>), and superseded (<see cref="Supersede"/>) paths can all be reached for the same session, and only one of them should take effect.
    /// </summary>
    private int CleanupCompleted;

    /// <summary>
    ///     Removes this session from the in-memory pool, but only if the pool entry still maps to this exact session.
    ///     Returns whether this session was the current pool holder, so the caller tears down the shared host state (the distributed cache entry) only when this session genuinely owned it.
    ///     A session that has been superseded by a reconnecting one returns <see langword="false"/> and leaves the shared host state for its replacement.
    /// </summary>
    private bool RemoveFromPoolIfCurrentHolder()
    {
        // Use Interlocked To Guarantee The Body Runs At Most Once, Even Under Concurrent Disconnect Paths
        if (Interlocked.Exchange(ref CleanupCompleted, 1) is 1)
            return false;

        // If The Metadata Is NULL, The Handshake Never Completed And The Session Was Never Added To The Pool, So There Is Nothing To Clean Up
        if (Metadata is null)
            return false;

        // Remove The Pool Entry Only If It Still Maps To This Exact Session
        if (Context.MatchServerManagerChatSessions.TryRemove(new KeyValuePair<int, MatchServerManagerChatSession>(Metadata.ServerManagerID, this)))
        {
            Log.Information(@"Match Server Manager ID ""{MatchServerManagerID}"" Was Removed From The Match Server Manager Pool", Metadata.ServerManagerID);

            Terminal.Broadcast($"Match Server Manager {Metadata.ServerManagerID} Terminated", Terminal.ManagersPerRegion());

            return true;
        }

        // The Pool Entry No Longer Maps To This Session, So It Was Superseded By A Reconnecting Session Which Now Owns The Shared Host State
        Log.Debug(@"Match Server Manager ID ""{MatchServerManagerID}"" Was Already Superseded And Will Not Tear Down Shared Host State", Metadata.ServerManagerID);

        return false;
    }

    /// <summary>
    ///     Tears down this session because the host is reconnecting and a replacement session has taken over its entry in the pool.
    ///     The socket is closed, but the shared host state (the distributed cache entry) is deliberately preserved for the replacement session, because the reconnecting host reuses its session cookie and the cache entry is what validates it.
    /// </summary>
    public void Supersede()
    {
        // Mark The Cleanup As Done So The Resulting OnDisconnected Leaves The Shared Host State For The Replacement Session
        Interlocked.Exchange(ref CleanupCompleted, 1);

        // Tear Down The Connection; The Stale Socket Is Force-Closed While The Shared Host State Is Preserved For The Replacement Session
        Disconnect();
    }

    /// <summary>
    ///     Performs an immediate, authoritative teardown of this session, reached on a graceful shutdown or a rejected handshake.
    ///     The distributed cache entry is removed right away, whereas the dropped-connection path (<see cref="OnDisconnected"/>) leaves it for the <see cref="StaleHostReaper"/> to reconcile after its grace period.
    /// </summary>
    public async Task Terminate(IDatabase distributedCacheStore)
    {
        if (RemoveFromPoolIfCurrentHolder())
        {
            // Await The Distributed Cache Removal Here So That Callers Which Immediately Register A Replacement Manager Do Not Race The Removal
            await Remove(distributedCacheStore);

            Log.Information(@"Match Server Manager ID ""{MatchServerManagerID}"" Has Disconnected Gracefully", Metadata.ServerManagerID);
        }

        // Tear Down The Connection, Flushing Any Queued Frames Before The Socket Is Closed
        await CloseGracefully();
    }

    /// <summary>
    ///     Invoked by the TCP transport after the underlying socket has been closed (graceful disconnect, network drop, crash, or keep-alive timeout).
    ///     On a dropped connection, it removes the in-memory session, but deliberately leaves the distributed cache entry in place for the <see cref="StaleHostReaper"/> to reconcile.
    ///     When the disconnect is graceful, <see cref="Terminate"/> has already torn the session down, so there is nothing left for this method to do.
    /// </summary>
    protected override void OnDisconnected()
    {
        RemoveFromPoolIfCurrentHolder();

        /*
            The Session Is Removed From The In-Memory Pool, But The Distributed Cache Entry Is Left In Place
            A Dropped Connection Is Frequently A Brief Reconnect (The Host Reuses Its Session Cookie), And The Cache Entry Is What Validates That Reused Cookie On The Reconnecting Handshake
            Tearing The Cache Entry Down Here Would Reject The Reconnect
            If The Manager Does Not Reconnect, <see cref="StaleHostReaper"/> Reaps The Cache Entry After Its Grace Period
            A Graceful Shutdown Still Tears Down Immediately Via "Terminate"
         */

        base.OnDisconnected();
    }

    private async Task Remove(IDatabase distributedCacheStore)
    {
        // Remove Match Server Manager From The Distributed Cache
        // Match Server Children Are Also Implicitly Removed From The Distributed Cache
        await distributedCacheStore.RemoveMatchServerManagerByID(Metadata.ServerManagerID);
    }
}
