namespace TRANSMUTANSTEIN.ChatServer.Domain.Core;

public class MatchServerChatSession(ConnectionContext connection, IServiceProvider serviceProvider) : ChatSession(connection, serviceProvider)
{
    /// <summary>
    ///     Gets set after a successful match server handshake.
    ///     Contains metadata about the match server connected to this chat session.
    /// </summary>
    public MatchServerChatSessionMetadata Metadata { get; set; } = null!;

    /// <summary>
    ///     Gets set after a successful match server handshake.
    ///     Contains the account information of the match server connected to this chat session.
    /// </summary>
    public Account Account { get; set; } = null!;

    /// <summary>
    ///     Completes the connection process by sending the accept packet.
    /// </summary>
    public MatchServerChatSession SetOnline()
    {
        Metadata.LastStatusUpdate = DateTimeOffset.UtcNow;

        ChatBuffer accept = new ();

        accept.WriteCommand(ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_ACCEPT);

        Send(accept);

        return this;
    }

    /// <summary>
    ///     Sends server configuration options to the match server.
    /// </summary>
    /// <param name="remoteCommands">Additional remote commands to execute on the server.</param>
    public MatchServerChatSession SendOptions(IEnumerable<string>? remoteCommands = null)
    {
        ChatBuffer options = new ();

        options.WriteCommand(ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_OPTIONS);

        // Match ID Required: Whether The Server Must Request A Match ID From The Master Server
        options.WriteBool(true);

        // Match ID Request Attempts: Number Of Times The Server Will Attempt To Request A Match ID
        options.WriteInt32(3);

        // Match ID Request Total Timeout: Milliseconds Before All Match ID Requests Time Out
        options.WriteInt32(15000);

        // Match ID Request Interval: Milliseconds Between Match ID Request Attempts
        options.WriteInt32(500);

        // Auth Request Attempts: Number Of Times The Server Will Attempt To Authenticate A Client
        options.WriteInt32(3);

        // Auth Request Total Timeout: Milliseconds Before All Auth Requests Time Out
        options.WriteInt32(30000);

        // Auth Request Interval: Milliseconds Between Auth Request Attempts
        options.WriteInt32(10000);

        // Submit Stats: Whether The Server Should Submit Statistics
        options.WriteBool(true);

        // Upload Replays: Whether The Server Should Upload Replays
        options.WriteBool(true);

        // Upload To FTP: Whether Replays Should Be Uploaded To FTP
        options.WriteBool(false);

        // Upload To S3: Whether Replays Should Be Uploaded To S3
        options.WriteBool(true);

        // Max Connection Attempts: Maximum Client Connection Attempts
        options.WriteInt32(10);

        // Max Incoming Packets Per Second
        options.WriteInt32(1000);

        // Max Incoming Bytes Per Second
        options.WriteInt32(65536);

        // Submit Match Stat Disconnects: Whether To Track Disconnect Statistics
        options.WriteBool(true);

        // Heartbeat Interval: Milliseconds Between Status Updates (Minimum 60000)
        options.WriteInt32(60000);

        // Disconnect Reporting: Whether To Report Disconnect Reasons
        options.WriteBool(true);

        // Quests Availability: Whether Quests Are Enabled
        options.WriteInt8(Convert.ToByte(ChatProtocol.QuestsAvailabilityType.EQAT_ENABLED));

        // Quests Ladder Availability: Whether Quest Leaderboards Are Enabled
        options.WriteInt8(Convert.ToByte(ChatProtocol.QuestsAvailabilityType.EQAT_ENABLED));

        // Log Product Usage: Whether To Log Product Usage Statistics
        options.WriteBool(true);

        // Dynamic Concede Times: Whether Concede Times Are Dynamic Based On Match State
        options.WriteBool(true);

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
    ///     Sends a remote command to the match server for execution.
    /// </summary>
    /// <param name="command">The command string to execute.</param>
    public MatchServerChatSession SendRemoteCommand(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
            return this;

        ChatBuffer remoteCommand = new ();

        remoteCommand.WriteCommand(ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_REMOTE_COMMAND);
        remoteCommand.WriteString(Metadata.SessionCookie);
        remoteCommand.WriteString(command);

        Send(remoteCommand);

        return this;
    }

    /// <summary>
    ///     Sends multiple remote commands to the match server.
    /// </summary>
    /// <param name="commands">The commands to execute.</param>
    public MatchServerChatSession SendRemoteCommands(IEnumerable<string> commands)
    {
        foreach (string command in commands)
            SendRemoteCommand(command);

        return this;
    }

    /// <summary>
    ///     Updates the match server's status from a status update packet.
    /// </summary>
    public MatchServerChatSession UpdateStatus(ChatProtocol.ServerStatus status)
    {
        Metadata.Status = status;
        Metadata.LastStatusUpdate = DateTimeOffset.UtcNow;

        return this;
    }

    /// <summary>
    ///     Marks this server as recently used to prevent immediate reuse.
    /// </summary>
    /// <param name="expirationSeconds">Number of seconds before the server can be reused.</param>
    public MatchServerChatSession SetRecentlyUsed(int expirationSeconds = 30)
    {
        Metadata.RecentlyUsedExpiration = DateTimeOffset.UtcNow.AddSeconds(expirationSeconds);

        return this;
    }

    /// <summary>
    ///     Checks if this server was recently used and should not be selected for new matches.
    /// </summary>
    public bool WasRecentlyUsed()
    {
        if (Metadata.RecentlyUsedExpiration is null)
            return false;

        return DateTimeOffset.UtcNow < Metadata.RecentlyUsedExpiration;
    }

    /// <summary>
    ///     Clears the recently used expiration flag.
    /// </summary>
    public MatchServerChatSession ClearRecentlyUsed()
    {
        Metadata.RecentlyUsedExpiration = null;

        return this;
    }

    /// <summary>
    ///     Sends a request to create a match on this server.
    /// </summary>
    /// <param name="matchPacket">The match creation packet to send.</param>
    public MatchServerChatSession CreateMatch(ChatBuffer matchPacket)
    {
        Send(matchPacket);
        SetRecentlyUsed();

        return this;
    }

    /// <summary>
    ///     Sends a request to end the current match on this server.
    /// </summary>
    /// <param name="losingTeam">The team that lost (0 or 1, or -1 for draw/abort).</param>
    public MatchServerChatSession EndMatch(int losingTeam = -1)
    {
        ChatBuffer endMatch = new ();

        endMatch.WriteCommand(ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_END_MATCH);
        endMatch.WriteInt32(losingTeam);

        Send(endMatch);

        return this;
    }

    /// <summary>
    ///     Checks if this server is available for a new match.
    /// </summary>
    public bool IsAvailable()
    {
        return Metadata.Status is ChatProtocol.ServerStatus.SERVER_STATUS_IDLE && WasRecentlyUsed() is false;
    }

    /// <summary>
    ///     Gets the full server name including location.
    /// </summary>
    public string GetFullServerName()
    {
        return $"{Metadata.Location ?? "Unknown"} - {Metadata.Name ?? $"Server {Metadata.ServerID}"}";
    }

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
        if (Context.MatchServerChatSessions.TryRemove(new KeyValuePair<int, MatchServerChatSession>(Metadata.ServerID, this)))
        {
            Log.Information(@"Match Server ID ""{MatchServerID}"" Was Removed From The Match Server Pool", Metadata.ServerID);

            Terminal.Broadcast($"Match Server {Metadata.ServerID} Terminated", Terminal.ServersPerRegion());

            return true;
        }

        // The Pool Entry No Longer Maps To This Session, So It Was Superseded By A Reconnecting Session Which Now Owns The Shared Host State
        Log.Debug(@"Match Server ID ""{MatchServerID}"" Was Already Superseded And Will Not Tear Down Shared Host State", Metadata.ServerID);

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
    ///     Performs an immediate, authoritative teardown of this session, reached on a graceful shutdown, a rejected handshake, or when the <see cref="StaleHostReaper"/> reaps a hung session.
    ///     The distributed cache entry is removed right away, whereas the dropped-connection path (<see cref="OnDisconnected"/>) leaves it for the reaper to reconcile after its grace period.
    /// </summary>
    public async Task Terminate(IDatabase distributedCacheStore)
    {
        if (RemoveFromPoolIfCurrentHolder())
        {
            // Send The Quit Command While The Socket Is Still Open
            if (IsConnected)
                SendRemoteCommand("quit");

            // Await The Distributed Cache Removal Here So That Callers Which Immediately Register A Replacement Server Do Not Race The Removal
            await Remove(distributedCacheStore);

            Log.Information(@"Match Server ID ""{MatchServerID}"" Has Disconnected Gracefully", Metadata.ServerID);
        }

        // Tear Down The Connection, Flushing Any Queued Frames (Such As The "quit" Remote Command) Before The Socket Is Closed
        await CloseGracefully();
    }

    /// <summary>
    ///     Invoked by the TCP transport after the underlying socket has been closed (graceful disconnect, network drop, crash, or keep-alive timeout).
    ///     On a dropped connection, it removes the in-memory session so the server is never selected for a match, but deliberately leaves the distributed cache entry in place for the <see cref="StaleHostReaper"/> to reconcile.
    ///     When the disconnect is graceful, <see cref="Terminate"/> has already torn the session down, so there is nothing left for this method to do.
    /// </summary>
    protected override void OnDisconnected()
    {
        RemoveFromPoolIfCurrentHolder();

        /*
            The Session Is Removed From The In-Memory Pool (Which Excludes It From Match Selection), But The Distributed Cache Entry Is Left In Place
            A Dropped Connection Is Frequently A Brief Reconnect: The Host Reuses Its Session Cookie, And The Handshake Validates That Cookie Against The Cache Entry, So Tearing The Entry Down Here Would Reject The Reconnect
            If The Host Does Not Reconnect, The <see cref="StaleHostReaper"/> Reaps The Now-Sessionless Cache Entry After Its Grace Period
            A Graceful Shutdown Still Tears Down Immediately Via "Terminate"
         */

        base.OnDisconnected();
    }

    private async Task Remove(IDatabase distributedCacheStore)
    {
        // Remove Match Server From The Distributed Cache
        await distributedCacheStore.RemoveMatchServerByID(Metadata.ServerID);
    }
}
