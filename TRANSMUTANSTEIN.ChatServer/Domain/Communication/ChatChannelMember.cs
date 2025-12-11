namespace TRANSMUTANSTEIN.ChatServer.Domain.Communication;

public class ChatChannelMember(ClientChatSession session, ChatChannel chatChannel)
{
    public ClientChatSession Session = session;

    public ChatChannel ChatChannel = chatChannel;

    public Account Account => Session.Account;

    public ChatProtocol.ChatClientStatus ConnectionStatus => Session.Metadata.LastKnownClientState;

    public ChatProtocol.AdminLevel AdministratorLevel => GetAdministratorLevel();

    public bool IsAdministrator => GetAdministratorStatus();

    public DateTime? SilencedUntil { get; set; } = null;

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

    /// <summary>
    ///     Check if this member is currently silenced in the channel.
    /// </summary>
    /// <returns>TRUE if the member is silenced, FALSE otherwise.</returns>
    public bool IsSilenced()
    {
        // Staff Members Are Immune To Being Silenced
        if (AdministratorLevel is ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_STAFF)
        {
            return false;
        }

        if (SilencedUntil.HasValue)
        {
            // Check If Silence Has Expired
            if (DateTime.UtcNow > SilencedUntil.Value)
            {
                // Silence Has Expired, Clear The Property
                SilencedUntil = null;

                return false;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    ///     Check whether this member has elevated privileges (Leader, Administrator, Staff).
    /// </summary>
    public bool HasElevatedPrivileges()
        => AdministratorLevel is ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_LEADER
                              or ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_ADMINISTRATOR
                              or ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_STAFF;

    /// <summary>
    ///     Check whether this member has a higher administrator level than another member.
    /// </summary>
    public bool HasHigherAdministratorLevelThan(ChatChannelMember other)
    {
        return (AdministratorLevel, other.AdministratorLevel) switch
        {
            // Staff Outranks: Everyone
            (ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_STAFF, ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_ADMINISTRATOR)   => true,
            (ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_STAFF, ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_LEADER)          => true,
            (ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_STAFF, ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_OFFICER)         => true,
            (ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_STAFF, ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_NONE)            => true,

            // Administrator Outranks: Leader, Officer, None
            (ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_ADMINISTRATOR, ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_LEADER)  => true,
            (ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_ADMINISTRATOR, ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_OFFICER) => true,
            (ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_ADMINISTRATOR, ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_NONE)    => true,

            // Leader Outranks: Officer, None
            (ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_LEADER, ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_OFFICER)        => true,
            (ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_LEADER, ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_NONE)           => true,

            // Officer Outranks: None
            (ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_OFFICER, ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_NONE)          => true,

            // All Other Cases: No Higher Privilege
            _                                                                                                            => false
        };
    }
}
