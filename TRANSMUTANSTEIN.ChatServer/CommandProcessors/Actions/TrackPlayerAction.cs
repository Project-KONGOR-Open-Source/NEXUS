namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Actions;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_TRACK_PLAYER_ACTION)]
public class TrackPlayerAction : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        TrackPlayerActionRequestData requestData = new (buffer);

        // TODO: Do Something With This Data

        // TODO: Look Into Updating The Online Players Count And Matchmaking Details Using Custom Action OBTAIN_DETAILED_ONLINE_STATUS (Or Equivalent)

        Log.Error(@"Unhandled User Action: ""{RequestData.Action}""", requestData.Action);
    }
}

public class TrackPlayerActionRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();

    public ChatProtocol.ActionCampaign Action = (ChatProtocol.ActionCampaign) buffer.ReadInt8();
}
