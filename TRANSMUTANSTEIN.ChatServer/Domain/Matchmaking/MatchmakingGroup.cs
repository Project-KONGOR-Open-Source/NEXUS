using System.Globalization;

using MERRICK.DatabaseContext.Entities.Statistics;
using MERRICK.DatabaseContext.Extensions;

using TRANSMUTANSTEIN.ChatServer.Internals;
using TRANSMUTANSTEIN.ChatServer.Services;

namespace TRANSMUTANSTEIN.ChatServer.Domain.Matchmaking;

public class MatchmakingGroup
{
    private readonly Lock _lock = new();

    /// <summary>
    ///     Hidden Constructor Which Enforces <see cref="Create" /> As The Primary Mechanism For Creating Matchmaking Groups
    /// </summary>
    private MatchmakingGroup() { }

    public Guid GUID { get; } = Guid.CreateVersion7();

    public MatchmakingGroupMember Leader => Members.Single(member => member.IsLeader);

    public required List<MatchmakingGroupMember> Members { get; set; }

    public required MatchmakingGroupInformation Information { get; set; }

    // public required ChatChannel ChatChannel { get; set; }

    /// <summary>
    ///     Tracks users who have been invited to this group but have not yet joined.
    ///     Used by <see cref="Services.MatchmakingService.GetMatchmakingGroupByInvitedUser"/> to route JOIN requests
    ///     when the client sends the Invitee's name instead of the Leader's name.
    ///     Key: Invitee Account Name (Case-Insensitive).
    ///     Value: Timestamp of invitation (for future expiration logic).
    /// </summary>
    public ConcurrentDictionary<string, DateTimeOffset> PendingInvites { get; } = new(StringComparer.OrdinalIgnoreCase);

    public float AverageRating => (float) Members.Average(member => member.Rating);

    public float RatingDisparity => Members.Count > 0
        ? (float) (Members.Max(member => member.Rating) - Members.Min(member => member.Rating))
        : 0;

    public int FullTeamDifference => Information.TeamSize - Members.Count;

    public DateTimeOffset? QueueStartTime { get; set; }

    public TimeSpan QueueDuration =>
        QueueStartTime is not null ? DateTimeOffset.UtcNow - QueueStartTime.Value : TimeSpan.Zero;

    public static MatchmakingGroup GetByMemberAccountID(IMatchmakingService matchmakingService, int accountID)
    {
        MatchmakingGroup group = matchmakingService.GetMatchmakingGroup(accountID)
                                 ?? throw new NullReferenceException(
                                     $@"No Matchmaking Group Found For Account ID ""{accountID}""");

        return group;
    }

    public static MatchmakingGroup GetByMemberAccountName(IMatchmakingService matchmakingService, string accountName)
    {
        MatchmakingGroup group = matchmakingService.GetMatchmakingGroup(accountName)
                                 ?? throw new NullReferenceException(
                                     $@"No Matchmaking Group Found For Account Name ""{accountName}""");

        return group;
    }

    internal static MatchmakingGroup Create(IMatchmakingService matchmakingService, ChatSession session, MerrickContext merrick, MatchmakingGroupInformation information)
    {
        AccountStatistics? stats = merrick.AccountStatistics.Find(session.Account.ID);
        double rating = stats?.SkillRating ?? 1500.0;

        MatchmakingGroupMember member = new(session)
        {
            Slot = 1, // The Group Leader Is Always In Slot 1
            IsLeader = true,
            IsReady = false,
            IsInGame = false,
            IsEligibleForMatchmaking = true,
            LoadingPercent = 0,
            Rating = rating,
            GameModeAccess = string.Join('|', information.GameModes.Select(mode => "true"))
        };

        // TODO: Create Chat Channel For The Group

        MatchmakingGroup group = new() { Members = [member], Information = information };

        if (matchmakingService.Groups.ContainsKey(session.Account.ID) is false)
        {
            // TODO: Check If The Account Is Already In A Matchmaking Group And Handle Accordingly

            if (matchmakingService.Groups.TryAdd(session.Account.ID, group) is false)
            {
                throw new InvalidOperationException(
                    $@"Failed To Create Matchmaking Group For Account ID ""{session.Account.ID}""");
            }
        }

        else
        {
            if (matchmakingService.Groups.TryUpdate(session.Account.ID, group,
                    matchmakingService.Groups[session.Account.ID]) is false)
            {
                throw new InvalidOperationException(
                    $@"Failed To Update Matchmaking Group For Account ID ""{session.Account.ID}""");
            }
        }

        group.MulticastUpdate(session.Account.ID, ChatProtocol.TMMUpdateType.TMM_CREATE_GROUP);

        return group;
    }

