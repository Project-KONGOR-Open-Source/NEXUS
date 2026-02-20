namespace TRANSMUTANSTEIN.ChatServer.Domain.Matchmaking;

public class MatchmakingGroup
{
    private const double MaximumTMR = 2500.0;
    private const int InexperiencedMatchCount = 50;
    private const double InexperiencedTMRCutoff = 1625.0;
    private const double MaximumGroupTMRDisparity = 150.0;

    public Guid GUID { get; } = Guid.CreateVersion7();

    public MatchmakingGroupMember Leader => Members.Single(member => member.IsLeader);

    public required List<MatchmakingGroupMember> Members { get; set; }

    public required MatchmakingGroupInformation Information { get; set; }

    /// <summary>
    ///     The group chat channel for pre-match communication.
    ///     Uses RESERVED | UNJOINABLE | HIDDEN flags per original implementation.
    /// </summary>
    public ChatChannel? ChatChannel { get; set; }

    /// <summary>
    ///     The total TMR (Team Match Rating) of all members in this group.
    /// </summary>
    public double TotalTMR => Members.Sum(member => member.TMR);

    /// <summary>
    ///     The average TMR of all members in this group.
    /// </summary>
    public double AverageTMR => Members.Count > 0 ? TotalTMR / Members.Count : 0;

    /// <summary>
    ///     The adjusted TMR for this group, including a bonus for premade coordination.
    /// </summary>
    /// <remarks>
    ///     Formula: groupMMR += 4 * 2^groupSize (Solo=0, 2p=+16, 3p=+32, 4p=+64, 5p=+128)
    /// </remarks>
    public double AdjustedTotalTMR => TotalTMR + GetPremadeBonus();

    /// <summary>
    ///     The adjusted average TMR for this group, including premade bonus.
    /// </summary>
    public double AdjustedAverageTMR => Members.Count > 0 ? AdjustedTotalTMR / Members.Count : 0;

    /// <summary>
    ///     Gets the premade coordination bonus based on group size.
    ///     Larger groups get a higher bonus to compensate for voice comms and coordination advantage.
    /// </summary>
    public double GetPremadeBonus()
    {
        if (Members.Count <= 1)
            return 0;

        return 4.0 * Math.Pow(2.0, Members.Count);
    }

    /// <summary>
    ///     The highest TMR among all members in this group.
    /// </summary>
    public double HighestTMR => Members.Count > 0 ? Members.Max(member => member.TMR) : 0;

    /// <summary>
    ///     The lowest TMR among all members in this group.
    /// </summary>
    public double LowestTMR => Members.Count > 0 ? Members.Min(member => member.TMR) : 0;

    /// <summary>
    ///     The TMR range within this group.
    /// </summary>
    public double TMRRange => HighestTMR - LowestTMR;

    /// <summary>
    ///     The total number of matches played by all members in this group.
    /// </summary>
    public int TotalMatchCount => Members.Sum(member => member.TotalMatchCount);

    /// <summary>
    ///     The average kill/death ratio of all members in this group.
    /// </summary>
    public double AverageKD => Members.Count > 0 ? Members.Average(member => member.KDRatio) : 0;

    public float AverageRating => (float)AverageTMR;

    public float RatingDisparity => (float)TMRRange;

    public int FullTeamDifference => Information.TeamSize - Members.Count;

    public bool IsFull => Members.Count >= Information.TeamSize;

    public bool IsQueued => QueueStartTime is not null;

    public DateTimeOffset? QueueStartTime { get; set; } = null;

    public TimeSpan QueueDuration => QueueStartTime is not null ? DateTimeOffset.UtcNow - QueueStartTime.Value : TimeSpan.Zero;

    public double QueuedTimeInMinutes => QueueDuration.TotalMinutes;

    public double QueuedTimeInSeconds => QueueDuration.TotalSeconds;

    /// <summary>
    ///     Whether this group has been matched and is awaiting a match to start.
    /// </summary>
    public bool MatchedUp { get; set; }

    /// <summary>
    ///     The GUID of the team this group has been assigned to.
    /// </summary>
    public Guid? AssignedTeamGUID { get; set; }

    /// <summary>
    ///     The GUID of the match this group has been assigned to.
    /// </summary>
    public Guid? AssignedMatchGUID { get; set; }

