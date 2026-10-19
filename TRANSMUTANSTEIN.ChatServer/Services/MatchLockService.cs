namespace TRANSMUTANSTEIN.ChatServer.Services;

/// <summary>
///     Decides whether a match lock still holds before its player is sent back to the locked match, and lifts the lock when it does not.
///     A lock holds only while its match server is registered and alive, still hosts the locked match, and can still seat the player, so that nobody is ever sent to a dead server, a finished match, or an unreclaimable slot.
/// </summary>
public class MatchLockService(IDatabase distributedCacheStore, MatchServerReconnectQuery reconnectQuery, ILogger<MatchLockService> logger)
{
    /// <summary>
    ///     The IDs of the match servers whose unanswered reconnect query has been logged as an error, so that an unreachable match server is logged once rather than at every verification.
    ///     An ID is removed when its match server next answers, so that the match server is logged again if it becomes unreachable again.
    /// </summary>
    private ConcurrentDictionary<int, byte> UnreachableServerIDs { get; } = [];

    /// <summary>
    ///     Verifies a match lock against its match server, and lifts the lock unless the verdict is <see cref="MatchLockVerdict.SendBack"/>.
    ///     The player's slot is queried only when the match server does not rule the lock out by itself.
    /// </summary>
    public async Task<MatchLockVerdict> Verify(MatchLock matchLock)
    {
        MatchServer? registeredServer = await distributedCacheStore.GetMatchServerByID(matchLock.ServerID);

        Context.MatchServerChatSessions.TryGetValue(matchLock.ServerID, out MatchServerChatSession? serverSession);

        MatchLockVerdict? serverVerdict = EvaluateServer(matchLock, registeredServer, serverSession?.Metadata, DateTimeOffset.UtcNow);

        if (serverVerdict is not null)
            LogServerVerdict(matchLock, serverVerdict.Value, registeredServer, serverSession?.Metadata);

        MatchLockVerdict verdict = serverVerdict ?? await VerifySlot(matchLock, registeredServer, serverSession);

        if (verdict is not MatchLockVerdict.SendBack)
            await distributedCacheStore.RemoveMatchLock(matchLock.UserID, matchLock.MatchID);

        return verdict;
    }

    /// <summary>
    ///     Rules a match lock out when its match server is gone or no longer hosts the locked match.
    ///     Returns <see langword="null"/> when the match server does not rule the lock out, so that the player's slot decides.
    /// </summary>
    internal static MatchLockVerdict? EvaluateServer(MatchLock matchLock, MatchServer? registeredServer, MatchServerChatSessionMetadata? serverSessionMetadata, DateTimeOffset now)
    {
        if (registeredServer is null)
            return MatchLockVerdict.LiftServerGone;

        // The Stale Host Reaper Has Yet To Decide On A Registered Match Server Without A Chat Session, So The Player's Slot Decides Instead
        if (serverSessionMetadata is null)
            return null;

        if (StaleHostReaper.SessionIsStale(serverSessionMetadata.LastStatusUpdate, now, StaleHostReaper.MatchServerSessionStaleStateThreshold))
            return MatchLockVerdict.LiftServerGone;

        // The Match Server Reports The ID Of A New Match In The Status It Sends Before Announcing That Match, So Another Match ID Ends The Locked Match Whatever The Status
        if (serverSessionMetadata.MatchID > 0 && serverSessionMetadata.MatchID != matchLock.MatchID)
            return MatchLockVerdict.LiftMatchOver;

        return serverSessionMetadata.Status switch
        {
            ChatProtocol.ServerStatus.SERVER_STATUS_IDLE     => MatchLockVerdict.LiftMatchOver,
            ChatProtocol.ServerStatus.SERVER_STATUS_SLEEPING => MatchLockVerdict.LiftMatchOver,
            ChatProtocol.ServerStatus.SERVER_STATUS_CRASHED  => MatchLockVerdict.LiftMatchOver,
            ChatProtocol.ServerStatus.SERVER_STATUS_KILLED   => MatchLockVerdict.LiftMatchOver,
            _                                                => null
        };
    }

    /// <summary>
    ///     Reads the reply to the reconnect query with the game phase which the match server last reported, which is <see langword="null"/> when it is unknown.
    ///     The reply is the time remaining on the player's slot, which is <see langword="null"/> when the match server did not answer.
    /// </summary>
    internal static MatchLockVerdict EvaluateSlot(GamePhase? lastReportedGamePhase, uint? slotTimeRemaining)
    {
        if (slotTimeRemaining is null)
            return MatchLockVerdict.LiftSlotUnconfirmed;

        if (slotTimeRemaining > 0)
            return MatchLockVerdict.SendBack;

        // Pick Phases Only Ever Precede Hero Loading, So From These Phases A Slot Without Time Remaining Can Only Have Been Removed Or Have Outlived Its Match
        // In Any Other Phase, Or An Unknown One, The Match Server May Answer Zero To Everyone, And A Reported Phase May Be Stale By One Status Interval, So The Lock Is Never Lifted During A Pick Phase
        return lastReportedGamePhase switch
        {
            GamePhase.GAME_PHASE_HERO_LOADING => MatchLockVerdict.LiftSlotGone,
            GamePhase.GAME_PHASE_PRE_MATCH    => MatchLockVerdict.LiftSlotGone,
            GamePhase.GAME_PHASE_ACTIVE       => MatchLockVerdict.LiftSlotGone,
            GamePhase.GAME_PHASE_POST_MATCH   => MatchLockVerdict.LiftSlotGone,
            GamePhase.GAME_PHASE_ENDED        => MatchLockVerdict.LiftSlotGone,
            _                                 => MatchLockVerdict.SendBack
        };
    }

