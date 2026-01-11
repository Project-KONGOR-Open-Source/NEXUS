namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

public class GroupCreateRequestData
{
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

    public byte[] CommandBytes { get; init; }

    public string ClientVersion { get; }

    public ChatProtocol.TMMType GroupType { get; }

    public ChatProtocol.TMMGameType GameType { get; }

    public string MapName { get; }

    public string[] GameModes { get; }

    public string[] GameRegions { get; }

    public bool Ranked { get; }

    /// <summary>
    ///     0: Skill Disparity Will Be Higher But The Matchmaking Queue Time Will Be Shorter
    ///     <br />
    ///     1: Skill Disparity Will Be Lower But The Matchmaking Queue Time Will Be Longer
    /// </summary>
    public byte MatchFidelity { get; }

    /// <summary>
    ///     1: Easy, 2: Medium, 3: Hard
    /// </summary>
    /// <remarks>
    ///     Only Used For Bot Matches, But Sent With Every Request To Create A Group
    /// </remarks>
    public byte BotDifficulty { get; }

    /// <remarks>
    ///     Only Used For Bot Matches, But Sent With Every Request To Create A Group
    /// </remarks>
    public bool RandomizeBots { get; }

    public MatchmakingGroupInformation ToGroupInformation()
    {
        return new MatchmakingGroupInformation
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