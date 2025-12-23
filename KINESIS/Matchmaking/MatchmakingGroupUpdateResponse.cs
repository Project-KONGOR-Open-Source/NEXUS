namespace KINESIS.Matchmaking;

public class MatchmakingGroupUpdateResponse : ProtocolResponse
{
    // Taked from matchmaking.lua and is here for reference.
    /*
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
    */

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
    private readonly GameFinder.TMMGameType GameType;
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
    private readonly MatchmakingGroup.Participant[] GroupParticipants;
    private readonly byte[] FriendshipStatus;
    private readonly byte _readyStatus;
    public byte ReadyStatus => _readyStatus;

    public MatchmakingGroupUpdateResponse(byte updateType, int accountId, byte groupSize, short averageTMR, int leaderAccountId, byte unknown1, GameFinder.TMMGameType gameType, string mapName, string gameModes, string regions, byte ranked, byte matchFidelity, byte botDifficulty, byte randomizeBots, string unknown2, string playerInvitationResponses, byte teamSize, byte groupType, MatchmakingGroup.Participant[] groupParticipants, byte[] friendshipStatus, bool loadingResources)
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
        _readyStatus = Convert.ToByte(loadingResources);
    }

    public override CommandBuffer Encode()
    {
        CommandBuffer buffer = new();
        buffer.WriteInt16(ChatServerResponse.MatchmakingGroupUpdated);
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

        for (byte i = 0; i < GroupParticipants.Length; ++i)
        {
            MatchmakingGroup.Participant participant = GroupParticipants[i];
            if (sendAllData)
            {
                buffer.WriteInt32(participant.AccountId);
                buffer.WriteString(participant.ClientInformation.DisplayedName);
                buffer.WriteInt8(i);
                buffer.WriteInt32(/* NormalRankLevel */ 0);
                buffer.WriteInt32(/* CasualRankLevel */ 0);
                buffer.WriteInt32(/* NormalRanking */ 0);
                buffer.WriteInt32(/* CasualRanking */ 0);
                buffer.WriteInt8(/* EligibleForCampaign */ 1);
                buffer.WriteInt16(/* Rating */ 0);
            }

            buffer.WriteInt8(participant.LoadingStatus);
            buffer.WriteInt8(/* ReadyStatus */ _readyStatus);
            buffer.WriteInt8(/* InGame */ 0);

            if (sendAllData)
            {
                buffer.WriteInt8(/* Verified */ 1);
                buffer.WriteString(participant.ClientInformation.SelectedChatNameColourCode);
                buffer.WriteString(participant.ClientInformation.SelectedAccountIconCode);
                buffer.WriteString(/* Country */ "US");
                buffer.WriteInt8(/* GameModeAccessBool */ 1);
                buffer.WriteString(/* GameModeAccessString */ "");
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
