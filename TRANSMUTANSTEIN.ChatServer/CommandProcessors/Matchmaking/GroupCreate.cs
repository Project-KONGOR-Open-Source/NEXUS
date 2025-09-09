namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_CREATE)]
public class GroupCreate(MerrickContext merrick, ILogger<GroupCreate> logger) : CommandProcessorsBase, ICommandProcessor
{
    private MerrickContext MerrickContext { get; set; } = merrick;

    private ILogger<GroupCreate> Logger { get; set; } = logger;

    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        GroupCreateRequestData requestData = new (buffer);

        if (MatchmakingService.Groups.ContainsKey(session.ClientInformation.Account.ID) is false)
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

        MatchmakingGroup group = MatchmakingService.Groups[session.ClientInformation.Account.ID];

        group.MulticastUpdate(session.ClientInformation.Account.ID, ChatProtocol.TMMUpdateType.TMM_CREATE_GROUP);

        // TODO: Create Chat Channel For The Group
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

    /// <summary>
    ///     0: Skill Disparity Will Be Higher But The Matchmaking Queue Time Will Be Shorter
    ///     <br/>
    ///     1: Skill Disparity Will Be Lower But The Matchmaking Queue Time Will Be Longer
    /// </summary>
    public byte MatchFidelity = buffer.ReadInt8();

    /// <summary>
    ///     1: Easy, 2: Medium, 3: Hard
    /// </summary>
    /// <remarks>
    ///     Only Used For Bot Matches, But Sent With Every Request To Create A Group
    /// </remarks>
    public byte BotDifficulty = buffer.ReadInt8();

    /// <remarks>
    ///     Only Used For Bot Matches, But Sent With Every Request To Create A Group
    /// </remarks>
    public bool RandomizeBots = buffer.ReadBool();
}
