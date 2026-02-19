namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Actions;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_TRACK_PLAYER_ACTION)]
public class TrackPlayerAction : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        TrackPlayerActionRequestData requestData = new (buffer);

        switch (requestData.Action)
        {
            // Daily Login Action
            case ChatProtocol.ActionCampaign.AC_DAILY_LOGINS:
                Log.Debug(@"Daily Login Action Received: ""{Action}"" From Account ""{AccountName}""", requestData.Action, session.Account.Name);
                break;

            // Analytics Events
            case ChatProtocol.ActionCampaign.AC_CLICKED_HON_STORE:
            case ChatProtocol.ActionCampaign.AC_CLICKED_BUY_COINS:
            case ChatProtocol.ActionCampaign.AC_CLICKED_MOTD_ADS:
            case ChatProtocol.ActionCampaign.AC_CLICKED_SPECIALS:
            case ChatProtocol.ActionCampaign.AC_CLICKED_EARLY_ACCESS:
            case ChatProtocol.ActionCampaign.AC_CLICKED_HEROES:
            case ChatProtocol.ActionCampaign.AC_CLICKED_ALT_AVATARS:
            case ChatProtocol.ActionCampaign.AC_CLICKED_ACCOUNT_ICONS:
            case ChatProtocol.ActionCampaign.AC_CLICKED_NAME_COLORS:
            case ChatProtocol.ActionCampaign.AC_CLICKED_SYMBOLS:
            case ChatProtocol.ActionCampaign.AC_CLICKED_TAUNT:
            case ChatProtocol.ActionCampaign.AC_CLICKED_ANNOUNCERS:
            case ChatProtocol.ActionCampaign.AC_CLICKED_COURIERS:
            case ChatProtocol.ActionCampaign.AC_CLICKED_OTHER:
            case ChatProtocol.ActionCampaign.AC_CLICKED_BUNDLES:
            case ChatProtocol.ActionCampaign.AC_CLICKED_OPEN_VAULT:
            case ChatProtocol.ActionCampaign.AC_CLICKED_CUSTOM_WARDS:
                Log.Debug(@"Analytics Action Received: ""{Action}"" From Account ""{AccountName}""", requestData.Action, session.Account.Name);
                break;

            // Additional Campaign Actions
            case ChatProtocol.ActionCampaign.AC_ADDITIONAL_CAMPAIGN_ONE:
            case ChatProtocol.ActionCampaign.AC_ADDITIONAL_CAMPAIGN_TWO:
            case ChatProtocol.ActionCampaign.AC_ADDITIONAL_CAMPAIGN_THREE:
            case ChatProtocol.ActionCampaign.AC_ADDITIONAL_CAMPAIGN_FOUR:
            case ChatProtocol.ActionCampaign.AC_ADDITIONAL_CAMPAIGN_FIVE:
                Log.Debug(@"Additional Campaign Action Received: ""{Action}"" From Account ""{AccountName}""", requestData.Action, session.Account.Name);
                break;

            // Matchmaking Actions
            case ChatProtocol.ActionCampaign.AC_MATCHMAKING_MATCH_FOUND:
            case ChatProtocol.ActionCampaign.AC_MATCHMAKING_SERVER_FOUND:
            case ChatProtocol.ActionCampaign.AC_MATCHMAKING_MATCH_READY:
            case ChatProtocol.ActionCampaign.AC_MATCHMAKING_SERVER_NOT_IDLE:
            case ChatProtocol.ActionCampaign.AC_MATCHMAKING_MATCH_READY_REMINDER:
                Log.Debug(@"Matchmaking Action Received: ""{Action}"" From Account ""{AccountName}""", requestData.Action, session.Account.Name);
                break;

            default:
                Log.Error(@"Unhandled User Action: ""{Action}"" From Account ""{AccountName}""", requestData.Action, session.Account.Name);
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
