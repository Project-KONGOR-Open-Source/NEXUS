namespace TRANSMUTANSTEIN.ChatServer.Services;

/// <summary>
///     Locks the players of a competitive matchmade match to it, decides whether a match lock still holds before its player is sent back to the locked match, and lifts the lock when it does not.
///     A lock holds only while its match server is registered and alive, still hosts the locked match, and can still seat the player, so that nobody is ever sent to a dead server, a finished match, or an unreclaimable slot.
/// </summary>
public class MatchLockService(IDatabase distributedCacheStore, MatchServerReconnectQuery reconnectQuery, ILogger<MatchLockService> logger)
{
    /// <summary>
    ///     The sender of the notices which tell a locked player why another game is refused.
    /// </summary>
    internal const string MessageSenderName = "Matchmaking";

    /// <summary>
    ///     The notice to the account which holds the slot in the locked match, which is sent back to the match.
    /// </summary>
    internal const string SlotAccountMessage = "You left a match which is still in progress, so you cannot play another game until it ends. Sending you back to it.";

    /// <summary>
    ///     The notice to another account of the locked user, which is not sent back, because only the account which holds the slot can return to the match.
    /// </summary>
    internal static string OtherAccountMessage(string slotAccountName)
        => $@"Your account ""{slotAccountName}"" is in a match which is still in progress, so you cannot play another game until it ends. Log in as ""{slotAccountName}"" to return to it.";