    /// <summary>
    ///     Determines if this group is considered "experienced" based on match count or TMR.
    ///     In the original implementation, a group is experienced if matchCount >= 50 OR TMR >= 1625.
    /// </summary>
    public bool IsExperienced => TotalMatchCount >= InexperiencedMatchCount || AverageTMR >= InexperiencedTMRCutoff;

    /// <summary>
    ///     Gets the adaptive TMR spread based on queue wait time.
    ///     As queue time increases, the acceptable TMR range widens to improve match finding.
    ///     Based on the original adaptive TMR spread algorithm.
    /// </summary>
    public double GetAdaptiveTMRSpread()
    {
        double baseTMRSpread = TMRRange;
        double queueMinutes = QueuedTimeInMinutes;

        // Widen TMR spread as queue time increases (approximately 50 TMR per minute after 2 minutes)
        if (queueMinutes > 2.0)
        {
            double additionalSpread = (queueMinutes - 2.0) * 50.0;

            baseTMRSpread += additionalSpread;
        }

        // Cap The Maximum Spread
        return Math.Min(baseTMRSpread, MaximumTMR);
    }

    /// <summary>
    ///     Checks if this group is compatible with another group for team formation.
    ///     Based on the original compatibility check algorithm.
    /// </summary>
    /// <param name="other">The other group to check compatibility with.</param>
    /// <returns>TRUE if the groups are compatible, FALSE otherwise.</returns>
    public bool IsCompatibleWith(MatchmakingGroup other)
    {
        // Must Have Matching Game Type
        if (Information.GameType != other.Information.GameType)
            return false;

        // Must Have Overlapping Game Modes
        if (Information.GameModes.Intersect(other.Information.GameModes).Any() is false)
            return false;

        // Must Have Overlapping Regions (NEWERTH Is A Wildcard That Matches All Regions)
        bool eitherHasAutoRegion = Information.GameRegions.Contains("NEWERTH", StringComparer.OrdinalIgnoreCase)
            || other.Information.GameRegions.Contains("NEWERTH", StringComparer.OrdinalIgnoreCase);

        if (eitherHasAutoRegion is false && Information.GameRegions.Intersect(other.Information.GameRegions).Any() is false)
            return false;

        // Must Be Same Ranked Status
        if (Information.Ranked != other.Information.Ranked)
            return false;

        // Combined Size Must Not Exceed Team Size
        if (Members.Count + other.Members.Count > Information.TeamSize)
            return false;

        // TMR Should Be Within Adaptive Range
        double combinedTMRSpread = GetAdaptiveTMRSpread();
        double tmrDifference = Math.Abs(AverageTMR - other.AverageTMR);

        if (tmrDifference > combinedTMRSpread)
            return false;

        // Experience Level Should Match (Experienced vs Inexperienced)
        if (IsExperienced != other.IsExperienced)
            return false;

        return true;
    }

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

    internal static MatchmakingGroup Create(ClientChatSession session, MatchmakingGroupInformation information)
    {
        // Check If Already In A Matchmaking Group And Remove From It First
        MatchmakingGroup? existingGroup = MatchmakingService.GetMatchmakingGroup(session.Account.ID);

        if (existingGroup is not null)
        {
            existingGroup.RemoveMember(session.Account.ID);
        }

        MatchmakingGroupMember member = new (session)
        {
            Slot = 1, // The Group Leader Is Always In Slot 1
            IsLeader = true,
            IsReady = false,
            IsInGame = false,
            IsEligibleForMatchmaking = true,
            LoadingPercent = 0,
            GameModeAccess = string.Join('|', information.GameModes.Select(mode => "true"))
        };

        MatchmakingGroup group = new () { Members = [member], Information = information };

        // Create Group Chat Channel (Uses RESERVED | UNJOINABLE | HIDDEN Flags)
        group.ChatChannel = ChatChannel.GetOrCreateGroupChannel(group.GUID.GetHashCode());

        // Join Group Chat Channel (Silent Join For Leader)
        if (group.ChatChannel.Members.ContainsKey(session.Account.Name) is false)
        {
            ChatChannelMember channelMember = new (session, group.ChatChannel);
            group.ChatChannel.Members.TryAdd(session.Account.Name, channelMember);
            session.CurrentChannels.Add(group.ChatChannel.ID);
        }

        if (MatchmakingService.Groups.ContainsKey(session.Account.ID) is false)
        {
            if (MatchmakingService.Groups.TryAdd(session.Account.ID, group) is false)
                throw new InvalidOperationException($@"Failed To Create Matchmaking Group For Account ID ""{session.Account.ID}""");
        }

        else
        {
            if (MatchmakingService.Groups.TryUpdate(session.Account.ID, group, MatchmakingService.Groups[session.Account.ID]) is false)
                throw new InvalidOperationException($@"Failed To Update Matchmaking Group For Account ID ""{session.Account.ID}""");
        }

        group.MulticastUpdate(session.Account.ID, ChatProtocol.TMMUpdateType.TMM_CREATE_GROUP);

        // Solo Co-Op Groups Bypass The Readiness And Loading Flow Because There Are No Other Players To Wait For
        if (information.GroupType == ChatProtocol.TMMType.TMM_TYPE_COOP && group.Members.Count == 1)
        {
            member.IsReady = true;
            member.LoadingPercent = 100;

            group.JoinQueue();
        }

        return group;
    }

