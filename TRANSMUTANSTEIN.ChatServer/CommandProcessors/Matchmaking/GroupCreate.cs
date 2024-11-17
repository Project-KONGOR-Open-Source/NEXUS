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

        /*

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

        */
    }
}

public class GroupCreateRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();
    public string ClientVersion = buffer.ReadString();
    public ChatProtocol.TMMType GroupType = (ChatProtocol.TMMType)buffer.ReadInt8();
    public ChatProtocol.TMMGameType GameType = (ChatProtocol.TMMGameType)buffer.ReadInt8();
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
