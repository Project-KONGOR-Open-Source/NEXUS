namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_JOIN)]
public class GroupJoin(MerrickContext merrick, ILogger<GroupJoin> logger) : CommandProcessorsBase, ICommandProcessor
{
    private MerrickContext MerrickContext { get; set; } = merrick;

    private ILogger<GroupJoin> Logger { get; } = logger;

    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        GroupJoinRequestData requestData = new (buffer);

        MatchmakingGroup group = MatchmakingService.GetMatchmakingGroup(requestData.InviteIssuerName)
            ?? throw new NullReferenceException($@"No Matchmaking Group Found For Invite Issuer Name ""{requestData.InviteIssuerName}""");

        MatchmakingGroupMember newMatchmakingGroupMember = new (session)
        {
            Slot = Convert.ToByte(group.Members.Count + 1),
            IsLeader = false,
            IsReady = false,
            IsInGame = false,
            IsEligibleForMatchmaking = true,
            LoadingPercent = 0,
            HasGameModeAccess = true,
            GameModeAccess = group.Leader.GameModeAccess
        };

        if (group.Members.Any(member => member.Account.ID == session.ClientInformation.Account.ID) is false)
        {
            group.Members.Add(newMatchmakingGroupMember);
        }

        else
        {
            Logger.LogWarning("Player {Session.ClientInformation.Account.Name} Tried To Join A Matchmaking Group They Are Already In", session.ClientInformation.Account.Name);

            return;
        }

        Response.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE);

        ChatProtocol.TMMUpdateType updateType = ChatProtocol.TMMUpdateType.TMM_PLAYER_JOINED_GROUP;

        Response.WriteInt8(Convert.ToByte(updateType));                                     // Group Update Type
        Response.WriteInt32(session.ClientInformation.Account.ID);                          // Account ID
        Response.WriteInt8(Convert.ToByte(group.Members.Count));                            // Group Size
        // TODO: Calculate Average Group Rating
        Response.WriteInt16(1500);                                                          // Average Group Rating
        Response.WriteInt32(group.Leader.Account.ID);                                       // Leader Account ID
        // TODO: Dynamically Set Arranged Match Type From The Request Data
        Response.WriteInt8(Convert.ToByte(ChatProtocol.ArrangedMatchType.AM_MATCHMAKING));  // Arranged Match Type

        Response.WriteInt8(Convert.ToByte(group.Information.GameType));                     // Game Type
        Response.WriteString(group.Information.MapName);                                    // Map Name
        Response.WriteString(string.Join('|', group.Information.GameModes));                // Game Modes
        Response.WriteString(string.Join('|', group.Information.GameRegions));              // Game Regions
        Response.WriteBool(group.Information.Ranked);                                       // Ranked
        Response.WriteInt8(group.Information.MatchFidelity);                                // Match Fidelity
        Response.WriteInt8(group.Information.BotDifficulty);                                // Bot Difficulty
        Response.WriteBool(group.Information.RandomizeBots);                                // Randomize Bots
        Response.WriteString(string.Empty);                                                 // Country Restrictions (e.g. "AB->USE|XY->USW" Means Only Country "AB" Can Access Region "USE" And Only Country "XY" Can Access Region "USW")
        Response.WriteString("TODO: Find Out What Player Invitation Responses Do");         // Player Invitation Responses
        // TODO: Dynamically Set Team Size From The Request Data
        Response.WriteInt8(5);                                                              // Team Size (e.g. 5 For Forests Of Caldavar, 3 For Grimm's Crossing)
        Response.WriteInt8(Convert.ToByte(group.Information.GroupType));                    // Group Type

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
                Response.WriteInt32(member.Account.ID);                                     // Account ID
                Response.WriteString(member.Account.Name);                                  // Account Name
                Response.WriteInt8(member.Slot);                                            // Group Slot
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
                Response.WriteInt32(20);                                                      // Normal Rank Level (Also Known As Normal Campaign Level Or Medal)
                Response.WriteInt32(15);                                                      // Casual Rank Level (Also Known As Casual Campaign Level Or Medal)
                // TODO: Figure Out What These Ranks Are (Potentially Actual Global Ranking Index In Order Of Rating Descending, e.g. Highest Rating Is Rank 1)
                Response.WriteInt32(20);                                                      // Normal Rank
                Response.WriteInt32(15);                                                      // Casual Rank
                Response.WriteBool(true);                                                     // Eligible For Campaign
                // TODO: Can Be Set To -1 To Hide The Rating From Other Players For Unranked Game Modes
                Response.WriteInt16(1850);                                                    // Rating
            }

            Response.WriteInt8(member.LoadingPercent);                                      // Loading Percent (0 to 100)
            Response.WriteBool(member.IsReady);                                             // Ready Status
            Response.WriteBool(member.IsInGame);                                            // In-Game Status

            if (fullGroupUpdate)
            {
                Response.WriteBool(member.IsEligibleForMatchmaking);                        // Eligible For Matchmaking
                Response.WriteString(member.Account.ChatNameColour);                        // Chat Name Colour
                Response.WriteString(member.Account.Icon);                                  // Account Icon
                Response.WriteString(member.Country);                                       // Country
                Response.WriteBool(member.HasGameModeAccess);                               // Game Mode Access Bool
                Response.WriteString(member.GameModeAccess);                                // Game Mode Access String
            }
        }

        if (fullGroupUpdate)
        {
            foreach (MatchmakingGroupMember member in group.Members)
            {
                // TODO: Determine Friendship Status
                Response.WriteBool(false);                                                  // Is Friend
            }
        }

        Response.PrependBufferSize();

        // TODO: Create Tentative Group, And Only Create Actual Group When Another Player Joins

        // Broadcast To All Current Group Members That A New Player Has Joined
        Parallel.ForEach(group.Members, member => member.Session.SendAsync(Response.Data));
    }
}

public class GroupJoinRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();
    public string ClientVersion = buffer.ReadString();
    public string InviteIssuerName = buffer.ReadString();
}
