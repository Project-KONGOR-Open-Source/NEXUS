namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_CREATE)]
public class GroupCreate : ISynchronousCommandProcessor
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        GroupCreateRequestData requestData = new (buffer);

        MatchmakingGroup
            .Create(session, requestData);
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