    public MatchmakingGroup Invite(ClientChatSession session, MerrickContext merrick, string receiverAccountName)
    {
        ChatBuffer invite = new ();

        invite.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_INVITE);
        invite.WriteString(session.Account.Name);                                                     // Invite Issuer Name
        invite.WriteInt32(session.Account.ID);                                                        // Invite Issuer ID
        invite.WriteInt8(Convert.ToByte(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_CONNECTED)); // Invite Issuer Status
        invite.WriteInt8(session.Account.GetChatClientFlags());                                       // Invite Issuer Chat Flags
        invite.WriteString(session.Account.NameColourNoPrefixCode);                                   // Invite Issuer Chat Name Colour
        invite.WriteString(session.Account.IconNoPrefixCode);                                         // Invite Issuer Icon
        invite.WriteString(Information.MapName);                                                      // Map Name
        invite.WriteInt8(Convert.ToByte(Information.GameType));                                       // Game Type
        invite.WriteString(string.Join('|', Information.GameModes));                                  // Game Modes
        invite.WriteString(string.Join('|', Information.GameRegions));                                // Game Regions

        ClientChatSession inviteReceiverSession = Context.ClientChatSessions
            .Values.Single(session => session.Account.Name.Equals(receiverAccountName));

        inviteReceiverSession.Send(invite);

        ChatBuffer broadcast = new ();

        Account inviteReceiver = merrick.Accounts.Include(account => account.Clan)
            .Single(account => account.Name.Equals(receiverAccountName));

