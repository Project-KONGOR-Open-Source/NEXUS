using TRANSMUTANSTEIN.ChatServer.Domain.Clans;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Clans;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_CLAN_WHISPER)]
public class ClanWhisperRequest : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        buffer.ReadCommandBytes(); // Skip Header
        string message = buffer.ReadString();
        Account? account = session.Account;

        if (account == null || account.Clan == null)
        {
            session.Send(new ClanWhisperFailedResponse());
            return;
        }

        // Broadcast to all online clan members (excluding self)
        ClanWhisperResponse response = new(account.ID, message);

        // Iterate all online sessions to find clan members
        // Optimization: Could cache clan members list in ClanManager if scaling is needed.
        // Current approach is O(N) where N = online users. Acceptable for now.
        foreach (ChatSession clientSession in Context.ClientChatSessions.Values)
        {
            if (clientSession.Account != null &&
                clientSession.Account.Clan != null &&
                clientSession.Account.Clan.ID == account.Clan.ID &&
                clientSession.Account.ID != account.ID)
            {
                clientSession.Send(response);
            }
        }
    }
}