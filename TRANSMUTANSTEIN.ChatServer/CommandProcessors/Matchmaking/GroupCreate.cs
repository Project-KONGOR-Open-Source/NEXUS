namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_CREATE)]
public class GroupCreate(MerrickContext merrick, ILogger<GroupCreate> logger) : CommandProcessorsBase, ICommandProcessor
{
    private MerrickContext MerrickContext { get; set; } = merrick;
    private ILogger<GroupCreate> Logger { get; set; } = logger;

    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        GroupCreateRequestData requestData = new(buffer);

        // TODO: Perform Checks And Respond With ChatProtocol.TMMFailedToJoinReason If Needed

        if (Context.MatchmakingGroupChatChannels.ContainsKey(session.ClientInformation.Account.ID) is false)
            MatchmakingService.SoloPlayerGroups.TryAdd(session.ClientInformation.Account.ID, new MatchmakingGroup(new MatchmakingGroupMember { Rating = 1650.00f, IsLeader = true, IsReady = true }));

        // TODO: Set Actual Rating & Game Details

        // TODO: Add Some Flag For Queue State, For Groups Created In Advance

        Response.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE);

        ChatProtocol.TMMUpdateType updateType = ChatProtocol.TMMUpdateType.TMM_CREATE_GROUP;

        Response.WriteInt8(Convert.ToByte(updateType));    // Group Update Type
        Response.WriteInt32(session.ClientInformation.Account.ID);                          // Account ID
        Response.WriteInt8(1);                                                              // Group Size
        // TODO: Calculate Average TMR 
        Response.WriteInt16(1500);                                                          // Average TMR
        Response.WriteInt32(session.ClientInformation.Account.ID);                          // Leader Account ID
        // TODO: Dynamically Set Arranged Match Type From The Request Data
        Response.WriteInt8(Convert.ToByte(ChatProtocol.ArrangedMatchType.AM_MATCHMAKING));  // Arranged Match Type
        Response.WriteInt8(Convert.ToByte(requestData.GameType));                           // Game Type
        Response.WriteString(requestData.MapName);                                          // Map Name
        Response.WriteString(string.Join('|', requestData.GameModes));                      // Game Modes
        Response.WriteString(string.Join('|', requestData.GameRegions));                    // Regions
        Response.WriteBool(requestData.Ranked);                                             // Ranked
        Response.WriteBool(requestData.MatchFidelity);                                      // Match Fidelity
        Response.WriteInt8(requestData.BotDifficulty);                                      // Bot Difficulty
        Response.WriteBool(requestData.RandomizeBots);                                      // Randomize Bots
        Response.WriteString(string.Empty);                                                 // Country Restrictions (e.g. "AB->USE|XY->USW" means only country "AB" can access region "USE" and only country "XY" can access region "USW")
        Response.WriteString("TODO: Find Out What Player Invitation Responses Do");         // Player Invitation Responses
        // TODO: Dynamically Set Team Size From The Request Data
        Response.WriteInt8(5);                                                              // Team Size (e.g. 5 for Forests Of Caldavar, 3 for Grimm's Crossing)
        Response.WriteInt8(Convert.ToByte(requestData.GroupType));                          // Group Type (e.g. 2 for multiplayer group, 3 for bot match group)

        MatchmakingGroup group = MatchmakingService.Groups.Single(group => group.Key == session.ClientInformation.Account.ID).Value;

        if (updateType is not ChatProtocol.TMMUpdateType.TMM_PARTIAL_GROUP_UPDATE)
        {
            foreach (MatchmakingGroupMember member in group.Members)
            {
                Response.WriteInt32(member.a);                               // Client's Account ID
                Response.WriteString(member.Account.NameWithClanTag);                 // Client's Name
                Response.WriteInt8(Convert.ToByte(participant.Slot));                       // Client's Team Slot
                Response.WriteInt32(1500);                                                  // Client's Normal Rank Level
                Response.WriteInt32(1500);                                                  // Client's Casual Rank Level
                Response.WriteInt32(5);                                                     // Client's Normal Ranking
                Response.WriteInt32(5);                                                     // Client's Casual Ranking
            }
        }


        /*


    private readonly List<GroupParticipant> GroupParticipants;
    private readonly byte[] FriendshipStatus;




        0x0D03 - NET_CHAT_CL_TMM_GROUP_UPDATE - Gives the client updated information on their matchmaking group.
Format:
	[1] ETMMUpdateType - update type. Valid types:
		TMM_CREATE_GROUP: The group has been created.
		TMM_FULL_GROUP_UPDATE: Full group update.
		TMM_PARTIAL_GROUP_UPDATE: Partial group update.
		TMM_PLAYER_JOINED_GROUP: A player has joined the group.
		TMM_PLAYER_LEFT_GROUP: A player has left the group.
		TMM_PLAYER_KICKED_FROM_GROUP: A player was kicked from the group.
	[4] unsigned long - account ID (if update is for a specific client, this is the client's account ID)
	[1] unsigned char - number of clients in group
	[2] unsigned short - average MMR of clients in group
	[4] unsigned long - group leader's account ID
	[1] EArrangedMatchType - arranged match type
	[1] ETMMGameTypes - game type
	[X] string - map name. Valid names:
		"caldavar"
		"grimmscrossing"
		"midwars"
	[X] string - game modes, delimited by | (e.g "ap|sd"). Case sensitive. Valid modes:
		"ap" - All Pick
		"apg" - All Pick Core Pool
		"sd" - Single Draft
		"bd" - Banning Draft
		"bp" - Banning Pick
		"ar" - All Random
		"lp" - Lock Pick
		"bb" - Blind Ban (Midwars mode)
		"bbg" - Blind Ban Core Pool
		"bm" - Bot Match
		"cm" - Captain's Pick
		"br" - Balanced Random
	[X] string - server regions, delimited by | (e.g. "VN|DX"). Case sensitive. Valid regions:
		"USE" - US East
		"USW" - US West
		"EU" - Europe
		"SG" - Singapore
		"MY" - Malaysia
		"PH" - Philippines
		"TH" - Thailand
		"ID" - Indonesia
		"VN" - Vietnam
		"RU" - Russia
		"KR" - Korea
		"AU" - Australia
		"LAT" - Latin America
		"DX" - China DX region
		"LT" - China LT region
	[1] bool - ranked
	[1] bool - match fidelity
	[1] unsigned char - bot difficulty
	[1] bool - randomize bots
	[X] string - country restrictions (e.g. "AB->USE|XY->USW" means only country "AB" can access region "USE" and only country "XY" can access region "USW")
	[X] string - player response string (TODO: this has information on player invites)
	[1] unsigned char - matchmaking team size (e.g. 5 for Forests of Caldavar, 3 for Grimm's Crossing)
	[1] unsigned char - group type. Valid types:
		2: multiplayer group
		3: bot match group
	for each client in the group
		if (update type != TMM_PARTIAL_GROUP_UPDATE)
			[4] unsigned long - client's account ID
			[X] string - client's name
			[1] unsigned char - client's team slot
			[2] unsigned short - client's MMR (USHORT_MAX if unranked or Midwars)
		[1] unsigned char - loading percent (0 to 100)
		[1] bool - ready
		[1] bool - in game
		if (update type != TMM_PARTIAL_GROUP_UPDATE)
			[1] bool - eligible for ranked matchmaking
			[X] string - client's chat name color
			[X] string - client's account icon
			[X] string - client's country
			[1] bool - client has access to all of the group's game modes
			[X] string - client's game mode access, delimited by "|" (e.g. "true|true|false")
	if (update type != TMM_PARTIAL_GROUP_UPDATE)
		for each client in the group
			[1] bool - client is a buddy
			
Notes:
- This packet is sent whenever group information changes.

        MatchmakingGroupUpdateResponse matchmakingGroupUpdateResponse = new MatchmakingGroupUpdateResponse(
            updateType: Convert.ToByte(updateType),
            accountId: removedOrKickedAccountId,
            groupSize: Convert.ToByte(participantsListCopy.Count),
            averageTMR: Convert.ToInt16(1500),
            leaderAccountId: Participants[0].AccountId,
            unknown1: 1, // 1 for ranked, 4 midwars, 5 bot, 7 riftwars? Possibly wrong.
            gameType: GameType,
            mapName: GetMapName(GameType),
            gameModes: GameModes,
            regions: Regions,
            ranked: Ranked,
            matchFidelity: MatchFidelity,
            botDifficulty: BotDifficulty,
            randomizeBots: RandomizeBots,
            unknown2: "",
            playerInvitationResponses: "", // seems important.
            teamSize: 5, // max number of players?
            groupType: GroupType,
            groupParticipants: CreateGroupParticipants(),
            friendshipStatus: friendshipStatus
        );

           private List<MatchmakingGroupUpdateResponse.GroupParticipant> CreateGroupParticipants()
           {
               List<MatchmakingGroupUpdateResponse.GroupParticipant> groupParticipants = new();
               for (int i = 0; i < Participants.Count; ++i)
               {
                   groupParticipants.Add(CreateGroupParticipant(i, Participants[i]));
               }
               return groupParticipants;
           }

           private MatchmakingGroupUpdateResponse.GroupParticipant CreateGroupParticipant(int slot, Participant participant)
           {
               bool isReady;
               if (participant == Participants[0])
               {
                   // Leader is Ready when they advance the state.
                   isReady = State != GroupState.WaitingToStart;
               }
               else
               {
                   // Non-Leaders are always ready.
                   isReady = true;
               }
               return new MatchmakingGroupUpdateResponse.GroupParticipant(
                   AccountId: participant.AccountId,
                   Name: participant.Name,
                   Slot: Convert.ToByte(slot),
                   NormalRankLevel: 1500,
                   CasualRankLevel: 1500,
                   NormalRanking: 5,
                   CasualRanking: 5,
                   EligibleForCampaign: 1,
                   Rating: 1500,
                   LoadingPercent: participant.LoadingStatus,
                   ReadyStatus: Convert.ToByte(isReady),
                   InGame: 0,
                   Verified: 1,
                   ChatNameColor: participant.ChatNameColor, // not sure if this works.
                   AccountIcon: participant.AccountIcon,
                   Country: "US", // ??
                   GameModeAccessBool: 1,
                   GameModeAccessString: GameModes // game modes that user can access?
               );
           }

           if (participantCount == 0)
           {
               // First connected Account become the Leader.
               BroadcastUpdate(MatchmakingGroup.GroupUpdateType.GroupCreated);
           }
           else
           {
               // Create a chat channel for the group if there are more than one Participants.
               if (ChatChannel == null)
               {
                   string channelName = "Group #" + GroupId;
                   ChatChannel? chatChannel = KongorContext.ChatChannels.ChatChannelByName(channelName);
                   ChatServerProtocol.ChatChannelFlags channelFlags = ChatServerProtocol.ChatChannelFlags.CannotBeJoined;
                   if (chatChannel != null)
                   {
                       chatChannel.RemoveEveryone();
                       chatChannel.Flags = channelFlags;
                   }
                   else
                   {
                       chatChannel = new ChatChannel(channelName, "TMM Group Chat", channelFlags);
                       KongorContext.ChatChannels.Add(chatChannel);
                   }

                   ChatChannel = chatChannel;
                   chatChannel.AddAccountIds(Participants.Select(participant => participant.AccountId));
               }
               else
               {
                   // ChatChannel already exists, join it.
                   ChatChannel.AddAccount(account, KongorContext.ConnectedClients[account.AccountId]);
               }

               BroadcastUpdate(MatchmakingGroup.GroupUpdateType.Full);
           }

           if (State == GroupState.LoadingResources)
           {
               // Update LoadingProgress progress.
               BroadcastUpdate(GroupUpdateType.Partial);
           }

        namespace KONGOR.ChatServer.Matchmaking;

public class MatchmakingGroupUpdateResponse : ProtocolResponse
{
    // Taked from matchmaking.lua and is here for reference.

    local function TMMPlayerStatus(sourceWidget, playerID, ...)
		0 = uiAccountID
		1 = sName
		2 = ySlot
		3 = nRating
		4 = yLoadingPercent
		5 = yReadyStatus
		6 = isLeader
		7 = isValidIndex
		8 = bVerified
		9 = bFriend
		10 = uiChatNameColorString
		11 = GetChatNameColorTexturePath(uiChatNameColorString)
		12 = bGameModeAccess
		13 = GetChatNameGlow(uiChatNameColor)
		14 = sGameModeAccess
		15 = Ingame
		16 = GetChatNameGlowColorString
		17 = GetChatNameGlowColorIngameString
		18 = Normal RankLevel
		19 = Casual RankLevel
		20 = Normal Ranking
		21 = Casual Ranking
		22 = bEligibleForCampaign

    local function TMMDisplay(sourceWidget, ...)
		-- for player slots 1 - 5
		0 = Slot Account ID
		1 = Slot Username
		2 = Slot number
		3 = Slot TMR
		4 = Player Loading TMM Status | Player Ready Status
		--
		25 = Update type
		26 = group size
		27 = average TMR
		28 = leader account id
		29 = game type
		30 = MapName
		31 = GameModes
		32 = Regions
		33 = TSNULL
		34 = PlayerInvitationResponses
		35 = TeamSize
		36 = TSNULL
		37 = Verified
		38 = VerifiedOnly
		39 = BotDifficulty
		40 = RandomizeBots
		41 = GroupType

    public record GroupParticipant(
        int AccountId,
        string Name,
        byte Slot,
        int NormalRankLevel,
        int CasualRankLevel,
        int NormalRanking,
        int CasualRanking,
        byte EligibleForCampaign,
        short Rating,
        byte LoadingPercent,
        byte ReadyStatus,
        byte InGame,
        byte Verified,
        string ChatNameColor,
        string AccountIcon, // Icon:Slot format.
        string Country,
        byte GameModeAccessBool,
        string GameModeAccessString
    );

    private readonly byte UpdateType;
    private readonly int AccountId;
    private readonly byte GroupSize;
    private readonly short AverageTMR;
    private readonly int LeaderAccountId;
    private readonly byte Unknown1;
    private readonly TMMGameType GameType;
    private readonly string MapName;
    private readonly string GameModes;
    private readonly string Regions;
    private readonly byte Ranked;
    private readonly byte MatchFidelity;
    private readonly byte BotDifficulty;
    private readonly byte RandomizeBots;
    private readonly string Unknown2;
    private readonly string PlayerInvitationResponses;
    private readonly byte TeamSize;
    private readonly byte GroupType;
    private readonly List<GroupParticipant> GroupParticipants;
    private readonly byte[] FriendshipStatus;

    public MatchmakingGroupUpdateResponse(byte updateType, int accountId, byte groupSize, short averageTMR, int leaderAccountId, byte unknown1, TMMGameType gameType, string mapName, string gameModes, string regions, byte ranked, byte matchFidelity, byte botDifficulty, byte randomizeBots, string unknown2, string playerInvitationResponses, byte teamSize, byte groupType, List<GroupParticipant> groupParticipants, byte[] friendshipStatus)
    {
        UpdateType = updateType;
        AccountId = accountId;
        GroupSize = groupSize;
        AverageTMR = averageTMR;
        LeaderAccountId = leaderAccountId;
        Unknown1 = unknown1;
        GameType = gameType;
        MapName = mapName;
        GameModes = gameModes;
        Regions = regions;
        Ranked = ranked;
        MatchFidelity = matchFidelity;
        BotDifficulty = botDifficulty;
        RandomizeBots = randomizeBots;
        Unknown2 = unknown2;
        PlayerInvitationResponses = playerInvitationResponses;
        TeamSize = teamSize;
        GroupType = groupType;
        GroupParticipants = groupParticipants;
        FriendshipStatus = friendshipStatus;
    }

    public override CommandBuffer Encode()
    {
        CommandBuffer buffer = new();
        buffer.WriteInt16(ChatServerProtocol.MatchmakingTwoWay.GroupUpdate);
        buffer.WriteInt8(UpdateType);
        buffer.WriteInt32(AccountId);
        buffer.WriteInt8(GroupSize);
        buffer.WriteInt16(AverageTMR);
        buffer.WriteInt32(LeaderAccountId);
        buffer.WriteInt8(Unknown1);
        buffer.WriteInt8((byte)GameType);
        buffer.WriteString(MapName);
        buffer.WriteString(GameModes);
        buffer.WriteString(Regions);
        buffer.WriteInt8(Ranked);
        buffer.WriteInt8(MatchFidelity);
        buffer.WriteInt8(BotDifficulty);
        buffer.WriteInt8(RandomizeBots);
        buffer.WriteString(Unknown2);
        buffer.WriteString(PlayerInvitationResponses);
        buffer.WriteInt8(TeamSize);
        buffer.WriteInt8(GroupType);

        bool sendAllData = (MatchmakingGroup.GroupUpdateType)UpdateType switch
        {
            MatchmakingGroup.GroupUpdateType.GroupCreated => true,
            MatchmakingGroup.GroupUpdateType.Full => true,
            MatchmakingGroup.GroupUpdateType.ParticipantAdded => true,
            MatchmakingGroup.GroupUpdateType.ParticipantRemoved => true,
            MatchmakingGroup.GroupUpdateType.ParticipantKicked => true,
            _ => false,
        };

        foreach (GroupParticipant participant in GroupParticipants)
        {
            if (sendAllData)
            {
                buffer.WriteInt32(participant.AccountId);
                buffer.WriteString(participant.Name);
                buffer.WriteInt8(participant.Slot);
                buffer.WriteInt32(participant.NormalRankLevel);
                buffer.WriteInt32(participant.CasualRankLevel);
                buffer.WriteInt32(participant.NormalRanking);
                buffer.WriteInt32(participant.CasualRanking);
                buffer.WriteInt8(participant.EligibleForCampaign);
                buffer.WriteInt16(participant.Rating);
            }

            buffer.WriteInt8(participant.LoadingPercent);
            buffer.WriteInt8(participant.ReadyStatus);
            buffer.WriteInt8(participant.InGame);

            if (sendAllData)
            {
                buffer.WriteInt8(participant.Verified);
                buffer.WriteString(participant.ChatNameColor);
                buffer.WriteString(participant.AccountIcon);
                buffer.WriteString(participant.Country);
                buffer.WriteInt8(participant.GameModeAccessBool);
                buffer.WriteString(participant.GameModeAccessString);
            }
        }

        if (sendAllData)
        {
            foreach (byte isFriend in FriendshipStatus)
            {
                buffer.WriteInt8(isFriend);
            }
        }

        return buffer;
    }
}


        */
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

    // If TRUE, Skill Disparity Will Be Lower But The Matchmaking Queue Time Will Be Longer
    public bool MatchFidelity = buffer.ReadBool();

    // 1: Easy, 2: Medium, 3: Hard
    // Only Used For Bot Matches, But Sent With Every Request To Create A Group
    public byte BotDifficulty = buffer.ReadInt8();

    // Only Used For Bot Matches, But Sent With Every Request To Create A Group
    public bool RandomizeBots = buffer.ReadBool();
}
