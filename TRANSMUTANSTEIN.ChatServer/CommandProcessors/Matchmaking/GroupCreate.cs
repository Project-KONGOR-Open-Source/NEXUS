namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_CREATE)]
public class GroupCreate(MerrickContext merrick, ILogger<GroupCreate> logger) : CommandProcessorsBase, ICommandProcessor
{
    private MerrickContext MerrickContext { get; set; } = merrick;
    private ILogger<GroupCreate> Logger { get; set; } = logger;

    public async Task Process(TCPSession session, ChatBuffer buffer)
    {
        byte[] _ = buffer.ReadCommandBytes();
        string clientVersion = buffer.ReadString();
        ChatProtocol.TMMTypes groupType = (ChatProtocol.TMMTypes) buffer.ReadInt8();
        ChatProtocol.TMMGameTypes gameType = (ChatProtocol.TMMGameTypes) buffer.ReadInt8(); // TODO: Rename These Enums To Be In The Singular Form
        string mapName = buffer.ReadString();
        string[] gameModes = buffer.ReadString().Split('|', StringSplitOptions.RemoveEmptyEntries);
        string[] gameRegions = buffer.ReadString().Split('|', StringSplitOptions.RemoveEmptyEntries);
        bool ranked = buffer.ReadBool();

        // If TRUE, Skill Disparity Will Be Lower But The Matchmaking Queue Time Will Be Longer
        bool matchFidelity = buffer.ReadBool();

        // 1: Easy, 2: Medium, 3: Hard
        // Only Used For Bot Matches, But Sent With Every Request To Create A Group
        byte botDifficulty = buffer.ReadInt8();

        // Only Used For Bot Matches, But Sent With Every Request To Create A Group
        bool randomizeBots = buffer.ReadBool();

        // TODO: Perform Checks And Respond With ChatProtocol.TMMFailedToJoinReason If Needed
    }
}