    /// <summary>
    ///     Queries the match server for the time remaining on the player's slot, and reads the reply with the game phase which the match server last reported.
    /// </summary>
    private async Task<MatchLockVerdict> VerifySlot(MatchLock matchLock, MatchServer? registeredServer, MatchServerChatSession? serverSession)
    {
        IPEndPoint? gameEndPoint = ResolveGameEndPoint(matchLock, registeredServer, serverSession);

        uint? slotTimeRemaining = gameEndPoint is null ? null : await reconnectQuery.QuerySlotTimeRemaining(gameEndPoint, matchLock.MatchID, matchLock.AccountID);

        // Without A Chat Session, The Match Server Has No Reported Phase Which Is Known To Be Current
        GamePhase? lastReportedGamePhase = serverSession is null ? null : (GamePhase) serverSession.Metadata.GamePhase;

        string gamePhaseName = lastReportedGamePhase?.ToString() ?? "Unknown";

        MatchLockVerdict verdict = EvaluateSlot(lastReportedGamePhase, slotTimeRemaining);

        if (slotTimeRemaining is null)
        {
            if (UnreachableServerIDs.TryAdd(matchLock.ServerID, default))
            {
                logger.LogError(@"Match Server ID ""{MatchServerID}"" Could Not Be Reached With The Reconnect Query At ""{MatchServerAddress}:{MatchServerPort}""",
                    matchLock.ServerID, gameEndPoint?.Address.ToString() ?? registeredServer?.IPAddress, matchLock.ServerPort);
            }

            logger.LogWarning(@"Lifting The Match Lock Of Account ""{AccountName}"" On Match ID ""{MatchID}"" Without Confirmation, Because Match Server ID ""{MatchServerID}"" Did Not Answer The Reconnect Query",
                matchLock.AccountName, matchLock.MatchID, matchLock.ServerID);

            return verdict;
        }

        if (UnreachableServerIDs.TryRemove(matchLock.ServerID, out _))
            logger.LogInformation(@"Match Server ID ""{MatchServerID}"" Answers The Reconnect Query Again", matchLock.ServerID);

        if (verdict is MatchLockVerdict.SendBack)
        {
            logger.LogInformation(@"Keeping The Match Lock Of Account ""{AccountName}"" On Match ID ""{MatchID}"", Because Match Server ID ""{MatchServerID}"" Reports {SlotTimeRemaining} Milliseconds Remaining On The Slot In Game Phase ""{GamePhase}""",
                matchLock.AccountName, matchLock.MatchID, matchLock.ServerID, slotTimeRemaining, gamePhaseName);
        }

        else
        {
            logger.LogInformation(@"Lifting The Match Lock Of Account ""{AccountName}"" On Match ID ""{MatchID}"", Because Match Server ID ""{MatchServerID}"" Reports No Time Remaining On The Slot In Game Phase ""{GamePhase}""",
                matchLock.AccountName, matchLock.MatchID, matchLock.ServerID, gamePhaseName);
        }

        return verdict;
    }

    /// <summary>
    ///     Resolves where the reconnect query goes, at the lock's game port.
    ///     It goes to the address of the match server's chat connection, at which the chat server demonstrably reaches the match server, or, without a chat session, to the registered address, which is also where the players connect.
    ///     The address which the lock holds is never used, so that the query always goes where the match server is registered now.
    /// </summary>
    private static IPEndPoint? ResolveGameEndPoint(MatchLock matchLock, MatchServer? registeredServer, MatchServerChatSession? serverSession)
    {
        if (serverSession?.RemoteEndPoint is IPEndPoint chatEndPoint)
            return new IPEndPoint(chatEndPoint.Address.IsIPv4MappedToIPv6 ? chatEndPoint.Address.MapToIPv4() : chatEndPoint.Address, matchLock.ServerPort);

        return IPAddress.TryParse(registeredServer?.IPAddress, out IPAddress? registeredAddress) ? new IPEndPoint(registeredAddress, matchLock.ServerPort) : null;
    }

    /// <summary>
    ///     Logs why the match server ruled the lock out.
    /// </summary>
    private void LogServerVerdict(MatchLock matchLock, MatchLockVerdict verdict, MatchServer? registeredServer, MatchServerChatSessionMetadata? serverSessionMetadata)
    {
        if (registeredServer is null)
        {
            logger.LogInformation(@"Lifting The Match Lock Of Account ""{AccountName}"" On Match ID ""{MatchID}"", Because Match Server ID ""{MatchServerID}"" Is No Longer Registered",
                matchLock.AccountName, matchLock.MatchID, matchLock.ServerID);
        }

        else if (verdict is MatchLockVerdict.LiftServerGone)
        {
            logger.LogInformation(@"Lifting The Match Lock Of Account ""{AccountName}"" On Match ID ""{MatchID}"", Because Match Server ID ""{MatchServerID}"" Has Not Reported Its Status Since ""{LastStatusUpdate}""",
                matchLock.AccountName, matchLock.MatchID, matchLock.ServerID, serverSessionMetadata?.LastStatusUpdate);
        }

        else
        {
            logger.LogInformation(@"Lifting The Match Lock Of Account ""{AccountName}"" On Match ID ""{MatchID}"", Because Match Server ID ""{MatchServerID}"" Reports Status ""{Status}"" With Match ID ""{ReportedMatchID}""",
                matchLock.AccountName, matchLock.MatchID, matchLock.ServerID, serverSessionMetadata?.Status, serverSessionMetadata?.MatchID);
        }
    }
}