    public MatchmakingGroup Invite(ChatSession session, MerrickContext merrick, IChatContext chatContext, string receiverAccountName)
    {
        lock (_lock)
        {
            ChatBuffer invite = new();

            invite.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_INVITE);
            invite.WriteString(session.Account.Name); // Invite Issuer Name
            invite.WriteInt32(session.Account.ID); // Invite Issuer ID
            invite.WriteInt8(Convert.ToByte(ChatProtocol.ChatClientStatus
                .CHAT_CLIENT_STATUS_CONNECTED, CultureInfo.InvariantCulture)); // Invite Issuer Status
            invite.WriteInt8(session.Account.GetChatClientFlags()); // Invite Issuer Chat Flags
            invite.WriteString(session.Account.GetNameColourNoPrefixCode()); // Invite Issuer Chat Name Colour
            invite.WriteString(session.Account.GetIconNoPrefixCode()); // Invite Issuer Icon
            invite.WriteString(Information.MapName); // Map Name
            invite.WriteInt8(Convert.ToByte(Information.GameType, CultureInfo.InvariantCulture)); // Game Type
            invite.WriteString(string.Join('|', Information.GameModes)); // Game Modes
            invite.WriteString(string.Join('|', Information.GameRegions)); // Game Regions

            // Attempt To Find The Invite Receiver In The Chat Context
            // Use TryGetValue For O(1) Lookup Instead Of O(N) Scan
            // Use Case-Insensitive Lookup If Possible, But ConcurrentDictionary Is Case-Sensitive By Default Unless Configured Otherwise
            // Assuming Account.Name Is Case-Sensitive Key
            if (chatContext.ClientChatSessions.TryGetValue(receiverAccountName, out ChatSession? inviteReceiverSession) is false)
            {
                // Fallback: Try Case-Insensitive Search If Exact Match Fails (Safety Net)
                inviteReceiverSession = chatContext.ClientChatSessions.Values
                    .FirstOrDefault(cSession => cSession.Account?.Name.Equals(receiverAccountName, StringComparison.OrdinalIgnoreCase) ?? false);
            }

            if (inviteReceiverSession is null)
            {
                Log.Warning(
                    @"Group Invite Failed: Receiver ""{ReceiverAccountName}"" Not Found Or Offline (Sender: ""{SenderAccountName}"")",
                    receiverAccountName, session.Account.Name);

                // Ideally Send A Notification To Sender Here
                return this;
            }

            inviteReceiverSession.Send(invite);

            PendingInvites.TryAdd(receiverAccountName, DateTimeOffset.UtcNow);

            ChatBuffer broadcast = new();

            Account? inviteReceiver = merrick.Accounts.Include(account => account.Clan)
                .SingleOrDefault(account => account.Name.Equals(receiverAccountName));

            if (inviteReceiver is null)
            {
                Log.Warning(
                    @"Group Invite Broadcast Failed: Receiver ""{ReceiverAccountName}"" Account Not Found In Database",
                    receiverAccountName);
                return this;
            }

            broadcast.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_INVITE_BROADCAST);
            broadcast.WriteString(inviteReceiver.GetNameWithClanTag()); // Invite Receiver Name
            broadcast.WriteString(session.Account.GetNameWithClanTag()); // Invite Issuer Name

            foreach (MatchmakingGroupMember member in Members)
            {
                member.Session.Send(broadcast);
            }

