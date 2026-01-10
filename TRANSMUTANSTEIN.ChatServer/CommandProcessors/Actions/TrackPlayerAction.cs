namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Actions;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_TRACK_PLAYER_ACTION)]
public class TrackPlayerAction : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        TrackPlayerActionRequestData requestData = new(buffer);

        // TODO: Do Something With This Data

        // TODO: Look Into Updating The Online Players Count And Matchmaking Details Using Custom Action OBTAIN_DETAILED_ONLINE_STATUS (Or Equivalent)

        switch (requestData.Action)
        {
            case ChatProtocol.ActionCampaign.AC_DAILY_LOGINS:
                // TODO: Handle Daily Logins Tracking Logic
                break;
            default:
                Log.Warning(@"Unhandled User Action: ""{RequestData.Action}""", requestData.Action);
                break;
        }
    }
}