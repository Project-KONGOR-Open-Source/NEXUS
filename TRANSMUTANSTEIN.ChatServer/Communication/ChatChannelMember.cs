namespace TRANSMUTANSTEIN.ChatServer.Communication;

public class ChatChannelMember(ChatSession session, ChatChannel chatChannel)
{
    public ChatSession Session = session;

    public ChatChannel ChatChannel = chatChannel;

    public Account Account => Session.ClientInformation.Account;

    public ChatProtocol.ChatClientStatus ConnectionStatus => Session.ClientInformation.LastKnownClientState;

    public ChatProtocol.AdminLevel AdministratorLevel => GetAdministratorLevel();

    public bool IsAdministrator => GetAdministratorStatus();

    private bool GetAdministratorStatus()
    {
        return AdministratorLevel switch
        {
            ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_NONE          => false,
            ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_OFFICER       => true,
            ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_LEADER        => true,
            ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_ADMINISTRATOR => true,
            ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_STAFF         => true,
            _                                                       => throw new ArgumentOutOfRangeException(@$"Unsupported Administrator Level ""{AdministratorLevel}""")
        };
    }

    private ChatProtocol.AdminLevel GetAdministratorLevel()
    {
        if (Account.Type is AccountType.Staff)
            return ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_STAFF;

        if (Account.Clan is not null && ChatChannel.Name == Account.Clan.GetChatChannelName())
        {
            return Account.ClanTier switch
            {
                ClanTier.Leader  => ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_LEADER,
                ClanTier.Officer => ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_OFFICER,
                ClanTier.Member  => ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_NONE,
                ClanTier.None    => ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_NONE,
                _                => throw new ArgumentOutOfRangeException(@$"Unsupported Clan Tier ""{Account.ClanTier}""")
            };
        }

        return ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_NONE;
    }
}
