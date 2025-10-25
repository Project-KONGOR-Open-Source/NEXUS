namespace TRANSMUTANSTEIN.ChatServer.Matchmaking;

public class MatchmakingGroup
{
    public MatchmakingGroupMember Leader => Members.Single(member => member.IsLeader);

    public required List<MatchmakingGroupMember> Members { get; set; }

    public required MatchmakingGroupInformation Information { get; set; }

    // public required ChatChannel ChatChannel { get; set; }

    public float AverageRating => 1500; // TODO: Members.Average(member => member.Rating);

    public float RatingDisparity => 100; // TODO: Members.Max(member => member.Rating) - Members.Min(member => member.Rating);

    public int FullTeamDifference => Information.TeamSize - Members.Count;

    public DateTimeOffset? QueueStartTime { get; set; } = null;

    public TimeSpan QueueDuration => QueueStartTime is not null ? DateTimeOffset.UtcNow - QueueStartTime.Value : TimeSpan.Zero;

    /// <summary>
    ///     Hidden Constructor Which Enforces <see cref="Create"/> As The Primary Mechanism For Creating Matchmaking Groups
    /// </summary>
    private MatchmakingGroup() { }

    public static MatchmakingGroup GetByMemberAccountID(int accountID)
    {
        MatchmakingGroup group = MatchmakingService.GetMatchmakingGroup(accountID)
            ?? throw new NullReferenceException($@"No Matchmaking Group Found For Account ID ""{accountID}""");

        return group;
    }

    public static MatchmakingGroup GetByMemberAccountName(string accountName)
    {
        MatchmakingGroup group = MatchmakingService.GetMatchmakingGroup(accountName)
            ?? throw new NullReferenceException($@"No Matchmaking Group Found For Account Name ""{accountName}""");

        return group;
    }

    public static MatchmakingGroup Create(ChatSession session, GroupCreateRequestData data)
    {
        MatchmakingGroupMember member = new (session)
        {
            Slot = 1, // The Group Leader Is Always In Slot 1
            IsLeader = true,
            IsReady = false,
            IsInGame = false,
            IsEligibleForMatchmaking = true,
            LoadingPercent = 0,
            GameModeAccess = string.Join('|', data.GameModes.Select(mode => "true"))
        };

        MatchmakingGroupInformation information = new ()
        {
            ClientVersion = data.ClientVersion,
            GroupType = data.GroupType,
            GameType = data.GameType,
            MapName = data.MapName,
            GameModes = data.GameModes,
            GameRegions = data.GameRegions,
            Ranked = data.Ranked,
            MatchFidelity = data.MatchFidelity,
            BotDifficulty = data.BotDifficulty,
            RandomizeBots = data.RandomizeBots
        };

        // TODO: Create Chat Channel For The Group

        MatchmakingGroup group = new () { Members = [member], Information = information };

        if (MatchmakingService.Groups.ContainsKey(session.Account.ID) is false)
        {
            // TODO: Check If The Account Is Already In A Matchmaking Group And Handle Accordingly

            if (MatchmakingService.Groups.TryAdd(session.Account.ID, group) is false)
                throw new InvalidOperationException($@"Failed To Create Matchmaking Group For Account ID ""{session.Account.ID}""");
        }

        else
        {
            if (MatchmakingService.Groups.TryUpdate(session.Account.ID, group, MatchmakingService.Groups[session.Account.ID]) is false)
                throw new InvalidOperationException($@"Failed To Update Matchmaking Group For Account ID ""{session.Account.ID}""");
        }

        group.MulticastUpdate(session.Account.ID, ChatProtocol.TMMUpdateType.TMM_CREATE_GROUP);

        return group;
    }

    public MatchmakingGroup Invite(ChatSession session, MerrickContext merrick, string receiverAccountName)
    {
        ChatBuffer invite = new ();

        invite.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_INVITE);
        invite.WriteString(session.Account.Name);                                                     // Invite Issuer Name
        invite.WriteInt32(session.Account.ID);                                                        // Invite Issuer ID
        invite.WriteInt8(Convert.ToByte(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_CONNECTED)); // Invite Issuer Status
        invite.WriteInt8(session.Account.GetChatClientFlags());                                       // Invite Issuer Chat Flags
        invite.WriteString(session.Account.NameColour);                                               // Invite Issuer Chat Name Colour
        invite.WriteString(session.Account.Icon);                                                     // Invite Issuer Icon
        invite.WriteString(Information.MapName);                                                      // Map Name
        invite.WriteInt8(Convert.ToByte(Information.GameType));                                       // Game Type
        invite.WriteString(string.Join('|', Information.GameModes));                                  // Game Modes
        invite.WriteString(string.Join('|', Information.GameRegions));                                // Game Regions