    /// <summary>
    ///     The notice to the leader of a group which cannot search for a match, because one of its members is locked to a match in progress.
    /// </summary>
    internal static string GroupMemberMessage(string lockedMemberName)
        => $@"""{lockedMemberName}"" is in a match which is still in progress, so your group cannot search for a match until it ends.";

    /// <summary>
    ///     How often a player whose return is pending is reminded to return, which is also how long a verification of the lock is trusted.
    /// </summary>
    internal static readonly TimeSpan ReturnResendInterval = TimeSpan.FromSeconds(5);

    /// <summary>
    ///     How long a player whose return is pending waits after a notice before a refused attempt tells them why again, which is as long as the match server keeps retrying the authentication of a refused connection.
    ///     The match server asks about a refused connection again with every retry, so that one attempt to join another game would otherwise be explained several times.
    /// </summary>
    internal static readonly TimeSpan NoticeRepeatInterval = TimeSpan.FromMilliseconds(MatchServerChatSession.AuthenticationRequestTotalTimeoutMilliseconds);

    /// <summary>
    ///     The match server's default "svr_AccountAuthInterval", which is how often it checks that the accounts of its players have not logged in elsewhere, and which the chat server does not change.
    /// </summary>
    internal static readonly TimeSpan EngineAccountAuthenticationInterval = TimeSpan.FromMinutes(2);

    /// <summary>
    ///     The match server's default "svr_AccountAuthTimeOut", which is how long it waits for that check to be answered, and which the chat server does not change.
    /// </summary>
    internal static readonly TimeSpan EngineAccountAuthenticationTimeout = TimeSpan.FromSeconds(15);

    /// <summary>
    ///     How long a player is reminded to return: as long as the match server keeps retrying the authentication of a refused connection, plus as long as its check of its players' accounts may take to remove the player's connection from another machine.
    /// </summary>
    internal static readonly TimeSpan ReturnWindow = TimeSpan.FromMilliseconds(MatchServerChatSession.AuthenticationRequestTotalTimeoutMilliseconds) + EngineAccountAuthenticationInterval + EngineAccountAuthenticationTimeout;

    /// <summary>
    ///     The IDs of the match servers whose unanswered reconnect query has been logged as an error, so that an unreachable match server is logged once rather than at every verification.
    ///     An ID is removed when its match server next answers, so that the match server is logged again if it becomes unreachable again.
    /// </summary>
    private ConcurrentDictionary<int, byte> UnreachableServerIDs { get; } = [];

    /// <summary>
    ///     Locks every committed player of a competitive matchmade match (Caldavar, MidWars, or RiftWars) to the match which its match server has announced.
    ///     A co-op match locks nobody, and a guest or staff account is never locked.
    /// </summary>
    public async Task LockMatchRoster(MatchmakingMatch match, int matchID, int serverID, string serverAddress, ushort serverPort)
    {
        if (match.QueueType is not (QueueType.Caldavar or QueueType.MidWars or QueueType.RiftWars))
            return;

        DateTimeOffset now = DateTimeOffset.UtcNow;

        int lockedPlayerCount = 0;

        foreach (MatchmakingGroupMember member in match.CommittedRoster)
        {
            if (IsExempt(member.Account))
                continue;

            MatchLock matchLock = new ()
            {
                UserID = member.Account.User.ID,
                AccountID = member.Account.ID,
                AccountName = member.Account.Name,
                MatchID = matchID,
                CorrelationID = match.CorrelationID,
                ArrangedMatchType = match.ArrangedMatchType,
                ServerID = serverID,
                ServerAddress = serverAddress,
                ServerPort = serverPort,
                TimestampStarted = now
            };

            await distributedCacheStore.SetMatchLock(matchLock);

            lockedPlayerCount++;
        }

        logger.LogInformation(@"Locked {LockedPlayerCount} Player(s) To Match ID ""{MatchID}"" On Match Server ID ""{MatchServerID}""", lockedPlayerCount, matchID, serverID);
    }

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
    ///     Decides whether a player's attempt to play another game must be refused, which is only while the player's user is locked to a match and the lock holds.
    ///     A refused player is told why, and the account which holds the slot is sent back to the locked match and reminded to return until it does, unless the game client reports being in a game, in which case it is refused and left undisturbed.
    ///     While a return to the same match is pending, an attempt neither starts another return nor queries the match server again within <see cref="ReturnResendInterval"/> of the last verification, and the player is told why again only once <see cref="NoticeRepeatInterval"/> has passed since the last notice.
    /// </summary>
    public async Task<bool> Enforce(ClientChatSession session)
    {
        if (IsExempt(session.Account))
            return false;

        MatchLockReturn? pendingReturn = session.PendingMatchLockReturn;

        MatchLock? matchLock = await distributedCacheStore.GetMatchLock(session.Account.User.ID);

        if (matchLock is null)
        {
            session.TryReplacePendingMatchLockReturn(pendingReturn, null);

            return false;
        }

        MatchLockReturn? continuedReturn = IsContinuable(pendingReturn, matchLock) ? pendingReturn : null;

        bool verificationIsDue = continuedReturn is null || DateTimeOffset.UtcNow - continuedReturn.LastVerified >= ReturnResendInterval;

        if (verificationIsDue && await Verify(matchLock) is not MatchLockVerdict.SendBack)
        {
            session.TryReplacePendingMatchLockReturn(pendingReturn, null);

            return false;
        }

        // A Game Client Which Reports Being In A Game Is On A Match Server, Where It Cannot Be Sent Back, And Is Usually A Player Still In The Locked Match, Who Must Not Be Told That They Left It
        if (session.ReportedJoiningGame)
        {
            logger.LogDebug(@"Refusing Account ""{AccountName}"" Without Sending It Back To Match ID ""{MatchID}"", Because It Reports Being In A Game",
                session.Account.Name, matchLock.MatchID);

            return true;
        }

        bool isSlotAccount = matchLock.AccountID == session.Account.ID;

        if (continuedReturn is not null)
        {
            ContinueReturn(session, matchLock, isSlotAccount, continuedReturn, verificationIsDue);

            return true;
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;

        MatchLockReturn matchLockReturn = new (matchLock.MatchID, now + ReturnWindow) { LastVerified = now, LastNotified = now };

        if (session.TryReplacePendingMatchLockReturn(pendingReturn, matchLockReturn) is false)
        {
            // A Concurrent Attempt Changed The Pending Return, So It Is Read Again: A Return It Started For This Match Is Continued, And Anything Else Is Replaced Once More
            MatchLockReturn? concurrentReturn = session.PendingMatchLockReturn;

            if (IsContinuable(concurrentReturn, matchLock))
            {
                ContinueReturn(session, matchLock, isSlotAccount, concurrentReturn, verified: true);

                return true;
            }

            // A Refused Attempt Always Tells The Player Why, Even When No Return Could Be Started For It
            if (session.TryReplacePendingMatchLockReturn(concurrentReturn, matchLockReturn) is false)
            {
                logger.LogWarning(@"Could Not Start The Return Of Account ""{AccountName}"" To Match ID ""{MatchID}"", Because Its Pending Return Kept Changing",
                    session.Account.Name, matchLock.MatchID);

                Notify(session, matchLock, isSlotAccount);

                return true;
            }
        }

        Notify(session, matchLock, isSlotAccount);

        // Only The Account Which Holds The Slot Can Return To The Match, So Another Account Of The Same User Is Only Told Which Account That Is
        if (isSlotAccount)
            _ = RemindToReturn(session, matchLock, matchLockReturn);

        return true;
    }

    /// <summary>
    ///     Decides whether a group's attempt to search for a match must be refused, which is while <see cref="Enforce"/> refuses any of its members.
    ///     Every member is enforced, so that every locked member who is not in a game is sent back, and a leader who is not refused is told which member keeps the group from searching.
    /// </summary>
    public async Task<bool> EnforceForGroup(MatchmakingGroup group)
    {
        // The Members Are Read Once, Because The Group Can Change While They Are Enforced
        MatchmakingGroupMember leader = group.Leader;

        List<MatchmakingGroupMember> members = [.. group.Members];

        // The Members Are Enforced Concurrently, Because Each Locked Member May Wait On Its Own Match Server's Answer
        bool[] refusals = await Task.WhenAll(members.Select(member => Enforce(member.Session)));

        List<MatchmakingGroupMember> refusedMembers = [.. members.Where((member, index) => refusals[index])];

        if (refusedMembers.Count is 0)
            return false;

        logger.LogInformation(@"Refusing The Queue Of Matchmaking Group GUID ""{GroupGUID}"", Because {RefusedMemberCount} Member(s) Cannot Play Another Game Until Their Match Ends",
            group.GUID, refusedMembers.Count);

        // A Refused Leader Has Already Been Told Why, Or Is Still In Its Match And Is Left Undisturbed
        if (refusedMembers.Contains(leader) is false)
            leader.Session.SendNotice(MessageSenderName, GroupMemberMessage(refusedMembers[0].Account.Name));

        return true;
    }

    /// <summary>
    ///     Enforces the match lock when a player logs in, once per login, so that the chat reconnects of the same login, which report logging in again, are left alone.
    ///     A session which has reported joining a game, as the game client does when it is already connected to a match server, is left alone too.
    /// </summary>
    public async Task EnforceAtLogin(ClientChatSession session)
    {
        if (IsExempt(session.Account) || session.ReportedJoiningGame)
            return;

        if (await distributedCacheStore.MarkMatchLockLoginChecked(session.Metadata.SessionCookie) is false)
            return;

        await Enforce(session);
    }

    /// <summary>
    ///     Acts on the master server's refusal of a locked user's client connection on a match server other than the locked one, which sends no chat message of its own.
    ///     The refused account is enforced through its chat session when it has one, and otherwise the user's lock is verified anyway, so that a player who was removed from the match while their chat is down is no longer locked when the match server asks again.
    /// </summary>
    public async Task HandleRefusal(MatchLockRefusal refusal)
    {
        if (Context.ClientChatSessions.TryGetValue(refusal.AccountName, out ClientChatSession? session))
        {
            await Enforce(session);

            return;
        }

        MatchLock? matchLock = await distributedCacheStore.GetMatchLock(refusal.UserID);

        if (matchLock is not null)
            await Verify(matchLock);
    }

    /// <summary>
    ///     Ends the pending return when the game client reports joining the locked match server, which it reports only once it is loading there, so that its connection was accepted.
    ///     Joining any other match server leaves the return and its reminders in place.
    /// </summary>
    public async Task EndReturnOnJoiningGame(ClientChatSession session, string serverAddress)
    {
        if (session.PendingMatchLockReturn is not { } pendingReturn)
            return;

        MatchLock? matchLock = await distributedCacheStore.GetMatchLock(session.Account.User.ID);

        if (matchLock is null || matchLock.MatchID != pendingReturn.MatchID || await NamesLockedServer(matchLock, serverAddress) is false)
            return;

        if (session.TryReplacePendingMatchLockReturn(pendingReturn, null))
        {
            logger.LogInformation(@"Ending The Return Of Account ""{AccountName}"" To Match ID ""{MatchID}"", Because It Is Joining Match Server ID ""{MatchServerID}""",
                session.Account.Name, matchLock.MatchID, matchLock.ServerID);
        }
    }

    /// <summary>
    ///     Records that the game client acknowledged an "AUTO_MATCH_CONNECT" while its return is pending, as telemetry only.
    ///     Acting on it is not reaching the match, because the connection to the match server can still be refused, so the return carries on until the game client reports joining the locked match server.
    /// </summary>
    public void RecordReturnAcknowledgement(ClientChatSession session, ChatProtocol.ActionCampaign action)
    {
        if (session.PendingMatchLockReturn is { } pendingReturn)
        {
            logger.LogDebug(@"Account ""{AccountName}"" Acknowledged Its Return To Match ID ""{MatchID}"" With Action ""{Action}""",
                session.Account.Name, pendingReturn.MatchID, action);
        }
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

        // Without A Chat Session, The Match Server Has No Reported Phase Which Is Known To Be Current
        // The Phase Is Read Before The Query, Because A Status Reported During The Query's Retries Could Pair A Zero Given During A Pick Phase With A Later Phase And Lift The Lock During That Pick Phase
        GamePhase? lastReportedGamePhase = serverSession is null ? null : (GamePhase) serverSession.Metadata.GamePhase;

        uint? slotTimeRemaining = gameEndPoint is null ? null : await reconnectQuery.QuerySlotTimeRemaining(gameEndPoint, matchLock.MatchID, matchLock.AccountID);

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
            logger.LogDebug(@"Keeping The Match Lock Of Account ""{AccountName}"" On Match ID ""{MatchID}"", Because Match Server ID ""{MatchServerID}"" Reports {SlotTimeRemaining} Milliseconds Remaining On The Slot In Game Phase ""{GamePhase}""",
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

    /// <summary>
    ///     Decides whether an account is exempt from the match lock, by the rule which <see cref="MatchLock.IsExempt"/> defines for the master server and the chat server alike.
    /// </summary>
    private static bool IsExempt(Account account) => MatchLock.IsExempt(account.Type);

    /// <summary>
    ///     Decides whether a pending return can be continued for the lock, which is the case while it is unexpired and for the lock's match.
    /// </summary>
    private static bool IsContinuable([NotNullWhen(true)] MatchLockReturn? pendingReturn, MatchLock matchLock)
        => pendingReturn is not null && pendingReturn.MatchID == matchLock.MatchID && pendingReturn.Expires > DateTimeOffset.UtcNow;

    /// <summary>
    ///     Continues a pending return for a further refused attempt, recording the verification when the attempt verified the lock, and telling the player why again only once <see cref="NoticeRepeatInterval"/> has passed since the last notice.
    /// </summary>
    private void ContinueReturn(ClientChatSession session, MatchLock matchLock, bool isSlotAccount, MatchLockReturn pendingReturn, bool verified)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        if (verified)
            pendingReturn.LastVerified = now;

        if (now - pendingReturn.LastNotified < NoticeRepeatInterval)
            return;

        pendingReturn.LastNotified = now;

        Notify(session, matchLock, isSlotAccount);
    }

    /// <summary>
    ///     Tells a refused player why, naming the account which holds the slot when it is another account of the same user.
    /// </summary>
    private void Notify(ClientChatSession session, MatchLock matchLock, bool isSlotAccount)
    {
        if (isSlotAccount)
        {
            session.SendNotice(MessageSenderName, SlotAccountMessage);

            logger.LogInformation(@"Sending Account ""{AccountName}"" Back To Match ID ""{MatchID}"" On Match Server ID ""{MatchServerID}"", Which Is Still In Progress",
                session.Account.Name, matchLock.MatchID, matchLock.ServerID);
        }

        else
        {
            session.SendNotice(MessageSenderName, OtherAccountMessage(matchLock.AccountName));

            logger.LogInformation(@"Telling Account ""{AccountName}"" That Its User Is Locked To Match ID ""{MatchID}"" On Match Server ID ""{MatchServerID}"" As Account ""{SlotAccountName}""",
                session.Account.Name, matchLock.MatchID, matchLock.ServerID, matchLock.AccountName);
        }
    }

    /// <summary>
    ///     Sends the account which holds the slot back to the locked match at once, and reminds it every <see cref="ReturnResendInterval"/> until the return ends.
    ///     No chat message reaches the chat server when the match server refuses the connection, and the game client ignores a reminder while it is still connecting, so the reminders carry on until the game client reports joining the locked match server, the lock no longer holds, the session ends, or the return window expires.
    ///     It runs without being awaited, so it handles its own exceptions.
    /// </summary>
    private async Task RemindToReturn(ClientChatSession session, MatchLock matchLock, MatchLockReturn matchLockReturn)
    {
        CancellationToken cancellationToken = matchLockReturn.Cancellation.Token;

        try
        {
            session.SendAutoMatchConnect(matchLock.ArrangedMatchType, matchLock.MatchID, matchLock.ServerAddress, matchLock.ServerPort, reminder: false);

            while (true)
            {
                await Task.Delay(ReturnResendInterval, cancellationToken);

                if (session.IsConnected is false)
                    return;

                if (DateTimeOffset.UtcNow >= matchLockReturn.Expires)
                {
                    logger.LogInformation(@"Ending The Return Of Account ""{AccountName}"" To Match ID ""{MatchID}"", Because It Did Not Join Match Server ID ""{MatchServerID}"" Within The Return Window",
                        session.Account.Name, matchLock.MatchID, matchLock.ServerID);

                    return;
                }

                MatchLock? currentLock = await distributedCacheStore.GetMatchLock(matchLock.UserID);

                if (currentLock is null || currentLock.MatchID != matchLock.MatchID)
                    return;

                if (await Verify(currentLock) is not MatchLockVerdict.SendBack)
                    return;

                // The Return May Have Ended While The Match Server Was Being Queried
                if (cancellationToken.IsCancellationRequested)
                    return;

                matchLockReturn.LastVerified = DateTimeOffset.UtcNow;

                session.SendAutoMatchConnect(currentLock.ArrangedMatchType, currentLock.MatchID, currentLock.ServerAddress, currentLock.ServerPort, reminder: true);

                logger.LogDebug(@"Reminded Account ""{AccountName}"" To Return To Match ID ""{MatchID}"" On Match Server ID ""{MatchServerID}""",
                    session.Account.Name, currentLock.MatchID, currentLock.ServerID);
            }
        }

        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }

        catch (Exception exception)
        {
            logger.LogError(exception, @"The Return Of Account ""{AccountName}"" To Match ID ""{MatchID}"" Failed", session.Account.Name, matchLock.MatchID);
        }

        finally
        {
            // A Return Which Ends Here Is Cleared, So That The Next Attempt Starts A New One, While A Return Which Was Replaced Or Ended Elsewhere Is Left As It Is
            session.TryReplacePendingMatchLockReturn(matchLockReturn, null);
        }
    }

    /// <summary>
    ///     Decides whether the address which the game client reports joining names the locked match server, either exactly as the client was sent there, or as the match server is registered.
    /// </summary>
    private async Task<bool> NamesLockedServer(MatchLock matchLock, string serverAddress)
    {
        if (serverAddress.Equals($"{matchLock.ServerAddress}:{matchLock.ServerPort}", StringComparison.Ordinal))
            return true;

        string[] serverAddressParts = serverAddress.Split(':');

        if (serverAddressParts.Length is not 2 || int.TryParse(serverAddressParts[1], out int serverPort) is false)
            return false;

        MatchServer? server = await distributedCacheStore.GetMatchServerByIPAddressAndPort(serverAddressParts[0], serverPort);

        return server?.ID == matchLock.ServerID;
    }
}
