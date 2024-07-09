namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_CREATE)]
public class GroupCreate(MerrickContext merrick, ILogger<GroupCreate> logger) : CommandProcessorsBase, ICommandProcessor
{
    private MerrickContext MerrickContext { get; set; } = merrick;
    private ILogger<GroupCreate> Logger { get; set; } = logger;

    public async Task Process(TCPSession session, ChatBuffer buffer)
    {
        GroupCreateRequestData requestData = new(buffer);

        // TODO: Perform Checks And Respond With ChatProtocol.TMMFailedToJoinReason If Needed
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
