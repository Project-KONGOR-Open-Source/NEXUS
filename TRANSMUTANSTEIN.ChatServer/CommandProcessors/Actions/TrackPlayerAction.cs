namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Actions;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_TRACK_PLAYER_ACTION)]
public class TrackPlayerAction : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        TrackPlayerActionRequestData requestData = new (buffer);

        // Handle The Action Based On Type
        switch (requestData.Action)
        {
            // Matchmaking Actions - These Are Analytics Events From The Client, No Response Required
            case ChatProtocol.ActionCampaign.AC_MATCHMAKING_MATCH_FOUND:
            case ChatProtocol.ActionCampaign.AC_MATCHMAKING_SERVER_FOUND:
            case ChatProtocol.ActionCampaign.AC_MATCHMAKING_MATCH_READY:
            case ChatProtocol.ActionCampaign.AC_MATCHMAKING_SERVER_NOT_IDLE:
            case ChatProtocol.ActionCampaign.AC_MATCHMAKING_MATCH_READY_REMINDER:
                Log.Debug(@"Matchmaking Action Received: ""{Action}"" From Account ""{AccountName}""", requestData.Action, session.Account.Name);
                break;

            // TODO: Handle Other Actions As Needed
            default:
                Log.Warning(@"Unhandled User Action: ""{Action}"" From Account ""{AccountName}""", requestData.Action, session.Account.Name);
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
