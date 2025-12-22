namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_CREATE)]
public class GroupCreate : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        GroupCreateRequestData requestData = new (buffer);

        MatchmakingGroup
            .Create(session, requestData.ToGroupInformation());
    }
}

file class GroupCreateRequestData
{
    public byte[] CommandBytes { get; init; }

    public string ClientVersion { get; init; }

    public ChatProtocol.TMMType GroupType { get; init; }

    public ChatProtocol.TMMGameType GameType { get; init; }

    public string MapName { get; init; }

    public string[] GameModes { get; init; }

    public string[] GameRegions { get; init; }

    public bool Ranked { get; init; }

    /// <summary>
    ///     0: Skill Disparity Will Be Higher But The Matchmaking Queue Time Will Be Shorter
    ///     <br/>
    ///     1: Skill Disparity Will Be Lower But The Matchmaking Queue Time Will Be Longer
    /// </summary>
    public byte MatchFidelity { get; init; }

    /// <summary>
    ///     1: Easy, 2: Medium, 3: Hard
    /// </summary>
    /// <remarks>
    ///     Only Used For Bot Matches, But Sent With Every Request To Create A Group
    /// </remarks>
    public byte BotDifficulty { get; init; }

    /// <remarks>
    ///     Only Used For Bot Matches, But Sent With Every Request To Create A Group
    /// </remarks>
    public bool RandomizeBots { get; init; }

    public GroupCreateRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ClientVersion = buffer.ReadString();
        GroupType = (ChatProtocol.TMMType) buffer.ReadInt8();
        GameType = (ChatProtocol.TMMGameType) buffer.ReadInt8();
        MapName = buffer.ReadString();
        GameModes = buffer.ReadString().Split('|', StringSplitOptions.RemoveEmptyEntries);
        GameRegions = buffer.ReadString().Split('|', StringSplitOptions.RemoveEmptyEntries);
        Ranked = buffer.ReadBool();
        MatchFidelity = buffer.ReadInt8();
        BotDifficulty = buffer.ReadInt8();
        RandomizeBots = buffer.ReadBool();
    }

    public MatchmakingGroupInformation ToGroupInformation()
    {
        return new MatchmakingGroupInformation()
        {

            ClientVersion = ClientVersion,
            GroupType = GroupType,
            GameType = GameType,
            MapName = MapName,
            GameModes = GameModes,
            GameRegions = GameRegions,
            Ranked = Ranked,
            MatchFidelity = MatchFidelity,
            BotDifficulty = BotDifficulty,
            RandomizeBots = RandomizeBots
        };
    }
}
