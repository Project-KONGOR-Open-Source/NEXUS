namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Actions;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_TRACK_PLAYER_ACTION)]
public class TrackPlayerAction : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        TrackPlayerActionRequestData requestData = new (buffer);

        // TODO: Do Something With This Data

        // TODO: Look Into Updating The Online Players Count And Matchmaking Details Using Custom Action OBTAIN_DETAILED_ONLINE_STATUS (Or Equivalent)

        switch (requestData.Action)
        {
            case ChatProtocol.ActionCampaign.AC_DAILY_LOGINS:
                // TODO: Implement Logic To Track Daily Logins
                Log.Debug(@"Received User Action: ""{Action}""", requestData.Action);
                break;

            case ChatProtocol.ActionCampaign.AC_CLICKED_HON_STORE:
                // TODO: Implement Logic To Track Store Clicks
                Log.Debug(@"Received User Action: ""{Action}""", requestData.Action);
                break;

            default:
                Log.Error(@"Unhandled User Action: ""{Action}""", requestData.Action);
                break;
        }
    }
}

file class TrackPlayerActionRequestData
{
    public byte[] CommandBytes { get; init; }

    public ChatProtocol.ActionCampaign Action { get; init; }

    public TrackPlayerActionRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        Action = (ChatProtocol.ActionCampaign) buffer.ReadInt8();
    }
}