        ChatSession inviteReceiverSession = Context.ChatSessions
            .Values.Single(session => session.Account.Name.Equals(receiverAccountName));

        inviteReceiverSession.Send(invite);

        ChatBuffer broadcast = new ();

        Account inviteReceiver = merrick.Accounts.Include(account => account.Clan)
            .Single(account => account.Name.Equals(receiverAccountName));

        broadcast.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_INVITE_BROADCAST);
        broadcast.WriteString(inviteReceiver.NameWithClanTag);  // Invite Receiver Name
        broadcast.WriteString(session.Account.NameWithClanTag); // Invite Issuer Name

        Parallel.ForEach(Members, (member) => member.Session.Send(broadcast));

        return this;
    }

    public MatchmakingGroup Join(ChatSession session)
    {
        // TODO: If The Group Is Full (Members Count Is Equal To Max Map Players Count), Reject The Join Request With An Appropriate Error

        // TODO: If The Group Is Already In Queue For A Match, Reject The Join Request With An Appropriate Error

        MatchmakingGroupMember newMatchmakingGroupMember = new (session)
        {
            Slot = Convert.ToByte(Members.Count + 1),
            IsLeader = false,
            IsReady = true,
            IsInGame = false,
            IsEligibleForMatchmaking = true,
            LoadingPercent = 0,
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

            throw new InvalidOperationException($@"Player ""{session.Account.Name}"" Tried To Join A Matchmaking Group They Are Already In");
        }

        // TODO: Create Tentative Group, And Only Create Actual Group When Another Player Joins, Or Create Group As Is But Disband On Invite Refusal/Timeout
        MulticastUpdate(session.Account.ID, ChatProtocol.TMMUpdateType.TMM_PLAYER_JOINED_GROUP);

        // TODO: Create "TMM Group Chat" Chat Channel Or Join Already-Existing One For The Group; Must Have CannotBeJoined Flag Set

        return this;
    }

    public MatchmakingGroup SendLoadingStatusUpdate(ChatSession session, byte loadingPercent)
    {
        MatchmakingGroupMember groupMember = Members.Single(member => member.Account.ID == session.Account.ID);

        groupMember.LoadingPercent = loadingPercent;

        bool loaded = Members.All(member => member.LoadingPercent is 100);

        if (loaded)
        {
            ChatBuffer queue = new ();

            queue.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_JOIN_QUEUE);

            Parallel.ForEach(Members, member => member.Session.Send(queue));

            QueueStartTime = DateTimeOffset.UtcNow;

            ChatBuffer load = new ();

            load.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_QUEUE_UPDATE);
            load.WriteInt8(Convert.ToByte(ChatProtocol.TMMUpdateType.TMM_GROUP_QUEUE_UPDATE));
            // TODO: Get Actual Average Time In Queue (In Seconds)
            load.WriteInt32(83);

            Parallel.ForEach(Members, member => member.Session.Send(load));
        }

        MulticastUpdate(session.Account.ID, ChatProtocol.TMMUpdateType.TMM_PARTIAL_GROUP_UPDATE);

        return this;
    }

    public MatchmakingGroup SendPlayerReadinessStatusUpdate(ChatSession session, ChatProtocol.TMMGameType matchType)
    {
        Information.GameType = matchType;

        MatchmakingGroupMember groupMember = Members.Single(member => member.Account.ID == session.Account.ID);

        if (groupMember.IsLeader is false)
            return this; // Non-Leader Group Members Are Implicitly Ready (By Means Of Joining The Group In A Ready State) And Do Not Need To Emit Readiness Status Updates

        if (groupMember.IsReady is false)
        {
            foreach (MatchmakingGroupMember member in Members)
            {
                if (member.IsReady is false)
                {
                    if (member.IsLeader is false)
                    {
                        Log.Error(@"[BUG] Non-Leader Group Member ""{Member.Account.Name}"" With ID ""{Member.Account.ID}"" Was Not Ready", member.Account.Name, member.Account.ID);

                        member.IsReady = true;
                    }
                }
            }

            MulticastUpdate(session.Account.ID, ChatProtocol.TMMUpdateType.TMM_PARTIAL_GROUP_UPDATE);
        }

        if (Members.All(member => member.IsReady))
        {
            ChatBuffer load = new ();

            load.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_START_LOADING);

            Parallel.ForEach(Members, member => member.Session.Send(load));
        }

        return this;
    }

    private void MulticastUpdate(int emitterAccountID, ChatProtocol.TMMUpdateType updateType)
    {
        ChatBuffer update = new ();

        update.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE);

        update.WriteInt8(Convert.ToByte(updateType));                                    // Group Update Type
        update.WriteInt32(emitterAccountID);                                             // Account ID
        update.WriteInt8(Convert.ToByte(Members.Count));                                 // Group Size
        // TODO: Calculate Average Group Rating
        update.WriteInt16(1500);                                                         // Average Group Rating
        update.WriteInt32(Leader.Account.ID);                                            // Leader Account ID
        // TODO: Dynamically Set Arranged Match Type From The Request Data
        update.WriteInt8(Convert.ToByte(ChatProtocol.ArrangedMatchType.AM_MATCHMAKING)); // Arranged Match Type
        update.WriteInt8(Convert.ToByte(Information.GameType));                          // Game Type
        update.WriteString(Information.MapName);                                         // Map Name
        update.WriteString(string.Join('|', Information.GameModes));                     // Game Modes
        update.WriteString(string.Join('|', Information.GameRegions));                   // Game Regions
        update.WriteBool(Information.Ranked);                                            // Ranked
        update.WriteInt8(Information.MatchFidelity);                                     // Match Fidelity
        update.WriteInt8(Information.BotDifficulty);                                     // Bot Difficulty
        update.WriteBool(Information.RandomizeBots);                                     // Randomize Bots
        update.WriteString(string.Empty);                                                // Country Restrictions (e.g. "AB->USE|XY->USW" Means Only Country "AB" Can Access Region "USE" And Only Country "XY" Can Access Region "USW")
        // TODO: Find Out What Player Invitation Responses Do
        update.WriteString("What Is This ??? (Player Invitation Responses)");            // Player Invitation Responses
        update.WriteInt8(Information.TeamSize);                                          // Team Size (e.g. 5 For Forests Of Caldavar, 3 For Grimm's Crossing)
        update.WriteInt8(Convert.ToByte(Information.GroupType));                         // Group Type

        bool fullGroupUpdate = updateType switch
        {
            ChatProtocol.TMMUpdateType.TMM_CREATE_GROUP             => true,
            ChatProtocol.TMMUpdateType.TMM_FULL_GROUP_UPDATE        => true,
            ChatProtocol.TMMUpdateType.TMM_PLAYER_JOINED_GROUP      => true,
            ChatProtocol.TMMUpdateType.TMM_PLAYER_LEFT_GROUP        => true,
            ChatProtocol.TMMUpdateType.TMM_PLAYER_KICKED_FROM_GROUP => true,
            _                                                       => false
        };

        foreach (MatchmakingGroupMember member in Members)
        {
            if (fullGroupUpdate)
            {
                update.WriteInt32(member.Account.ID);                                    // Account ID
                update.WriteString(member.Account.Name);                                 // Account Name
                update.WriteInt8(member.Slot);                                           // Group Slot

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

                update.WriteInt32(20);                                                   // Normal Rank Level (Also Known As Normal Campaign Level Or Medal)
                update.WriteInt32(15);                                                   // Casual Rank Level (Also Known As Casual Campaign Level Or Medal)
                // TODO: Figure Out What These Ranks Are (Potentially Actual Global Ranking Index In Order Of Rating Descending, e.g. Highest Rating Is Rank 1)
                update.WriteInt32(20);                                                   // Normal Rank
                update.WriteInt32(15);                                                   // Casual Rank
                update.WriteBool(true);                                                  // Eligible For Campaign
                // TODO: Set Actual Rating, Dynamically From The Database
                // TODO: Can Be Set To -1 To Hide The Rating From Other Players For Unranked Game Modes
                update.WriteInt16(1850);                                                 // Rating
            }

            update.WriteInt8(member.LoadingPercent);                                     // Loading Percent (0 to 100)
            update.WriteBool(member.IsReady);                                            // Ready Status
            update.WriteBool(member.IsInGame);                                           // In-Game Status

            if (fullGroupUpdate)
            {
                update.WriteBool(member.IsEligibleForMatchmaking);                       // Eligible For Matchmaking
                update.WriteString(member.Account.NameColour);                           // Chat Name Colour
                update.WriteString(member.Account.Icon);                                 // Account Icon
                update.WriteString(member.Country);                                      // Country
                update.WriteBool(member.HasGameModeAccess);                              // Game Mode Access Bool
                update.WriteString(member.GameModeAccess);                               // Game Mode Access String
            }
        }

        if (fullGroupUpdate)
        {
            foreach (MatchmakingGroupMember member in Members)
            {
                // TODO: Determine Friendship Status
                update.WriteBool(false);                                                 // Is Friend
            }
        }

        Parallel.ForEach(Members, member => member.Session.Send(update));
    }
}
