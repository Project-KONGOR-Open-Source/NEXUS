namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Actions;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_TRACK_PLAYER_ACTION)]
public class TrackPlayerAction(MerrickContext merrick, ILogger<TrackPlayerAction> logger) : CommandProcessorsBase, ICommandProcessor
{
    private MerrickContext MerrickContext { get; set; } = merrick;
    private ILogger<TrackPlayerAction> Logger { get; set; } = logger;

    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        TrackPlayerActionRequestData requestData = new(buffer);

        // TODO: Do Something With This Data

        Logger.LogError($@"Unhandled User Action: ""{requestData.Action}""");
    }
}

public class TrackPlayerActionRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();
    public ChatProtocol.ActionCampaign Action = (ChatProtocol.ActionCampaign)buffer.ReadInt8();
}