            return this;
        }
    }

    public MatchmakingGroup Join(ChatSession session, MerrickContext merrick)
    {
        lock (_lock)
        {
            if (Members.Count >= Information.TeamSize)
            {
                Log.Warning(
                    @"Account ID ""{AccountID}"" Failed To Join Matchmaking Group ""{GroupGUID}"" Because The Group Is Full",
                    session.Account.ID, GUID);

                ChatBuffer groupFull = new();

                groupFull.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_FAILED_TO_JOIN);
                groupFull.WriteInt8(Convert.ToByte(ChatProtocol.TMMFailedToJoinReason.TMMFTJR_GROUP_FULL,
                    CultureInfo.InvariantCulture));

                session.Send(groupFull);

                return this;
            }

            if (QueueStartTime is not null)
            {
                Log.Warning(
                    @"Account ID ""{AccountID}"" Failed To Join Matchmaking Group ""{GroupGUID}"" Because The Group Is Already Queued",
                    session.Account.ID, GUID);

                ChatBuffer alreadyQueued = new();

                alreadyQueued.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_FAILED_TO_JOIN);
                alreadyQueued.WriteInt8(Convert.ToByte(ChatProtocol.TMMFailedToJoinReason.TMMFTJR_ALREADY_QUEUED,
                    CultureInfo.InvariantCulture));

                session.Send(alreadyQueued);

                return this;
            }

            // Remove From Pending Invites (Cleanup)
            PendingInvites.TryRemove(session.Account.Name, out _);

            AccountStatistics? stats = merrick.AccountStatistics.Find(session.Account.ID);
            double rating = stats?.SkillRating ?? 1500.0;

            MatchmakingGroupMember newMatchmakingGroupMember = new(session)
            {
                Slot = Convert.ToByte(Members.Count + 1, CultureInfo.InvariantCulture),
                IsLeader = false,
                IsReady = true,
                IsInGame = false,
                IsEligibleForMatchmaking = true,
                LoadingPercent = 0,
                Rating = rating,
                HasGameModeAccess = true,
                GameModeAccess = Leader.GameModeAccess
            };

            if (Members.Any(member => member.Account.ID == session.Account.ID) is false)
            {
                // TODO: Remove From Previous Group, If Any

                Members.Add(newMatchmakingGroupMember);
            }

            else
            {
                // TODO: Send Failure Response

                throw new InvalidOperationException(
                    $@"Player ""{session.Account.Name}"" Tried To Join A Matchmaking Group They Are Already In");
            }

            // TODO: Create Tentative Group, And Only Create Actual Group When Another Player Joins, Or Create Group As Is But Disband On Invite Refusal/Timeout
            MulticastUpdate(session.Account.ID, ChatProtocol.TMMUpdateType.TMM_PLAYER_JOINED_GROUP);

            // TODO: Create "TMM Group Chat" Chat Channel Or Join Already-Existing One For The Group; Must Have CannotBeJoined Flag Set

            return this;
        }
    }

    public MatchmakingGroup SendLoadingStatusUpdate(ChatSession session, byte loadingPercent)
    {
        lock (_lock)
        {
            MatchmakingGroupMember groupMember = Members.Single(member => member.Account.ID == session.Account.ID);

            groupMember.LoadingPercent = loadingPercent;

            // Check If All Members Have Reached 100% Loading
            bool allMembersAreFullyLoaded = Members.All(member => member.LoadingPercent is 100);

            if (allMembersAreFullyLoaded)
            {
                JoinQueue();
            }

            MulticastUpdate(session.Account.ID, ChatProtocol.TMMUpdateType.TMM_PARTIAL_GROUP_UPDATE);

            return this;
        }
    }

    public MatchmakingGroup SendPlayerReadinessStatusUpdate(ChatSession session, ChatProtocol.TMMGameType matchType)
    {
        lock (_lock)
        {
            Information.GameType = matchType;

            MatchmakingGroupMember groupMember = Members.Single(member => member.Account.ID == session.Account.ID);

            // Non-Leader Group Members Are Implicitly Ready (By Means Of Joining The Group In A Ready State) And Do Not Need To Emit Readiness Status Updates
            if (groupMember.IsLeader is false)
            {
                return this;
            }

            if (groupMember.IsReady is false)
            {
                foreach (MatchmakingGroupMember member in Members)
                {
                    if (member.IsReady is false)
                    {
                        if (member.IsLeader is false)
                        {
                            Log.Error(
                                @"[BUG] Non-Leader Group Member ""{Member.Account.Name}"" With ID ""{Member.Account.ID}"" Was Not Ready",
                                member.Account.Name, member.Account.ID);
                        }

                        // All Matchmaking Group Members Need To Be Ready For The Queue To Start
                        member.IsReady = true;
                    }
                }

                MulticastUpdate(session.Account.ID, ChatProtocol.TMMUpdateType.TMM_PARTIAL_GROUP_UPDATE);
            }

            if (Members.All(member => member.IsReady))
            {
                ChatBuffer load = new();

                load.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_START_LOADING);

                foreach (MatchmakingGroupMember member in Members)
                {
                    member.Session.Send(load);
                }
            }

            return this;
        }
    }

    /// <summary>
    ///     Attempts to join the matchmaking queue.
    ///     Validates that all members are ready and fully loaded (100%) before joining.
    /// </summary>
    public void JoinQueue()
    {
        lock (_lock)
        {
            // Prevent Double-Queuing: Check If Already In Queue
            if (QueueStartTime is not null)
            {
                Log.Error(@"[BUG] Matchmaking Group GUID ""{Group.GUID}"" Tried To Join Queue While Already Queued",
                    GUID);

                return;
            }

            // Validate That All Members Are Ready And Loaded Before Joining Queue
            bool allMembersReadyAndLoaded = Members.All(member => member.IsReady && member.LoadingPercent is 100);

            if (allMembersReadyAndLoaded is false)
            {
                return;
            }

            // TODO: Validate Regional Restrictions (Turkey Region Requires GarenaID)
            // TODO: Validate Game Mode Restrictions (Lock Pick Only For 5-Person Groups)
            // TODO: Validate Disabled Game Modes
            // TODO: Update Group Statistics And Cache Information

            if (Information.GameType is ChatProtocol.TMMGameType.TMM_GAME_TYPE_PUBLIC && Members.Count > 1)
            {
                Log.Warning(
                    $@"Matchmaking Group GUID ""{GUID}"" Tried To Join Public Game Queue With {Members.Count} Members. Public Games Are Solo Only.");

                return;
            }

            // Set Group As Queued
            QueueStartTime = DateTimeOffset.UtcNow;

            // Broadcast Queue Join To All Group Members
            ChatBuffer joinQueueBroadcast = new();

            joinQueueBroadcast.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_JOIN_QUEUE);

            foreach (MatchmakingGroupMember member in Members)
            {
                member.Session.Send(joinQueueBroadcast);
            }

            // Broadcast Queue Update With Average Queue Time
            ChatBuffer queueUpdateBroadcast = new();

            queueUpdateBroadcast.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_QUEUE_UPDATE);
            queueUpdateBroadcast.WriteInt8(Convert.ToByte(ChatProtocol.TMMUpdateType.TMM_GROUP_QUEUE_UPDATE, CultureInfo.InvariantCulture));
            // TODO: Calculate Real Average Queue Time In Seconds
            queueUpdateBroadcast.WriteInt32(83);

            foreach (MatchmakingGroupMember member in Members)
            {
                member.Session.Send(queueUpdateBroadcast);
            }
        }
    }


    public void UpdateInformation(MatchmakingGroupInformation newInformation)
    {
        lock (_lock)
        {
            // Preserve Fields That Are Not Sent In The Update Packet
            newInformation.ClientVersion = Information.ClientVersion;
            newInformation.GroupType = Information.GroupType;
            
            Information = newInformation;

            MulticastUpdate(Leader.Account.ID, ChatProtocol.TMMUpdateType.TMM_FULL_GROUP_UPDATE);
        }
    }

    public void UpdateGroupType(ChatProtocol.TMMType newType)
    {
        lock (_lock)
        {
            Information.GroupType = newType;

            // Prevent invalid state: PVP Group cannot be in Public Game (TeamSize 1)
            // Default to Normal (TeamSize 5) until the OptionUpdate packet arrives
            if (newType == ChatProtocol.TMMType.TMM_TYPE_PVP && 
                Information.GameType == ChatProtocol.TMMGameType.TMM_GAME_TYPE_PUBLIC)
            {
                Information.GameType = ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL;
            }

            MulticastUpdate(Leader.Account.ID, ChatProtocol.TMMUpdateType.TMM_FULL_GROUP_UPDATE);
        }
    }

    public void MulticastUpdate(int emitterAccountID, ChatProtocol.TMMUpdateType updateType)
    {
        lock (_lock)
        {
            ChatBuffer update = new();

            update.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE);

            update.WriteInt8(Convert.ToByte(updateType, CultureInfo.InvariantCulture)); // Group Update Type
            update.WriteInt32(emitterAccountID); // Account ID
            update.WriteInt8(Convert.ToByte(Members.Count, CultureInfo.InvariantCulture)); // Group Size
            // TODO: Calculate Average Group Rating
            update.WriteInt16((short) AverageRating); // Average Group Rating
            update.WriteInt32(Leader.Account.ID); // Leader Account ID
            ChatProtocol.ArrangedMatchType arrangedMatchType = Information.GameType switch
            {
                // Explicitly Set Public Games To AM_PUBLIC
                ChatProtocol.TMMGameType.TMM_GAME_TYPE_PUBLIC => ChatProtocol.ArrangedMatchType.AM_PUBLIC,
                
                // CONDITIONAL MAPPING:
                // Solo MidWars/RiftWars must use generic AM_MATCHMAKING.
                // Group (PVP) MidWars/RiftWars must use specific AM_MATCHMAKING_MIDWARS/RIFTWARS.
                ChatProtocol.TMMGameType.TMM_GAME_TYPE_MIDWARS when Information.GroupType == ChatProtocol.TMMType.TMM_TYPE_PVP 
                    => ChatProtocol.ArrangedMatchType.AM_MATCHMAKING_MIDWARS,
                ChatProtocol.TMMGameType.TMM_GAME_TYPE_RIFTWARS when Information.GroupType == ChatProtocol.TMMType.TMM_TYPE_PVP 
                    => ChatProtocol.ArrangedMatchType.AM_MATCHMAKING_RIFTWARS,

                // Ranked (Normal/Casual) Groups likely require Campaign (Season) type
                ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL when Information.GroupType == ChatProtocol.TMMType.TMM_TYPE_PVP
                    => ChatProtocol.ArrangedMatchType.AM_MATCHMAKING_CAMPAIGN,
                ChatProtocol.TMMGameType.TMM_GAME_TYPE_CASUAL when Information.GroupType == ChatProtocol.TMMType.TMM_TYPE_PVP
                    => ChatProtocol.ArrangedMatchType.AM_MATCHMAKING_CAMPAIGN,
                ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_NORMAL when Information.GroupType == ChatProtocol.TMMType.TMM_TYPE_PVP
                    => ChatProtocol.ArrangedMatchType.AM_MATCHMAKING_CAMPAIGN,
                ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_CASUAL when Information.GroupType == ChatProtocol.TMMType.TMM_TYPE_PVP
                    => ChatProtocol.ArrangedMatchType.AM_MATCHMAKING_CAMPAIGN,
                
                // Use Standard AM_MATCHMAKING For All Other Queues
                _ => ChatProtocol.ArrangedMatchType.AM_MATCHMAKING
            };
            
            // 30: Arranged Match Type (Unknown1) - STRICT KONGOR PARITY
            update.WriteInt8(Convert.ToByte(arrangedMatchType, CultureInfo.InvariantCulture)); 
            
            // 31: Game Type
            update.WriteInt8(Convert.ToByte(Information.GameType, CultureInfo.InvariantCulture)); 
            update.WriteString(Information.MapName); // 32: Map Name
            update.WriteString(string.Join('|', Information.GameModes)); // 33: Game Modes
            update.WriteString(string.Join('|', Information.GameRegions)); // 34: Game Regions

            Log.Information("Sending Group Update: Modes={Modes}, Regions={Regions}, Type={UpdateType}, GroupType={GroupType}, GameType={GameType}, AM={AM}", 
                string.Join('|', Information.GameModes), 
                string.Join('|', Information.GameRegions), 
                updateType,
                Information.GroupType,
                Information.GameType,
                arrangedMatchType);

            // 35: Ranked (Verified)
            update.WriteBool(Information.Ranked); 
            
            // 36: Match Fidelity (VerifiedOnly)
            update.WriteInt8(Information.MatchFidelity); 
            
            // 37: Bot Difficulty
            update.WriteInt8(Information.BotDifficulty); 
            
            // 38: Randomize Bots
            update.WriteBool(Information.RandomizeBots); 

            // 39: Unknown2 (STRING) - STRICT KONGOR PARITY (Previously was Byte(0))
            update.WriteString(string.Empty);

            // 40: Invitation Responses (String)
            update.WriteString(string.Empty); 
            
            // 41: Team Size (Byte)
            update.WriteInt8(Information.TeamSize); 
            
            // 42: Group Type (Byte)
            update.WriteInt8(Convert.ToByte(Information.GroupType, CultureInfo.InvariantCulture));


            bool fullGroupUpdate = updateType switch
            {
                ChatProtocol.TMMUpdateType.TMM_CREATE_GROUP => true,
                ChatProtocol.TMMUpdateType.TMM_FULL_GROUP_UPDATE => true,
                ChatProtocol.TMMUpdateType.TMM_PLAYER_JOINED_GROUP => true,
                ChatProtocol.TMMUpdateType.TMM_PLAYER_LEFT_GROUP => true,
                ChatProtocol.TMMUpdateType.TMM_PLAYER_KICKED_FROM_GROUP => true,
                _ => false
            };

            foreach (MatchmakingGroupMember member in Members)
            {
                if (fullGroupUpdate)
                {
                    update.WriteInt32(member.Account.ID); // Account ID
                    update.WriteString(member.Account.Name); // Account Name
                    update.WriteInt8(member.Slot); // Group Slot

                    // TODO: Get Real Rank Level And Rating
                    /* TODO: Establish Rank (Medal) Level From Rating And Add To The Database
                        enum ECampaignLevel
                        {
                            CAMPAIGN_LEVEL_NONE = 0,

                            CAMPAIGN_LEVEL_BRONZE_5,
                            CAMPAIGN_LEVEL_BRONZE_4,
                            CAMPAIGN_LEVEL_BRONZE_3,
                            CAMPAIGN_LEVEL_BRONZE_2,
                            CAMPAIGN_LEVEL_BRONZE_1,

                            CAMPAIGN_LEVEL_SILVER_5,
                            CAMPAIGN_LEVEL_SILVER_4,
                            CAMPAIGN_LEVEL_SILVER_3,
                            CAMPAIGN_LEVEL_SILVER_2,
                            CAMPAIGN_LEVEL_SILVER_1,

                            CAMPAIGN_LEVEL_GOLD_4,
                            CAMPAIGN_LEVEL_GOLD_3,
                            CAMPAIGN_LEVEL_GOLD_2,
                            CAMPAIGN_LEVEL_GOLD_1,

                            CAMPAIGN_LEVEL_DIAMOND_3,
                            CAMPAIGN_LEVEL_DIAMOND_2,
                            CAMPAIGN_LEVEL_DIAMOND_1,

                            CAMPAIGN_LEVEL_LEGENDARY2,
                            CAMPAIGN_LEVEL_LEGENDARY1,

                            CAMPAIGN_LEVEL_IMMORTAL
                        };
                    */

                    update.WriteInt32(20); // Normal Rank Level (Also Known As Normal Campaign Level Or Medal)
                    update.WriteInt32(15); // Casual Rank Level (Also Known As Casual Campaign Level Or Medal)
                    // TODO: Figure Out What These Ranks Are (Potentially Actual Global Ranking Index In Order Of Rating Descending, e.g. Highest Rating Is Rank 1)
                    update.WriteInt32(20); // Normal Rank
                    update.WriteInt32(15); // Casual Rank
                    update.WriteBool(true); // Eligible For Campaign
                    // TODO: Set Actual Rating, Dynamically From The Database
                    // TODO: Can Be Set To -1 To Hide The Rating From Other Players For Unranked Game Modes
                    update.WriteInt16((short) member.Rating); // Rating
                }

                update.WriteInt8(member.LoadingPercent); // Loading Percent (0 to 100)
                update.WriteBool(member.IsReady); // Ready Status
                update.WriteBool(member.IsInGame); // In-Game Status

                if (fullGroupUpdate)
                {
                    update.WriteBool(member.IsEligibleForMatchmaking); // Eligible For Matchmaking
                    update.WriteString(member.Account.GetNameColourNoPrefixCode()); // Chat Name Colour
                    update.WriteString(member.Account.GetIconNoPrefixCode()); // Account Icon
                    update.WriteString(member.Country); // Country
                    update.WriteBool(member.HasGameModeAccess); // Game Mode Access Bool
                    update.WriteString(member.GameModeAccess); // Game Mode Access String
                }
            }

            if (fullGroupUpdate)
            {
                foreach (MatchmakingGroupMember member in Members)
                {
                    // TODO: Determine Friendship Status
                    update.WriteBool(false); // Is Friend
                }
            }

            foreach (MatchmakingGroupMember member in Members)
            {
                member.Session.Send(update);
            }
        }
    }

    /// <summary>
    ///     Removes a member from the matchmaking group.
    ///     If the leader leaves and other members remain, leadership transfers to the member with the lowest slot index.
    ///     If the leader leaves and no other members remain, or if the last member leaves, the group is disbanded.
    /// </summary>
    public void RemoveMember(IMatchmakingService matchmakingService, int accountID, bool kick = false)
    {
        lock (_lock)
        {
            MatchmakingGroupMember? memberToRemove = Members.SingleOrDefault(member => member.Account.ID == accountID);

            if (memberToRemove is null)
            {
                return;
            }

            bool memberToRemoveIsLeader = memberToRemove.IsLeader;

            // Broadcast Removal To All Members Before Removing
            ChatProtocol.TMMUpdateType updateType = kick
                ? ChatProtocol.TMMUpdateType.TMM_PLAYER_KICKED_FROM_GROUP
                : ChatProtocol.TMMUpdateType.TMM_PLAYER_LEFT_GROUP;

            // Send Partial Group Update Indicating Member Removal
            MulticastUpdate(accountID, updateType);

            // Remove Member From Group
            Members.Remove(memberToRemove);

            // If Group Is Now Empty, Disband It
            if (Members.Count is 0)
            {
                DisbandGroup(matchmakingService, accountID);

                return;
            }

            // Reassign Slots And Transfer Leadership
            ReassignSlots();

            // If the removed member was the leader, we must migrate the group in the MatchmakingService.Groups dictionary
            // to be keyed by the NEW leader's ID.
            if (memberToRemoveIsLeader)
            {
                if (matchmakingService.Groups.TryRemove(accountID, out MatchmakingGroup? existingGroup))
                {
                    if (existingGroup != this)
                    {
                        // Paranoid check: If we removed the wrong group (shouldn't happen if IDs are unique), put it back?
                        // Or just log error.
                        Log.Error(@"[BUG] Groups Dictionary State Mismatch During Leader Migration. Removed Group GUID ""{RemovedGUID}"" But Expected ""{CurrentGUID}""", existingGroup.GUID, GUID);
                    }
                    
                    if (matchmakingService.Groups.TryAdd(Leader.Account.ID, this) is false)
                    {
                         Log.Error(@"[BUG] Failed To Re-Add Matchmaking Group For New Leader ""{LeaderID}""", Leader.Account.ID);
                    }
                    else
                    {
                        Log.Information(@"Migrated Matchmaking Group Key From ""{OldLeaderID}"" To ""{NewLeaderID}""", accountID, Leader.Account.ID);
                    }
                }
                else
                {
                     Log.Error(@"[BUG] Failed To Remove Old Leader Key ""{OldLeaderID}"" During Migration", accountID);
                }
            }

            // Reset All Members To Default Readiness State: Leader = Not Ready, Others = Ready
            // Non-Leader Members Should Always Be Ready So That Group Readiness Is Determined Solely By The Leader
            foreach (MatchmakingGroupMember member in Members)
            {
                member.IsReady = member.IsLeader is false;
            }

            if (QueueStartTime is not null)
            {
                LeaveQueue(accountID);
            }

            // TODO: Leave Group Chat Channel

            // Send Full Group Update To Remaining Members
            MulticastUpdate(accountID, ChatProtocol.TMMUpdateType.TMM_FULL_GROUP_UPDATE);
        }
    }

    /// <summary>
    ///     Removes the group from the matchmaking queue and resets member readiness.
    ///     Broadcasts LEAVE_QUEUE message to all members.
    /// </summary>
    public void LeaveQueue(int emitterAccountID)
    {
        lock (_lock)
        {
            if (QueueStartTime is null)
            {
                return;
            }

            // Remove Group From Queue
            QueueStartTime = null;

            // Unready The Group Leader And Unload All Members
            // Non-Leader Members Should Always Be Ready So That Group Readiness Is Determined Solely By The Leader
            foreach (MatchmakingGroupMember member in Members)
            {
                member.IsReady = member.IsLeader is false;
                member.LoadingPercent = 0;
            }

            // Broadcast Leave Queue To All Group Members
            ChatBuffer leaveQueueBroadcast = new();

            leaveQueueBroadcast.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_LEAVE_QUEUE);

            foreach (MatchmakingGroupMember member in Members)
            {
                member.Session.Send(leaveQueueBroadcast);
            }
        }
    }


    /// <summary>
    ///     Removes the specified member from the group and records the action as a kick.
    /// </summary>
    public void KickMember(IMatchmakingService matchmakingService, int accountID)
    {
        RemoveMember(matchmakingService, accountID, true);
    }

    public void Kick(IMatchmakingService matchmakingService, ChatSession session, byte slot)
    {
        lock (_lock)
        {
            if (Leader.Account.ID != session.Account.ID)
            {
                // Only Leader Can Kick
                return;
            }

            MatchmakingGroupMember? target = Members.SingleOrDefault(m => m.Slot == slot);
            if (target != null)
            {
                KickMember(matchmakingService, target.Account.ID);
            }
        }
    }

    /// <summary>
    ///     Reassigns team slots to all group members sequentially based on current member order.
    ///     Leader always receives slot 1, and subsequent members receive slots 2, 3, 4, and 5.
    /// </summary>
    private void ReassignSlots()
    {
        if (Members.Count == 0)
        {
            Log.Error(@"[BUG] Attempted To Reassign Slots In Empty Matchmaking Group GUID ""{Group.GUID}""", GUID);

            return;
        }

        Members = [.. Members.OrderBy(member => member.Slot)];

        foreach (MatchmakingGroupMember member in Members)
        {
            member.Slot = Convert.ToByte(Members.IndexOf(member) + 1, CultureInfo.InvariantCulture);
        }

        Members.Single(member => member.Slot is 1).IsLeader = true;
    }

    /// <summary>
    ///     Disbands the matchmaking group by removing it from the matchmaking service registry.
    ///     Called when the last member leaves or when the leader leaves with no other members.
    /// </summary>
    private void DisbandGroup(IMatchmakingService matchmakingService, int accountID)
    {
        // Remove Group From Matchmaking Service Registry
        if (matchmakingService.Groups.TryRemove(accountID, out MatchmakingGroup? group) is false)
        {
            Log.Error(
                @"Failed To Disband Matchmaking Group GUID ""{Group.GUID}"" For Account ID ""{Member.Account.ID}""",
                group?.GUID ?? Guid.Empty, accountID);
        }

        // TODO: Remove From Queue If Queued

        // TODO: Remove All Members From Group Chat Channel
    }
}