        broadcast.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_INVITE_BROADCAST);
        broadcast.WriteString(inviteReceiver.NameWithClanTag);  // Invite Receiver Name
        broadcast.WriteString(session.Account.NameWithClanTag); // Invite Issuer Name

        foreach (MatchmakingGroupMember member in Members)
            member.Session.Send(broadcast);

        return this;
    }

    public MatchmakingGroup Join(ClientChatSession session)
    {
        // If The Group Is Full, Reject The Join Request
        if (IsFull)
        {
            ChatBuffer error = new ();

            error.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_FAILED_TO_JOIN);
            error.WriteInt8(Convert.ToByte(ChatProtocol.TMMFailedToJoinReason.TMMFTJR_GROUP_FULL));
            error.WriteInt32(0); // Ban Duration (Not Applicable)

            session.Send(error);

            return this;
        }

        // If The Group Is Already In Queue, Reject The Join Request
        if (IsQueued)
        {
            ChatBuffer error = new ();

            error.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_FAILED_TO_JOIN);
            error.WriteInt8(Convert.ToByte(ChatProtocol.TMMFailedToJoinReason.TMMFTJR_ALREADY_QUEUED));
            error.WriteInt32(0); // Ban Duration (Not Applicable)

            session.Send(error);

            return this;
        }

        // Remove From Previous Group If Any
        MatchmakingGroup? existingGroup = MatchmakingService.GetMatchmakingGroup(session.Account.ID);

        if (existingGroup is not null && existingGroup.GUID != this.GUID)
        {
            existingGroup.RemoveMember(session.Account.ID);
        }

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
            Members.Add(newMatchmakingGroupMember);
        }

        else
        {
            Log.Warning(@"Player ""{AccountName}"" Tried To Join A Matchmaking Group They Are Already In", session.Account.Name);

            return this;
        }

        // Join Group Chat Channel (Uses RESERVED | UNJOINABLE | HIDDEN Flags)
        if (ChatChannel is not null && ChatChannel.Members.ContainsKey(session.Account.Name) is false)
        {
            ChatChannelMember channelMember = new (session, ChatChannel);
            ChatChannel.Members.TryAdd(session.Account.Name, channelMember);
            session.CurrentChannels.Add(ChatChannel.ID);
        }

        MulticastUpdate(session.Account.ID, ChatProtocol.TMMUpdateType.TMM_PLAYER_JOINED_GROUP);

        return this;
    }

    public MatchmakingGroup SendLoadingStatusUpdate(ClientChatSession session, byte loadingPercent)
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

    public MatchmakingGroup SendPlayerReadinessStatusUpdate(ClientChatSession session, ChatProtocol.TMMGameType matchType)
    {
        Information.GameType = matchType;

        MatchmakingGroupMember groupMember = Members.Single(member => member.Account.ID == session.Account.ID);

        // Non-Leader Group Members Are Implicitly Ready (By Means Of Joining The Group In A Ready State) And Do Not Need To Emit Readiness Status Updates
        if (groupMember.IsLeader is false)
            return this;

        if (groupMember.IsReady is false)
        {
            foreach (MatchmakingGroupMember member in Members)
            {
                if (member.IsReady is false)
                {
                    if (member.IsLeader is false)
                        Log.Error(@"[BUG] Non-Leader Group Member ""{Member.Account.Name}"" With ID ""{Member.Account.ID}"" Was Not Ready", member.Account.Name, member.Account.ID);

                    // All Matchmaking Group Members Need To Be Ready For The Queue To Start
                    member.IsReady = true;
                }
            }

            MulticastUpdate(session.Account.ID, ChatProtocol.TMMUpdateType.TMM_PARTIAL_GROUP_UPDATE);
        }

        if (Members.All(member => member.IsReady))
        {
            ChatBuffer load = new ();

            load.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_START_LOADING);

            foreach (MatchmakingGroupMember member in Members)
                member.Session.Send(load);
        }

        return this;
    }

    /// <summary>
    ///     Attempts to join the matchmaking queue.
    ///     Validates that all members are ready and fully loaded (100%) before joining.
    /// </summary>
    public void JoinQueue()
    {
        // Prevent Double-Queuing: Check If Already In Queue
        if (QueueStartTime is not null)
        {
            Log.Error(@"[BUG] Matchmaking Group GUID ""{Group.GUID}"" Tried To Join Queue While Already Queued", GUID);

            return;
        }

        // Validate That All Members Are Ready And Loaded Before Joining Queue
        bool allMembersReadyAndLoaded = Members.All(member => member.IsReady && member.LoadingPercent is 100);

        if (allMembersReadyAndLoaded is false)
        {
            return;
        }

        // Validate MMR Disparity Within Group (Prevents Boosting/Smurfing)
        // If The Highest-Rated Player Is Too Far Above The Average Of The Rest, Reject
        if (Members.Count > 1 && HasExcessiveTMRDisparity())
        {
            SendQueueJoinError("The rating disparity within your group is too high. The difference between the highest-rated player and the average of others cannot exceed 150.");

            return;
        }

        // TODO: Validate Regional Restrictions (Turkey Region Requires GarenaID)
        // TODO: Validate Game Mode Restrictions (Lock Pick Only For 5-Person Groups)
        // TODO: Validate Disabled Game Modes
        // TODO: Update Group Statistics And Cache Information

        // Set Group As Queued
        QueueStartTime = DateTimeOffset.UtcNow;

        // Broadcast Queue Join To All Group Members
        ChatBuffer joinQueueBroadcast = new ();

        joinQueueBroadcast.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_JOIN_QUEUE);

        foreach (MatchmakingGroupMember member in Members)
            member.Session.Send(joinQueueBroadcast);

        // Broadcast Queue Update With Average Queue Time
        ChatBuffer queueUpdateBroadcast = new ();

        queueUpdateBroadcast.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_QUEUE_UPDATE);
        queueUpdateBroadcast.WriteInt8(Convert.ToByte(ChatProtocol.TMMUpdateType.TMM_GROUP_QUEUE_UPDATE));
        queueUpdateBroadcast.WriteInt32(83); // TODO: Calculate Real Average Queue Time In Seconds

        foreach (MatchmakingGroupMember member in Members)
            member.Session.Send(queueUpdateBroadcast);

        Log.Debug(@"Group GUID ""{Group.GUID}"" Joined Queue With {MemberCount} Member(s)", GUID, Members.Count);
    }

    public void MulticastUpdate(int emitterAccountID, ChatProtocol.TMMUpdateType updateType)
    {
        ChatBuffer update = new ();

        update.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE);

        update.WriteInt8(Convert.ToByte(updateType));                                  // Group Update Type
        update.WriteInt32(emitterAccountID);                                           // Account ID
        update.WriteInt8(Convert.ToByte(Members.Count));                               // Group Size
        update.WriteInt16(Convert.ToInt16(Math.Clamp(AverageTMR, 0, short.MaxValue))); // Average Group Rating (Calculated)
        update.WriteInt32(Leader.Account.ID);                                          // Leader Account ID
        update.WriteInt8(Convert.ToByte(Information.ArrangedMatchType));               // Arranged Match Type (From Information)
        update.WriteInt8(Convert.ToByte(Information.GameType));                        // Game Type
        update.WriteString(Information.MapName);                                       // Map Name
        update.WriteString(string.Join('|', Information.GameModes));                   // Game Modes
        update.WriteString(string.Join('|', Information.GameRegions));                 // Game Regions
        update.WriteBool(Information.Ranked);                                          // Ranked
        update.WriteInt8(Information.MatchFidelity);                                   // Match Fidelity
        update.WriteInt8(Information.BotDifficulty);                                   // Bot Difficulty
        update.WriteBool(Information.RandomizeBots);                                   // Randomize Bots
        update.WriteString(string.Empty);                                              // Country Restrictions
        update.WriteString(string.Empty);                                              // Player Invitation Responses (Unused/Legacy)
        update.WriteInt8(Information.TeamSize);                                        // Team Size (e.g. 5 For Forests Of Caldavar, 3 For Grimm's Crossing)
        update.WriteInt8(Convert.ToByte(Information.GroupType));                       // Group Type

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

                // Calculate Rank Level From TMR (Campaign Level / Medal)
                int normalRankLevel = CalculateCampaignLevel(member.TMR);
                int casualRankLevel = CalculateCampaignLevel(member.CasualTMR);

                update.WriteInt32(normalRankLevel);                                      // Normal Rank Level (Campaign Level / Medal)
                update.WriteInt32(casualRankLevel);                                      // Casual Rank Level (Campaign Level / Medal)
                update.WriteInt32(normalRankLevel);                                      // Normal Rank (Global Ranking Index - Placeholder)
                update.WriteInt32(casualRankLevel);                                      // Casual Rank (Global Ranking Index - Placeholder)
                update.WriteBool(member.IsEligibleForMatchmaking);                       // Eligible For Campaign

                // Rating: Use -1 To Hide For Unranked, Otherwise Show Real Rating
                short displayRating = Information.Ranked
                    ? Convert.ToInt16(Math.Clamp(member.TMR, 0, short.MaxValue))
                    : (short)-1;
                update.WriteInt16(displayRating);                                        // Rating
            }

            update.WriteInt8(member.LoadingPercent);                                     // Loading Percent (0 to 100)
            update.WriteBool(member.IsReady);                                            // Ready Status
            update.WriteBool(member.IsInGame);                                           // In-Game Status

            if (fullGroupUpdate)
            {
                update.WriteBool(member.IsEligibleForMatchmaking);                       // Eligible For Matchmaking
                update.WriteString(member.Account.NameColourNoPrefixCode);               // Chat Name Colour
                update.WriteString(member.Account.IconNoPrefixCode);                     // Account Icon
                update.WriteString(member.Country);                                      // Country
                update.WriteBool(member.HasGameModeAccess);                              // Game Mode Access Bool
                update.WriteString(member.GameModeAccess);                               // Game Mode Access String
            }
        }

        if (fullGroupUpdate)
        {
            long sharedBufferLength = update.Size;

            foreach (MatchmakingGroupMember recipient in Members)
            {
                update.Resize(sharedBufferLength);

                foreach (MatchmakingGroupMember member in Members)
                {
                    bool isFriend = recipient.Session.IsFriendOrClanMember(member.Account.ID);

                    update.WriteBool(isFriend);
                }

                recipient.Session.Send(update);
            }
        }

        else
        {
            foreach (MatchmakingGroupMember member in Members)
                member.Session.Send(update);
        }
    }

    /// <summary>
    ///     Removes a member from the matchmaking group.
    ///     If the leader leaves and other members remain, leadership transfers to the member with the lowest slot index.
    ///     If the leader leaves and no other members remain, or if the last member leaves, the group is disbanded.
    /// </summary>
    public void RemoveMember(int accountID, bool kick = false)
    {
        MatchmakingGroupMember? memberToRemove = Members.SingleOrDefault(member => member.Account.ID == accountID);

        if (memberToRemove is null)
            return;

        bool memberToRemoveIsLeader = memberToRemove.IsLeader;

        // Broadcast Removal To All Members Before Removing
        ChatProtocol.TMMUpdateType updateType = kick
            ? ChatProtocol.TMMUpdateType.TMM_PLAYER_KICKED_FROM_GROUP
            : ChatProtocol.TMMUpdateType.TMM_PLAYER_LEFT_GROUP;

        // Send Partial Group Update Indicating Member Removal
        MulticastUpdate(accountID, updateType);

        // Remove Member From Group Chat Channel
        if (ChatChannel is not null)
        {
            ChatChannel.Members.TryRemove(memberToRemove.Account.Name, out _);

            memberToRemove.Session.CurrentChannels.Remove(ChatChannel.ID);
        }

        // Remove Member From Group
        Members.Remove(memberToRemove);

        // If Group Is Now Empty, Disband It
        if (Members.Count is 0)
        {
            // Remove The Leader's Entry From The Matchmaking Service (Groups Are Keyed By Leader ID)
            if (memberToRemoveIsLeader)
            {
                MatchmakingService.Groups.TryRemove(accountID, out _);
            }

            DisbandGroup();

            return;
        }

        // Leave Queue If Queued (Queue Membership Changes Require Re-Queuing)
        if (IsQueued)
        {
            LeaveQueue();
        }

        // Reassign Slots And Transfer Leadership
        ReassignSlots();

        // If The Leader Left, Re-Key The Group Under The New Leader's ID
        if (memberToRemoveIsLeader)
        {
            MatchmakingService.Groups.TryRemove(accountID, out _);
            MatchmakingService.Groups.TryAdd(Leader.Account.ID, this);
        }

        // Reset All Members To Default Readiness State: Leader = Not Ready, Others = Ready
        // Non-Leader Members Should Always Be Ready So That Group Readiness Is Determined Solely By The Leader
        foreach (MatchmakingGroupMember member in Members)
            member.IsReady = member.IsLeader is false;

        // Send Full Group Update To Remaining Members
        MulticastUpdate(accountID, ChatProtocol.TMMUpdateType.TMM_FULL_GROUP_UPDATE);
    }

    /// <summary>
    ///     Removes the specified member from the group and records the action as a kick.
    /// </summary>
    public void KickMember(int accountID) => RemoveMember(accountID, kick: true);

    /// <summary>
    ///     Removes the member at the specified team slot from the group and records the action as a kick.
    ///     Based on the original implementation which removes a player by team slot.
    /// </summary>
    public void KickMemberBySlot(byte teamSlot)
    {
        MatchmakingGroupMember? memberToKick = Members.SingleOrDefault(member => member.Slot == teamSlot);

        if (memberToKick is null)
        {
            Log.Warning(@"Attempted To Kick Non-Existent Slot {TeamSlot} From Matchmaking Group GUID ""{GroupGUID}""", teamSlot, GUID);

            return;
        }

        RemoveMember(memberToKick.Account.ID, kick: true);
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
            member.Slot = Convert.ToByte(Members.IndexOf(member) + 1);

        Members.Single(member => member.Slot is 1).IsLeader = true;
    }

    /// <summary>
    ///     Cleans up resources when disbanding the matchmaking group.
    ///     Called when the last member leaves. The groups registry entry is removed by the caller.
    /// </summary>
    private void DisbandGroup()
    {
        // Leave Queue If Queued
        if (IsQueued)
        {
            LeaveQueue();
        }

        // Remove All Members From Group Chat Channel And Clean Up
        if (ChatChannel is not null)
        {
            foreach (MatchmakingGroupMember member in Members)
            {
                ChatChannel.Members.TryRemove(member.Account.Name, out _);

                member.Session.CurrentChannels.Remove(ChatChannel.ID);
            }

            // Remove The Chat Channel If Empty
            if (ChatChannel.Members.IsEmpty)
            {
                Context.ChatChannels.TryRemove(ChatChannel.Name, out _);
            }
        }
    }

    /// <summary>
    ///     Leaves the matchmaking queue.
    /// </summary>
    public void LeaveQueue()
    {
        if (QueueStartTime is null)
            return;

        QueueStartTime = null;

        // Broadcast Queue Leave To All Group Members
        ChatBuffer leaveQueueBroadcast = new ();

        leaveQueueBroadcast.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_LEAVE_QUEUE);

        foreach (MatchmakingGroupMember member in Members)
            member.Session.Send(leaveQueueBroadcast);
    }

    /// <summary>
    ///     Calculates the campaign level (medal/rank) from TMR.
    ///     Based on the original campaign level thresholds.
    /// </summary>
    /// <param name="tmr">The player's Team Match Rating.</param>
    /// <returns>The campaign level (1-21).</returns>
    private static int CalculateCampaignLevel(double tmr)
    {
        // Campaign Levels Based On TMR Thresholds
        // Bronze 5-1: 0-999 (Levels 1-5)
        // Silver 5-1: 1000-1249 (Levels 6-10)
        // Gold 4-1: 1250-1449 (Levels 11-14)
        // Diamond 3-1: 1450-1599 (Levels 15-17)
        // Legendary 2-1: 1600-1749 (Levels 18-19)
        // Immortal: 1750+ (Level 20-21)

        return tmr switch
        {
            < 800  => 1,   // Bronze 5
            < 850  => 2,   // Bronze 4
            < 900  => 3,   // Bronze 3
            < 950  => 4,   // Bronze 2
            < 1000 => 5,   // Bronze 1
            < 1050 => 6,   // Silver 5
            < 1100 => 7,   // Silver 4
            < 1150 => 8,   // Silver 3
            < 1200 => 9,   // Silver 2
            < 1250 => 10,  // Silver 1
            < 1300 => 11,  // Gold 4
            < 1350 => 12,  // Gold 3
            < 1400 => 13,  // Gold 2
            < 1450 => 14,  // Gold 1
            < 1500 => 15,  // Diamond 3
            < 1550 => 16,  // Diamond 2
            < 1600 => 17,  // Diamond 1
            < 1700 => 18,  // Legendary 2
            < 1800 => 19,  // Legendary 1
            < 2000 => 20,  // Immortal
            _      => 21   // Immortal (Top Tier)
        };
    }

    /// <summary>
    ///     Checks if the group has excessive TMR disparity between members.
    ///     This prevents boosting/smurfing by rejecting groups where the highest-rated player is significantly above the average of the remaining members.
    /// </summary>
    /// <returns>
    ///     TRUE if disparity exceeds the threshold, FALSE otherwise.
    /// </returns>
    private bool HasExcessiveTMRDisparity()
    {
        if (Members.Count <= 1)
            return false;

        // Calculate The Team Approximation (Extrapolate To Full Team Size)
        double combinedTMR = TotalTMR;
        double averageTMR = AverageTMR;
        int teamSize = Information.TeamSize;

        // Approximate What The Full Team's TMR Would Be
        double teamApproximation = combinedTMR + averageTMR * (teamSize - Members.Count);

        // Calculate The Bottom N-1 Players' Combined TMR
        double bottomMembersTMR = teamApproximation - HighestTMR;

        // Check If The Highest Player Is Too Far Above The Average Of The Rest
        double averageOfOthers = bottomMembersTMR / (teamSize - 1);
        double disparity = HighestTMR - averageOfOthers;

        return disparity >= MaximumGroupTMRDisparity;
    }

    /// <summary>
    ///     Sends an error message to the group leader when queue join fails.
    /// </summary>
    /// <param name="errorMessage">The error message to display.</param>
    private void SendQueueJoinError(string errorMessage)
    {
        // Send A Generic Error Response To The Leader
        // This Uses The TMM_GENERIC_RESPONSE Message Type To Display An Error
        ChatBuffer error = new ();

        error.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GENERIC_RESPONSE);
        error.WriteInt8(0); // Error Code (0 = Generic Error)
        error.WriteString(errorMessage);

        Leader.Session.Send(error);

        Log.Information(@"Queue Join Rejected For Group GUID ""{Group.GUID}"": {Reason}", GUID, errorMessage);
    }
}
