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
    internal const string MessageSenderName = "Match Lock";

    /// <summary>
    ///     The notice to the account which holds the slot in the locked match, which is sent back to the match.
    /// </summary>
    internal const string SlotAccountMessage = "You left a match which is still in progress, and a match lock keeps you from playing another game until it ends. Sending you back to it.";

    /// <summary>
    ///     The notice to the account which holds the slot in the locked match when it logs in, which is not sent back, because the game client's own reconnect prompt offers the return at login.
    /// </summary>
    internal const string SlotAccountLoginMessage = "You are in a match which is still in progress, and a match lock keeps you from playing another game until it ends. Trying to play another game sends you back to it.";

    /// <summary>
    ///     The notice to the account which holds the slot in the locked match while its slot is still connected to the match, which is sent back only once that connection has left.
    ///     The connection is most often that of a game which has just crashed or closed on the same machine, and which the match server has yet to drop, rather than a game still running on another machine, so the notice covers both.
    /// </summary>
    internal const string ConnectedElsewhereMessage = "Your account is still connected to your match from a game which is running or has just closed. Close it if it is still open, and you will be sent back to the match as soon as it has left.";

    /// <summary>
    ///     The notice to the account which holds the slot in the locked match when its wait for the slot's other connection to leave runs out, after which it is no longer sent back until it next tries to play.
    /// </summary>
    internal const string ConnectedElsewhereWaitExpiredMessage = "Your other game has not left the match yet. Try again once it has closed, and you will be sent back.";

    /// <summary>
    ///     The notice to a player whose refused attempt to play found that the lock has lifted, which nothing repeats, so the player has to try again.
    /// </summary>
    internal const string RefusalLiftedMessage = "Your match lock has lifted, because you can no longer return to your last match. Please try again.";

    /// <summary>
    ///     The reply to the reconnect query above which the slot is still connected to the match.
    ///     The match server answers the time left before the slot is terminated, which is the largest possible time less the game time while the slot is connected, and only the remaining disconnect allowance once it has disconnected, so the middle of the range tells the two apart.
    /// </summary>
    internal const uint ConnectedSlotThreshold = uint.MaxValue / 2;

    /// <summary>
    ///     The notice to another account of the locked user, which is not sent back, because only the account which holds the slot can return to the match.
    /// </summary>
    internal static string OtherAccountMessage(string slotAccountNameWithClanTag)
        => $@"Your account ""{slotAccountNameWithClanTag}"" is in a match which is still in progress, and a match lock keeps you from playing another game until it ends. Log in on that account to return to it.";

    /// <summary>
    ///     The notice to every member of a party which cannot search for a match, once for each of its members which holds a match lock.
    /// </summary>
    internal static string GroupMemberLockedMessage(string lockedMemberNameWithClanTag)
        => $@"""{lockedMemberNameWithClanTag}"" has a match lock, because their match is still in progress, so your group cannot search for a match until it ends.";

    /// <summary>
    ///     The notice to the inviter of a player who cannot join a group, because the player holds a match lock.
    /// </summary>
    internal static string InviteeLockedMessage(string inviteeNameWithClanTag)
        => $@"""{inviteeNameWithClanTag}"" is under a match lock until their match in progress ends, so they cannot join your group.";

    /// <summary>
    ///     How often a player whose return is pending is reminded to return, which is also how long a verification of the lock is trusted.
    /// </summary>
    internal static readonly TimeSpan ReturnResendInterval = TimeSpan.FromSeconds(5);

    /// <summary>
    ///     How long a player whose return is pending waits after a notice before a refused attempt tells them why again.
    ///     A player often repeats an attempt within seconds, by clicking Play again, or by asking for the server list and then joining a game, so that one wish to play another game would otherwise be explained several times.
    ///     It lasts as long as the match server retries the authentication of a refused connection, because during a staggered deployment a master server of the previous version can still answer, whose refusal the match server retries, and which relays every retry as another refused attempt.
    /// </summary>
    internal static readonly TimeSpan NoticeRepeatInterval = TimeSpan.FromMilliseconds(MatchServerChatSession.AuthenticationRequestTotalTimeoutMilliseconds);

    /// <summary>
    ///     The game client's default "cl_connectionTimeout", which is how long it keeps trying to connect to a match server before it gives up, and which the chat server cannot change, because it is a setting of the game client.
    /// </summary>
    internal static readonly TimeSpan EngineClientConnectionTimeout = TimeSpan.FromSeconds(15);

    /// <summary>
    ///     How long a player who is sent back is reminded to return: as long as the match server retries the authentication of a refused connection, plus as long as the game client keeps trying to connect, plus one resend interval, so that a reminder still follows the game client's last attempt.
    ///     The master server's own refusal is final and never retried, but during a staggered deployment a master server of the previous version can still answer, whose refusal the match server retries while the game client keeps connecting and ignores every reminder.
    ///     A window longer than a return needs costs nothing, because the reminders stop as soon as the game client reports joining the locked match server.
    /// </summary>
    internal static readonly TimeSpan ReturnWindow = TimeSpan.FromMilliseconds(MatchServerChatSession.AuthenticationRequestTotalTimeoutMilliseconds) + EngineClientConnectionTimeout + ReturnResendInterval;

    /// <summary>
    ///     How long a player whose slot account is still connected to the locked match from another machine waits for that connection to leave, after which the return ends with <see cref="ConnectedElsewhereWaitExpiredMessage"/>, and the player's next attempt starts afresh.
    /// </summary>
    internal static readonly TimeSpan ConnectedElsewhereWait = TimeSpan.FromMinutes(5);

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
                AccountNameWithClanTag = member.Account.NameWithClanTag,
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
    ///     Verifies a match lock against its match server, and lifts the lock unless <see cref="KeepsLock"/> accepts the verdict.
    ///     The player's slot is queried only when the match server does not rule the lock out by itself.
    /// </summary>
    /// <param name="matchLock">The match lock to verify.</param>
    /// <param name="unconfirmedSlotLifts">Whether an unanswered reconnect query lifts the lock, which it does unless the caller is a return which waits for another machine and has sent nobody anywhere yet, or an invite to the player from someone else, which is not the player's own attempt to play; a match server which is gone, or has ended the match, lifts the lock either way.</param>
    public async Task<MatchLockVerdict> Verify(MatchLock matchLock, bool unconfirmedSlotLifts = true)
    {
        MatchServer? registeredServer = await distributedCacheStore.GetMatchServerByID(matchLock.ServerID);

        Context.MatchServerChatSessions.TryGetValue(matchLock.ServerID, out MatchServerChatSession? serverSession);

        MatchLockVerdict? serverVerdict = EvaluateServer(matchLock, registeredServer, serverSession?.Metadata, DateTimeOffset.UtcNow);

        if (serverVerdict is not null)
            LogServerVerdict(matchLock, serverVerdict.Value, registeredServer, serverSession?.Metadata);

        MatchLockVerdict verdict = serverVerdict ?? await VerifySlot(matchLock, registeredServer, serverSession, unconfirmedSlotLifts);

        bool lifts = KeepsLock(verdict) is false && (verdict is not MatchLockVerdict.LiftSlotUnconfirmed || unconfirmedSlotLifts);

        if (lifts)
            await distributedCacheStore.RemoveMatchLock(matchLock.UserID, matchLock.MatchID);

        return verdict;
    }

    /// <summary>
    ///     Decides whether a player's attempt to play another game must be refused, which is only while the player's user is locked to a match and the lock holds.
    ///     A refused player is told why, and the account which holds the slot is sent back to the locked match and reminded to return until it does, unless the game client reports being in a game, in which case it is refused and left undisturbed.
    ///     A relayed refusal, of the server list or of a client connection on another match server, proves that the game client is trying to play elsewhere, so it is always sent back, whatever the game client reports.
    ///     While the slot account is still connected to the match from another machine, it is told to close the game there instead, and is sent back only once that connection has left.
    ///     A refused queue or group join which does not send the player back at once is answered with the game client's "Temporary Ban" popup before the notice, and the player leaves its group, because the game client's matchmaking screen covers the chat in which the notice appears.
    ///     While a return to the same match is pending, an attempt neither starts another return nor queries the match server again within <see cref="ReturnResendInterval"/> of the last verification, and the player is told why again only once <see cref="NoticeRepeatInterval"/> has passed since the last notice.
    /// </summary>
    /// <param name="session">The chat session of the player whose attempt to play another game is decided.</param>
    /// <param name="relayedRefusal">Whether the attempt is a refusal of the server list, or of a client connection on another match server, which was relayed to the chat server, which proves that the game client is trying to play elsewhere, so that the player is sent back even when the game client reports being in a game.</param>
    /// <param name="matchmaking">Whether the attempt is a queue or a group join, whose refusal must also release the game client's matchmaking screen.</param>
    public async Task<bool> Enforce(ClientChatSession session, bool relayedRefusal = false, bool matchmaking = false)
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

        // A Pending Return Which Was Verified Within The Resend Interval Is Trusted With The Verdict Which It Recorded
        MatchLockVerdict verdict = verificationIsDue ? await Verify(matchLock) : (continuedReturn?.SlotConnected is true ? MatchLockVerdict.WaitForSlot : MatchLockVerdict.SendBack);

        if (KeepsLock(verdict) is false)
        {
            session.TryReplacePendingMatchLockReturn(pendingReturn, null);

            return false;
        }

        // A Game Client Which Reports Being In A Game Is On A Match Server, Where It Cannot Be Sent Back, And Is Usually A Player Still In The Locked Match, Who Must Not Be Told That They Left It
        // A Relayed Refusal Shows That The Client Is Trying To Play Elsewhere, So It Is Not A Player In Its Locked Match, And For A Refused Client Connection A Chat Reconnect Reports Joining A Game While The Client Is Still Connecting
        if (session.ReportedJoiningGame && relayedRefusal is false)
        {
            logger.LogDebug(@"Refusing Account ""{AccountName}"" Without Sending It Back To Match ID ""{MatchID}"", Because It Reports Being In A Game",
                session.Account.NameWithClanTag, matchLock.MatchID);

            return true;
        }

        bool isSlotAccount = matchLock.AccountID == session.Account.ID;

        // Only A Slot Account Which This Attempt Sends Back At Once Releases Its Matchmaking Screen By Itself, So Every Other Refused Queue Or Group Join Gets The "Temporary Ban" Popup Before Its Notice, Even When The Notice Repeat Interval Holds The Notice Back
        if (continuedReturn is not null)
        {
            if (matchmaking)
                RefuseMatchmaking(session, matchLock);

            ContinueReturn(session, matchLock, isSlotAccount, continuedReturn, verdict, verificationIsDue);

            return true;
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;

        bool slotConnected = verdict is MatchLockVerdict.WaitForSlot;

        // Closing The Game On Another Machine Takes The Player Longer Than Acting On A Send-Back, So A Return Which Waits For It Lasts Longer
        MatchLockReturn matchLockReturn = new (matchLock.MatchID, now + (slotConnected ? ConnectedElsewhereWait : ReturnWindow)) { LastVerified = now, LastNotified = now, SlotConnected = slotConnected };

        if (session.TryReplacePendingMatchLockReturn(pendingReturn, matchLockReturn) is false)
        {
            // A Concurrent Attempt Changed The Pending Return, So It Is Read Again: A Return It Started For This Match Is Continued, And Anything Else Is Replaced Once More
            MatchLockReturn? concurrentReturn = session.PendingMatchLockReturn;

            if (IsContinuable(concurrentReturn, matchLock))
            {
                if (matchmaking)
                    RefuseMatchmaking(session, matchLock);

                ContinueReturn(session, matchLock, isSlotAccount, concurrentReturn, verdict, verified: true);

                return true;
            }

            // A Refused Attempt Always Tells The Player Why, Even When No Return Could Be Started For It
            if (session.TryReplacePendingMatchLockReturn(concurrentReturn, matchLockReturn) is false)
            {
                logger.LogWarning(@"Could Not Start The Return Of Account ""{AccountName}"" To Match ID ""{MatchID}"", Because Its Pending Return Kept Changing",
                    session.Account.NameWithClanTag, matchLock.MatchID);

                if (matchmaking)
                    RefuseMatchmaking(session, matchLock);

                Notify(session, matchLock, isSlotAccount, verdict);

                return true;
            }
        }

        if (matchmaking && (isSlotAccount is false || slotConnected))
            RefuseMatchmaking(session, matchLock);

        Notify(session, matchLock, isSlotAccount, verdict);

        // Only The Account Which Holds The Slot Can Return To The Match, So Another Account Of The Same User Is Only Told Which Account That Is
        if (isSlotAccount)
            _ = RemindToReturn(session, matchLock, matchLockReturn);

        return true;
    }

    /// <summary>
    ///     Decides whether the session's user holds a match lock which still holds, without telling the player anything or sending the player anywhere.
    ///     A lock which no longer holds is lifted, but an unanswered reconnect query gives <see langword="false"/> and keeps the lock, because the check is someone else's invite rather than the player's own attempt to play.
    ///     So an invite never frees a player who may still be in the match, or whose slot account waits for its other machine, and the player's own group join decides instead.
    /// </summary>
    /// <param name="session">The chat session of the player whose lock is read.</param>
    public async Task<bool> HoldsLock(ClientChatSession session)
    {
        if (IsExempt(session.Account))
            return false;

        MatchLock? matchLock = await distributedCacheStore.GetMatchLock(session.Account.User.ID);

        return matchLock is not null && KeepsLock(await Verify(matchLock, unconfirmedSlotLifts: false));
    }

    /// <summary>
    ///     Decides whether a group's attempt to search for a match must be refused, which is while <see cref="Enforce"/> refuses any of its members.
    ///     Every member is enforced as a matchmaking attempt, so that every locked member who is not in a game is sent back, or shown why when it cannot be sent back at once.
    ///     A refused party's remaining members are unreadied, and every member which the party had when the queue started is told which members hold a match lock.
    /// </summary>
    /// <param name="group">The group which is about to search for a match.</param>
    public async Task<bool> EnforceForGroup(MatchmakingGroup group)
    {
        // The Members Are Read Once, Because The Group Can Change While They Are Enforced, Not Least When A Refused Member Leaves It
        List<MatchmakingGroupMember> members = [.. group.Members];

        // Every Member Is Enforced, Because A Premade Party Outlives The Match It Played Together, So A Locked Player Can Still Queue With Its Teammates
        // The Members Are Enforced Concurrently, Because Each Locked Member May Wait On Its Own Match Server's Answer
        bool[] refusals = await Task.WhenAll(members.Select(member => Enforce(member.Session, matchmaking: true)));

        List<MatchmakingGroupMember> refusedMembers = [.. members.Where((member, index) => refusals[index])];

        if (refusedMembers.Count is 0)
            return false;

        logger.LogInformation(@"Refusing The Queue Of Matchmaking Group GUID ""{GroupGUID}"", Because {RefusedMemberCount} Member(s) Cannot Play Another Game Until Their Match Ends",
            group.GUID, refusedMembers.Count);

        // A Solo Group Gets Nothing More, Because A Group Update Would Put Its Game Client Back Into It. The Refusal's Reply Has Already Removed It Without A Group Update, Or The Game Client Deletes It By Itself When It Acts On The Send-Back, Which Removes It Then
        if (members.Count is 1)
            return true;

        // A Member Whose Send-Back Found It Alone In The Group Gets No Group Update, Because Its Game Client Deletes The Group When It Acts On The Send-Back, And An Update Would Put The Game Client Back Into A Group Which The Acknowledgement Then Removes
        List<MatchmakingGroupMember> recipients = [.. group.Members.Where(member => member.Session.PendingMatchLockReturn?.SoloGroupAtLastSend != group)];

        // A Sent-Back Member's Game Client Keeps Its Party, And A Member In A Game Gets No Frame, So Only A Group Update Which Unreadies The Party Releases The Matchmaking Screens Of The Members Which Remain
        if (recipients.Count > 0)
        {
            group.UnloadAndUnreadyMembers();

            group.MulticastUpdate(group.Leader.Account.ID, ChatProtocol.TMMUpdateType.TMM_FULL_GROUP_UPDATE, isRecipient: member => recipients.Contains(member));
        }

        // Every Member Which The Party Had When The Queue Started, The Leader And The Refused Members Included, Is Told About Each Refused Member, So That Nobody Blames The Leader For The Queue Not Starting
        foreach (MatchmakingGroupMember refusedMember in refusedMembers)
        {
            string message = GroupMemberLockedMessage(refusedMember.Account.NameWithClanTag);

            foreach (MatchmakingGroupMember member in members)
                member.Session.SendNotice(MessageSenderName, message);
        }

        return true;
    }

    /// <summary>
    ///     Tells a locked player about the match lock when they log in, once per login, so that the chat reconnects of the same login, which report logging in again, are left alone.
    ///     A session which has reported a game, as the game client does when it is already connected to a match server, is left alone too.
    ///     It never sends the player back, because the game client raises its own reconnect prompt at login, which a send-back at the same moment would leave open and clickable beneath the match.
    /// </summary>
    public async Task NotifyAtLogin(ClientChatSession session)
    {
        if (IsExempt(session.Account) || session.ReportedJoiningGame)
            return;

        if (await distributedCacheStore.MarkMatchLockLoginChecked(session.Metadata.SessionCookie) is false)
            return;

        MatchLock? matchLock = await distributedCacheStore.GetMatchLock(session.Account.User.ID);

        if (matchLock is null)
            return;

        if (KeepsLock(await Verify(matchLock)) is false)
            return;

        if (matchLock.AccountID == session.Account.ID)
        {
            session.SendNotice(MessageSenderName, SlotAccountLoginMessage);

            logger.LogInformation(@"Telling Account ""{AccountName}"" At Login That It Is Locked To Match ID ""{MatchID}"" On Match Server ID ""{MatchServerID}""",
                session.Account.NameWithClanTag, matchLock.MatchID, matchLock.ServerID);
        }

        else
        {
            session.SendNotice(MessageSenderName, OtherAccountMessage(matchLock.TaggedAccountName));

            logger.LogInformation(@"Telling Account ""{AccountName}"" At Login That Its User Is Locked To Match ID ""{MatchID}"" On Match Server ID ""{MatchServerID}"" As Account ""{SlotAccountName}""",
                session.Account.NameWithClanTag, matchLock.MatchID, matchLock.ServerID, matchLock.TaggedAccountName);
        }
    }

    /// <summary>
    ///     Acts on the master server's refusal of a locked user's server list, or of its client connection on a match server other than the locked one, which sends no chat message of its own.
    ///     The refused account is enforced through its chat session when it has one, and otherwise the user's lock is verified anyway, so that a player who was removed from the match while their chat is down is no longer locked when they next try to play.
    /// </summary>
    public async Task HandleRefusal(MatchLockRefusal refusal)
    {
        if (Context.ClientChatSessions.TryGetValue(refusal.AccountName, out ClientChatSession? session))
        {
            // A Client Which Reports Being In Its Own Match Cannot Be Connecting Elsewhere, So The Refusal Is The Late Answer To An Attempt Which It Has Abandoned, Whereas A Client Which Reports Another Match Server May Be In A Chat Reconnect During A Refused Client Connection, And A Refused Server List Shows Only That The Client Is Trying To Play Elsewhere
            MatchLock? lockedMatch = await distributedCacheStore.GetMatchLock(refusal.UserID);

            if (lockedMatch is not null && session.ReportedGameServerAddress is { } reportedAddress && await NamesLockedServer(lockedMatch, reportedAddress))
            {
                logger.LogDebug(@"Ignoring A Stale Refusal Of Account ""{AccountName}"", Because It Reports Being In Match ID ""{MatchID}""",
                    session.Account.NameWithClanTag, lockedMatch.MatchID);

                return;
            }

            // Nothing Repeats A Refused Server List Or A Finally Rejected Join, So A Refusal Which Finds The Lock Lifted Tells The Player To Try Again
            if (await Enforce(session, relayedRefusal: true) is false)
            {
                session.SendNotice(MessageSenderName, RefusalLiftedMessage);

                logger.LogInformation(@"Telling Account ""{AccountName}"" To Try Again, Because Its Refused Attempt Found Its Match Lock Lifted", session.Account.NameWithClanTag);
            }

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
                session.Account.NameWithClanTag, matchLock.MatchID, matchLock.ServerID);
        }
    }

    /// <summary>
    ///     Acts on the game client's acknowledgement of an "AUTO_MATCH_CONNECT" while its return is pending, which the game client sends only when it acts on one, immediately before it connects to the match ("CChatManager::HandleAutoMatchConnect"), and never while it is still connecting, authenticating, or loading, when it ignores the send.
    ///     A game client which acts on a send deletes its matchmaking group by itself when it saw no other member in it, so the player leaves the group which <see cref="SendToMatch"/> found solo, without a group update, which would put the game client back into it.
    ///     Acting on it is not reaching the match, because the connection to the match server can still be refused, so the return carries on until the game client reports joining the locked match server.
    /// </summary>
    public void HandleReturnAcknowledgement(ClientChatSession session, ChatProtocol.ActionCampaign action)
    {
        if (session.PendingMatchLockReturn is { } pendingReturn)
        {
            logger.LogDebug(@"Account ""{AccountName}"" Acknowledged Its Return To Match ID ""{MatchID}"" With Action ""{Action}""",
                session.Account.NameWithClanTag, pendingReturn.MatchID, action);

            // A Join Which Falls Between The Send And The Acknowledgement Keeps The Group, And Its Update Puts The Game Client Back Into The Group With The New Member
            // Any Other Group Update Which Reaches A Player Still Alone In The Group In That Window Would Leave Its Game Client In A Group Which Is Removed Here, Which Is Accepted, Because A Refused Queue, The Only Path Which Sends One There In The Normal Course, Sends None To Such A Player
            pendingReturn.SoloGroupAtLastSend?.RemoveSoleMember(session.Account.ID);
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
    ///     The reply is the time remaining on the player's slot, which is <see langword="null"/> when the match server did not answer, and above <see cref="ConnectedSlotThreshold"/> while the slot is still connected.
    /// </summary>
    internal static MatchLockVerdict EvaluateSlot(GamePhase? lastReportedGamePhase, uint? slotTimeRemaining)
    {
        if (slotTimeRemaining is null)
            return MatchLockVerdict.LiftSlotUnconfirmed;

        // The Match Server Ignores Another Connection Of An Account Which Is Still Connected, So The Player Waits For That Connection To Leave, Whatever The Phase
        if (slotTimeRemaining > ConnectedSlotThreshold)
            return MatchLockVerdict.WaitForSlot;

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
    ///     Decides whether a verdict keeps the match lock, which is the case when the player is sent back, and when the player waits for the slot account's connection to leave the match.
    /// </summary>
    internal static bool KeepsLock(MatchLockVerdict verdict) => verdict is MatchLockVerdict.SendBack or MatchLockVerdict.WaitForSlot;

    /// <summary>
    ///     The whole seconds left until the match lock's safety limit, which the game client's "Temporary Ban" popup counts down, and zero once the limit has passed.
    ///     The seconds are rounded up, so that a lock with a fraction of a second left still shows one.
    /// </summary>
    internal static uint BanSecondsRemaining(MatchLock matchLock, DateTimeOffset now)
    {
        TimeSpan timeRemaining = matchLock.TimestampExpires - now;

        return timeRemaining > TimeSpan.Zero ? (uint) Math.Ceiling(timeRemaining.TotalSeconds) : 0;
    }

    /// <summary>
    ///     Queries the match server for the time remaining on the player's slot, and reads the reply with the game phase which the match server last reported.
    ///     An unanswered query is logged as lifting the lock only when <paramref name="unconfirmedSlotLifts"/> holds, because a waiting return, and an invite to the player, keep the lock through it.
    /// </summary>
    private async Task<MatchLockVerdict> VerifySlot(MatchLock matchLock, MatchServer? registeredServer, MatchServerChatSession? serverSession, bool unconfirmedSlotLifts)
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

            if (unconfirmedSlotLifts)
            {
                logger.LogWarning(@"Lifting The Match Lock Of Account ""{AccountName}"" On Match ID ""{MatchID}"" Without Confirmation, Because Match Server ID ""{MatchServerID}"" Did Not Answer The Reconnect Query",
                    matchLock.TaggedAccountName, matchLock.MatchID, matchLock.ServerID);
            }

            else
            {
                logger.LogDebug(@"Keeping The Match Lock Of Account ""{AccountName}"" On Match ID ""{MatchID}"", Although Match Server ID ""{MatchServerID}"" Did Not Answer The Reconnect Query, Because This Check Sends Nobody There",
                    matchLock.TaggedAccountName, matchLock.MatchID, matchLock.ServerID);
            }

            return verdict;
        }

        if (UnreachableServerIDs.TryRemove(matchLock.ServerID, out _))
            logger.LogInformation(@"Match Server ID ""{MatchServerID}"" Answers The Reconnect Query Again", matchLock.ServerID);

        if (verdict is MatchLockVerdict.WaitForSlot)
        {
            logger.LogDebug(@"Keeping The Match Lock Of Account ""{AccountName}"" On Match ID ""{MatchID}"", Because Match Server ID ""{MatchServerID}"" Reports That The Slot Is Still Connected In Game Phase ""{GamePhase}""",
                matchLock.TaggedAccountName, matchLock.MatchID, matchLock.ServerID, gamePhaseName);
        }

        else if (verdict is MatchLockVerdict.SendBack)
        {
            logger.LogDebug(@"Keeping The Match Lock Of Account ""{AccountName}"" On Match ID ""{MatchID}"", Because Match Server ID ""{MatchServerID}"" Reports {SlotTimeRemaining} Milliseconds Remaining On The Slot In Game Phase ""{GamePhase}""",
                matchLock.TaggedAccountName, matchLock.MatchID, matchLock.ServerID, slotTimeRemaining, gamePhaseName);
        }

        else
        {
            logger.LogInformation(@"Lifting The Match Lock Of Account ""{AccountName}"" On Match ID ""{MatchID}"", Because Match Server ID ""{MatchServerID}"" Reports No Time Remaining On The Slot In Game Phase ""{GamePhase}""",
                matchLock.TaggedAccountName, matchLock.MatchID, matchLock.ServerID, gamePhaseName);
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
                matchLock.TaggedAccountName, matchLock.MatchID, matchLock.ServerID);
        }

        else if (verdict is MatchLockVerdict.LiftServerGone)
        {
            logger.LogInformation(@"Lifting The Match Lock Of Account ""{AccountName}"" On Match ID ""{MatchID}"", Because Match Server ID ""{MatchServerID}"" Has Not Reported Its Status Since ""{LastStatusUpdate}""",
                matchLock.TaggedAccountName, matchLock.MatchID, matchLock.ServerID, serverSessionMetadata?.LastStatusUpdate);
        }

        else
        {
            logger.LogInformation(@"Lifting The Match Lock Of Account ""{AccountName}"" On Match ID ""{MatchID}"", Because Match Server ID ""{MatchServerID}"" Reports Status ""{Status}"" With Match ID ""{ReportedMatchID}""",
                matchLock.TaggedAccountName, matchLock.MatchID, matchLock.ServerID, serverSessionMetadata?.Status, serverSessionMetadata?.MatchID);
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
    ///     Continues a pending return for a further refused attempt, recording the verification and its verdict when the attempt verified the lock, and telling the player why again only once <see cref="NoticeRepeatInterval"/> has passed since the last notice.
    /// </summary>
    private void ContinueReturn(ClientChatSession session, MatchLock matchLock, bool isSlotAccount, MatchLockReturn pendingReturn, MatchLockVerdict verdict, bool verified)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        if (verified)
        {
            pendingReturn.LastVerified = now;
            pendingReturn.SlotConnected = verdict is MatchLockVerdict.WaitForSlot;
        }

        if (now - pendingReturn.LastNotified < NoticeRepeatInterval)
            return;

        pendingReturn.LastNotified = now;

        Notify(session, matchLock, isSlotAccount, verdict);
    }

    /// <summary>
    ///     Answers a refused queue or group join with the game client's "Temporary Ban" popup, counting down to the match lock's safety limit, and removes the player from its group, before the player is told why.
    ///     The game client's matchmaking screen covers the chat in which the notice appears, and the "Temporary Ban" popup is the only refusal of the game client whose text fits a match lock.
    ///     The game client leaves its group on that reply without telling the chat server, so the player is removed without a group update, which would make the game client leave its group again.
    ///     A slot account which is sent back at once needs none of this, because the game client leaves a solo group by itself when it acts on a send to a match, with the reason "foundmatch" ("CChatManager::MatchmakingConnect"), which <see cref="HandleReturnAcknowledgement"/> mirrors.
    /// </summary>
    private static void RefuseMatchmaking(ClientChatSession session, MatchLock matchLock)
    {
        // The Player Is Removed Before The Popup Is Sent, Because Another Refused Member's Removal Updates Every Member Still In The Group, And Such An Update After The Popup Would Put The Game Client Back Into The Group
        MatchmakingService.GetMatchmakingGroup(session.Account.ID)?.RemoveMember(session.Account.ID, notifyRemovedMember: false);

        session.SendMatchmakingBan(BanSecondsRemaining(matchLock, DateTimeOffset.UtcNow));
    }

    /// <summary>
    ///     Sends the account which holds the slot to the locked match with an "AUTO_MATCH_CONNECT", either as a send-back or as a reminder, and records on the return whether the account was its matchmaking group's only member at that send.
    ///     The game client deletes such a group by itself when it acts on the send, but it ignores the send while it is still connecting, authenticating, or loading, so nothing is removed here, and <see cref="HandleReturnAcknowledgement"/> removes the group only once the game client acknowledges the send.
    ///     A solo group which NEXUS kept after the game client deleted it would be removed when the game client next creates a group, with an update which the game client shows as "Group Disbanded".
    ///     The frame is sent even when the group lookup fails, because the player has already been told that they are being sent back.
    /// </summary>
    private void SendToMatch(ClientChatSession session, MatchLock matchLock, MatchLockReturn matchLockReturn, bool reminder)
    {
        Action send = () => session.SendAutoMatchConnect(matchLock.ArrangedMatchType, matchLock.MatchID, matchLock.ServerAddress, matchLock.ServerPort, reminder);

        MatchmakingGroup? group;

        try
        {
            group = MatchmakingService.GetMatchmakingGroup(session.Account.ID);
        }

        catch (Exception exception)
        {
            logger.LogError(exception, @"[BUG] Could Not Look Up The Matchmaking Group Of Account ""{AccountName}"", Which Is Sent Back To Match ID ""{MatchID}"" Without Its Group Being Recorded",
                session.Account.NameWithClanTag, matchLock.MatchID);

            group = null;
        }

        if (group is null)
        {
            matchLockReturn.SoloGroupAtLastSend = null;

            send();
        }

        else
        {
            group.SendMemberToMatch(session.Account.ID, soleMember =>
            {
                // The Group Is Recorded Before The Frame Is Sent, So That The Game Client's Acknowledgement Can Never Be Handled Before The Record
                matchLockReturn.SoloGroupAtLastSend = soleMember ? group : null;

                send();
            });
        }
    }

    /// <summary>
    ///     Tells a refused player why, naming the account which holds the slot when it is another account of the same user, and telling the slot account to close its other game while that game is still connected to the match.
    /// </summary>
    private void Notify(ClientChatSession session, MatchLock matchLock, bool isSlotAccount, MatchLockVerdict verdict)
    {
        if (isSlotAccount && verdict is MatchLockVerdict.WaitForSlot)
        {
            session.SendNotice(MessageSenderName, ConnectedElsewhereMessage);

            logger.LogInformation(@"Telling Account ""{AccountName}"" To Close Its Other Game, Which Is Still Connected To Match ID ""{MatchID}"" On Match Server ID ""{MatchServerID}"", Before It Is Sent Back",
                session.Account.NameWithClanTag, matchLock.MatchID, matchLock.ServerID);
        }

        else if (isSlotAccount)
        {
            session.SendNotice(MessageSenderName, SlotAccountMessage);

            logger.LogInformation(@"Sending Account ""{AccountName}"" Back To Match ID ""{MatchID}"" On Match Server ID ""{MatchServerID}"", Which Is Still In Progress",
                session.Account.NameWithClanTag, matchLock.MatchID, matchLock.ServerID);
        }

        else
        {
            session.SendNotice(MessageSenderName, OtherAccountMessage(matchLock.TaggedAccountName));

            logger.LogInformation(@"Telling Account ""{AccountName}"" That Its User Is Locked To Match ID ""{MatchID}"" On Match Server ID ""{MatchServerID}"" As Account ""{SlotAccountName}""",
                session.Account.NameWithClanTag, matchLock.MatchID, matchLock.ServerID, matchLock.TaggedAccountName);
        }
    }

    /// <summary>
    ///     Sends the account which holds the slot back to the locked match at once, unless the account is still connected to the match from another machine, and reminds it every <see cref="ReturnResendInterval"/> until the return ends.
    ///     While the account is still connected from another machine, the slot is verified again at every interval and nothing is sent, and the account is sent back as soon as the slot is no longer connected.
    ///     An unanswered verification never ends that wait, because nobody has been sent anywhere yet, and the wait carries on until the match server answers again, the match server is gone or has ended the match, or the wait expires.
    ///     No chat message reaches the chat server when the match server refuses the connection, and the game client ignores a reminder while it is still connecting, so the reminders carry on until the game client reports joining the locked match server, the lock no longer holds, the session ends, or the return expires.
    ///     Once it has sent the account back, it never sends a notice, because the game client's own connection to the locked match server reads as connected while it loads; a wait which runs out before then ends with one short notice, because nobody was sent anywhere.
    ///     It runs without being awaited, so it handles its own exceptions.
    /// </summary>
    private async Task RemindToReturn(ClientChatSession session, MatchLock matchLock, MatchLockReturn matchLockReturn)
    {
        CancellationToken cancellationToken = matchLockReturn.Cancellation.Token;

        try
        {
            bool sentBack = matchLockReturn.SlotConnected is false;

            if (sentBack)
                SendToMatch(session, matchLock, matchLockReturn, reminder: false);

            while (true)
            {
                await Task.Delay(ReturnResendInterval, cancellationToken);

                if (session.IsConnected is false)
                    return;

                if (DateTimeOffset.UtcNow >= matchLockReturn.Expires)
                {
                    if (sentBack)
                    {
                        logger.LogInformation(@"Ending The Return Of Account ""{AccountName}"" To Match ID ""{MatchID}"", Because It Did Not Join Match Server ID ""{MatchServerID}"" Within The Return Window",
                            session.Account.NameWithClanTag, matchLock.MatchID, matchLock.ServerID);
                    }

                    else
                    {
                        // A Wait Which Has Not Sent The Player Back Cannot Be Waiting On The Player's Own Loading Connection, So The Notice Can Never Reach A Player Who Is Returning
                        session.SendNotice(MessageSenderName, ConnectedElsewhereWaitExpiredMessage);

                        logger.LogInformation(@"Ending The Return Of Account ""{AccountName}"" To Match ID ""{MatchID}"", Because Its Slot On Match Server ID ""{MatchServerID}"" Stayed Connected From Another Game",
                            session.Account.NameWithClanTag, matchLock.MatchID, matchLock.ServerID);
                    }

                    return;
                }

                MatchLock? currentLock = await distributedCacheStore.GetMatchLock(matchLock.UserID);

                if (currentLock is null || currentLock.MatchID != matchLock.MatchID)
                    return;

                // Until The Player Has Been Sent Back, Nobody Is Sent To The Match Server, So An Unanswered Query Keeps The Wait Rather Than Ending It, While A Match Server Which Is Gone Or Has Ended The Match Still Lifts The Lock
                MatchLockVerdict verdict = await Verify(currentLock, unconfirmedSlotLifts: sentBack);

                if (verdict is MatchLockVerdict.LiftSlotUnconfirmed && sentBack is false)
                    continue;

                if (KeepsLock(verdict) is false)
                    return;

                // The Return May Have Ended While The Match Server Was Being Queried
                if (cancellationToken.IsCancellationRequested)
                    return;

                matchLockReturn.LastVerified = DateTimeOffset.UtcNow;
                matchLockReturn.SlotConnected = verdict is MatchLockVerdict.WaitForSlot;

                if (verdict is MatchLockVerdict.WaitForSlot)
                    continue;

                if (sentBack is false)
                {
                    sentBack = true;

                    // A Send-Back After A Wait Lasts Exactly As Long As One Which Was Sent At Once, So That The Return Window Covers The Match Server's And The Game Client's Retries From This Moment
                    matchLockReturn.Expires = DateTimeOffset.UtcNow + ReturnWindow;

                    SendToMatch(session, currentLock, matchLockReturn, reminder: false);

                    logger.LogInformation(@"Sending Account ""{AccountName}"" Back To Match ID ""{MatchID}"" On Match Server ID ""{MatchServerID}"", Because Its Slot Is No Longer Connected From Another Machine",
                        session.Account.NameWithClanTag, currentLock.MatchID, currentLock.ServerID);

                    continue;
                }

                SendToMatch(session, currentLock, matchLockReturn, reminder: true);

                logger.LogDebug(@"Reminded Account ""{AccountName}"" To Return To Match ID ""{MatchID}"" On Match Server ID ""{MatchServerID}""",
                    session.Account.NameWithClanTag, currentLock.MatchID, currentLock.ServerID);
            }
        }

        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }

        catch (Exception exception)
        {
            logger.LogError(exception, @"The Return Of Account ""{AccountName}"" To Match ID ""{MatchID}"" Failed", session.Account.NameWithClanTag, matchLock.MatchID);
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
