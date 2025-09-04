namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_PLAYER_READY_STATUS)]
public class GroupPlayerReadyStatus(ILogger<GroupPlayerReadyStatus> logger) : CommandProcessorsBase, ICommandProcessor
{
    private ILogger<GroupPlayerReadyStatus> Logger { get; } = logger;

    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        GroupPlayerReadyStatusRequestData requestData = new (buffer);

        MatchmakingGroup group = MatchmakingService.GetMatchmakingGroup(session.ClientInformation.Account.ID)
            ?? throw new NullReferenceException($@"No Matchmaking Group Found For Invite Issuer ID ""{session.ClientInformation.Account.ID}""");

        group.Information.GameType = requestData.GameType;

        MatchmakingGroupMember groupMember = group.Members.Single(member => member.Account.ID == session.ClientInformation.Account.ID);

        if (groupMember.IsLeader is false)
            return; // Non-Leader Group Members Are Implicitly Ready (By Means Of Joining The Group In A Ready State) And Do Not Need To Emit Readiness Status Updates

        if (groupMember.IsReady is false)
        {
            foreach (MatchmakingGroupMember member in group.Members)
            {
                if (member.IsReady is false)
                {
                    if (member.IsLeader is false)
                        Logger.LogError(@"[BUG] Non-Leader Group Member ""{Member.Account.Name}"" With ID ""{Member.Account.ID}"" Was Not Ready", member.Account.Name, member.Account.ID);

                    member.IsReady = true;
                }
            }

            ChatBuffer update = new ();

            update.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE); // TODO: Make This DRY To Eliminate The Duplication

            ChatProtocol.TMMUpdateType updateType = ChatProtocol.TMMUpdateType.TMM_PARTIAL_GROUP_UPDATE;

            update.WriteInt8(Convert.ToByte(updateType));                                     // Group Update Type
            update.WriteInt32(session.ClientInformation.Account.ID);                          // Account ID
            update.WriteInt8(Convert.ToByte(group.Members.Count));                            // Group Size
            // TODO: Calculate Average Group Rating
            update.WriteInt16(1500);                                                          // Average Group Rating
            update.WriteInt32(group.Leader.Account.ID);                                       // Leader Account ID
            // TODO: Dynamically Set Arranged Match Type From The Request Data
            update.WriteInt8(Convert.ToByte(ChatProtocol.ArrangedMatchType.AM_MATCHMAKING));  // Arranged Match Type
            update.WriteInt8(Convert.ToByte(group.Information.GameType));                     // Game Type
            update.WriteString(group.Information.MapName);                                    // Map Name
            update.WriteString(string.Join('|', group.Information.GameModes));                // Game Modes
            update.WriteString(string.Join('|', group.Information.GameRegions));              // Game Regions
            update.WriteBool(group.Information.Ranked);                                       // Ranked
            update.WriteInt8(group.Information.MatchFidelity);                                // Match Fidelity
            update.WriteInt8(group.Information.BotDifficulty);                                // Bot Difficulty
            update.WriteBool(group.Information.RandomizeBots);                                // Randomize Bots
            update.WriteString(string.Empty);                                                 // Country Restrictions (e.g. "AB->USE|XY->USW" Means Only Country "AB" Can Access Region "USE" And Only Country "XY" Can Access Region "USW")
            update.WriteString("TODO: Find Out What Player Invitation Responses Do");         // Player Invitation Responses
            // TODO: Dynamically Set Team Size From The Request Data
            update.WriteInt8(5);                                                              // Team Size (e.g. 5 For Forests Of Caldavar, 3 For Grimm's Crossing)
            update.WriteInt8(Convert.ToByte(group.Information.GroupType));                    // Group Type

            bool fullGroupUpdate = updateType switch
            {
                ChatProtocol.TMMUpdateType.TMM_CREATE_GROUP             => true,
                ChatProtocol.TMMUpdateType.TMM_FULL_GROUP_UPDATE        => true,
                ChatProtocol.TMMUpdateType.TMM_PLAYER_JOINED_GROUP      => true,
                ChatProtocol.TMMUpdateType.TMM_PLAYER_LEFT_GROUP        => true,
                ChatProtocol.TMMUpdateType.TMM_PLAYER_KICKED_FROM_GROUP => true,
                _                                                       => false
            };

            foreach (MatchmakingGroupMember member in group.Members)
            {
                if (fullGroupUpdate)
                {
                    update.WriteInt32(member.Account.ID);                                     // Account ID
                    update.WriteString(member.Account.Name);                                  // Account Name
                    update.WriteInt8(member.Slot);                                            // Group Slot
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
                    update.WriteInt32(20);                                                      // Normal Rank Level (Also Known As Normal Campaign Level Or Medal)
                    update.WriteInt32(15);                                                      // Casual Rank Level (Also Known As Casual Campaign Level Or Medal)
                    // TODO: Figure Out What These Ranks Are (Potentially Actual Global Ranking Index In Order Of Rating Descending, e.g. Highest Rating Is Rank 1)
                    update.WriteInt32(20);                                                      // Normal Rank
                    update.WriteInt32(15);                                                      // Casual Rank
                    update.WriteBool(true);                                                     // Eligible For Campaign
                    // TODO: Can Be Set To -1 To Hide The Rating From Other Players For Unranked Game Modes
                    update.WriteInt16(1850);                                                    // Rating
                }

                update.WriteInt8(member.LoadingPercent);                                      // Loading Percent (0 to 100)
                update.WriteBool(member.IsReady);                                             // Ready Status
                update.WriteBool(member.IsInGame);                                            // In-Game Status

                if (fullGroupUpdate)
                {
                    update.WriteBool(member.IsEligibleForMatchmaking);                        // Eligible For Matchmaking
                    update.WriteString(member.Account.ChatNameColour);                        // Chat Name Colour
                    update.WriteString(member.Account.Icon);                                  // Account Icon
                    update.WriteString(member.Country);                                       // Country
                    update.WriteBool(member.HasGameModeAccess);                               // Game Mode Access Bool
                    update.WriteString(member.GameModeAccess);                                // Game Mode Access String
                }
            }

            if (fullGroupUpdate)
            {
                foreach (MatchmakingGroupMember member in group.Members)
                {
                    // TODO: Determine Friendship Status
                    update.WriteBool(false);                                                  // Is Friend
                }
            }

            update.PrependBufferSize();

            Parallel.ForEach(group.Members, member => member.Session.SendAsync(update.Data));
        }

        if (group.Members.All(member => member.IsReady))
        {
            ChatBuffer load = new ();

            load.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_START_LOADING);
            load.PrependBufferSize();

            Parallel.ForEach(group.Members, member => member.Session.SendAsync(load.Data));
        }
    }
}

public class GroupPlayerReadyStatusRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();
    public byte ReadyStatus = buffer.ReadInt8();
    public ChatProtocol.TMMGameType GameType = (ChatProtocol.TMMGameType) buffer.ReadInt8();
}
