namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_CREATE)]
public class GroupCreate(MerrickContext merrick, ILogger<GroupCreate> logger) : CommandProcessorsBase, ICommandProcessor
{
    private MerrickContext MerrickContext { get; set; } = merrick;

    private ILogger<GroupCreate> Logger { get; set; } = logger;

    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        GroupCreateRequestData requestData = new (buffer);

        if (Context.MatchmakingGroupChatChannels.ContainsKey(session.ClientInformation.Account.ID) is false)
        {
            MatchmakingGroupMember member = new (session)
            {
                Slot = 1,
                IsLeader = true,
                IsReady = false,
                IsInGame = false,
                IsEligibleForMatchmaking = true,
                LoadingPercent = 0,
                GameModeAccess = string.Join('|', requestData.GameModes.Select(mode => "true"))
            };

            MatchmakingGroupInformation information = new ()
            {
                ClientVersion = requestData.ClientVersion,
                GroupType = requestData.GroupType,
                GameType = requestData.GameType,
                MapName = requestData.MapName,
                GameModes = requestData.GameModes,
                GameRegions = requestData.GameRegions,
                Ranked = requestData.Ranked,
                MatchFidelity = requestData.MatchFidelity,
                BotDifficulty = requestData.BotDifficulty,
                RandomizeBots = requestData.RandomizeBots
            };

            if (MatchmakingService.Groups.ContainsKey(session.ClientInformation.Account.ID) is false)
            {
                if (MatchmakingService.Groups.TryAdd(session.ClientInformation.Account.ID, new MatchmakingGroup(member) { Information = information }) is false)
                {
                    Logger.LogError(@"Failed To Create Matchmaking Group For Account ID ""{Session.ClientInformation.Account.ID}""", session.ClientInformation.Account.ID);

                    // TODO: Respond With ChatProtocol.TMMFailedToJoinReason Or Similar (e.g. TMMFailedToCreate, If It Exists) Or Maybe Just Throw An Exception

                    return;
                }
            }

            else
            {
                if (MatchmakingService.Groups.TryUpdate(session.ClientInformation.Account.ID, new MatchmakingGroup(member) { Information = information }, MatchmakingService.Groups[session.ClientInformation.Account.ID]) is false)
                {
                    Logger.LogError(@"Failed To Update Matchmaking Group For Account ID ""{Session.ClientInformation.Account.ID}""", session.ClientInformation.Account.ID);

                    // TODO: Respond With ChatProtocol.TMMFailedToJoinReason Or Similar (e.g. TMMFailedToCreate, If It Exists) Or Maybe Just Throw An Exception

                    return;
                }
            }
        }

        Response.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE); // TODO: Make This DRY To Eliminate The Duplication

        ChatProtocol.TMMUpdateType updateType = ChatProtocol.TMMUpdateType.TMM_CREATE_GROUP;

        Response.WriteInt8(Convert.ToByte(updateType));                                     // Group Update Type
        Response.WriteInt32(session.ClientInformation.Account.ID);                          // Account ID
        Response.WriteInt8(1);                                                              // Group Size
        // TODO: Calculate Average Group Rating
        Response.WriteInt16(1500);                                                          // Average Group Rating
        Response.WriteInt32(session.ClientInformation.Account.ID);                          // Leader Account ID
        // TODO: Dynamically Set Arranged Match Type From The Request Data
        Response.WriteInt8(Convert.ToByte(ChatProtocol.ArrangedMatchType.AM_MATCHMAKING));  // Arranged Match Type
        Response.WriteInt8(Convert.ToByte(requestData.GameType));                           // Game Type
        Response.WriteString(requestData.MapName);                                          // Map Name
        Response.WriteString(string.Join('|', requestData.GameModes));                      // Game Modes
        Response.WriteString(string.Join('|', requestData.GameRegions));                    // Game Regions
        Response.WriteBool(requestData.Ranked);                                             // Ranked
        Response.WriteInt8(requestData.MatchFidelity);                                      // Match Fidelity
        Response.WriteInt8(requestData.BotDifficulty);                                      // Bot Difficulty
        Response.WriteBool(requestData.RandomizeBots);                                      // Randomize Bots
        Response.WriteString(string.Empty);                                                 // Country Restrictions (e.g. "AB->USE|XY->USW" Means Only Country "AB" Can Access Region "USE" And Only Country "XY" Can Access Region "USW")
        Response.WriteString("TODO: Find Out What Player Invitation Responses Do");         // Player Invitation Responses
        // TODO: Dynamically Set Team Size From The Request Data
        Response.WriteInt8(5);                                                              // Team Size (e.g. 5 For Forests Of Caldavar, 3 For Grimm's Crossing)
        Response.WriteInt8(Convert.ToByte(requestData.GroupType));                          // Group Type

        MatchmakingGroup group = MatchmakingService.Groups[session.ClientInformation.Account.ID];

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

        // Create A New Matchmaking Group With The Initiator As The Group Leader
        session.SendAsync(Response.Data);
    }
}

public class GroupCreateRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();
    public string ClientVersion = buffer.ReadString();
    public ChatProtocol.TMMType GroupType = (ChatProtocol.TMMType) buffer.ReadInt8();
    public ChatProtocol.TMMGameType GameType = (ChatProtocol.TMMGameType) buffer.ReadInt8();
    public string MapName = buffer.ReadString();
    public string[] GameModes = buffer.ReadString().Split('|', StringSplitOptions.RemoveEmptyEntries);
    public string[] GameRegions = buffer.ReadString().Split('|', StringSplitOptions.RemoveEmptyEntries);
    public bool Ranked = buffer.ReadBool();

    // 0: Skill Disparity Will Be Higher But The Matchmaking Queue Time Will Be Shorter
    // 1: Skill Disparity Will Be Lower But The Matchmaking Queue Time Will Be Longer
    public byte MatchFidelity = buffer.ReadInt8();

    // 1: Easy, 2: Medium, 3: Hard
    // Only Used For Bot Matches, But Sent With Every Request To Create A Group
    public byte BotDifficulty = buffer.ReadInt8();

    // Only Used For Bot Matches, But Sent With Every Request To Create A Group
    public bool RandomizeBots = buffer.ReadBool();
}
