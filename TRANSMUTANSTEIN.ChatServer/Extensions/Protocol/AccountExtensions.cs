namespace TRANSMUTANSTEIN.ChatServer.Extensions.Protocol;

public static class AccountExtensions
{
    public static byte GetChatClientFlags(this Account account)
    {
        byte flags = 0x0;

        if (account.Type == AccountType.Staff)
        {
            flags |= Convert.ToByte(ChatProtocol.ChatClientType.CHAT_CLIENT_IS_STAFF);
        }

        if (account.ClanTier == ClanTier.Leader)
        {
            flags |= Convert.ToByte(ChatProtocol.ChatClientType.CHAT_CLIENT_IS_CLAN_LEADER);
        }

        if (account.ClanTier == ClanTier.Officer)
        {
            flags |= Convert.ToByte(ChatProtocol.ChatClientType.CHAT_CLIENT_IS_OFFICER);
        }

        // TODO: Do Something With ChatProtocol.ChatClientType.CHAT_CLIENT_IS_PREMIUM And ChatProtocol.ChatClientType.CHAT_CLIENT_IS_VERIFIED

        return flags;
    }
}
