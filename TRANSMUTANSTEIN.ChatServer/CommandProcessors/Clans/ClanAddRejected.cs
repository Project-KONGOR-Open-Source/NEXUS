using TRANSMUTANSTEIN.ChatServer.Domain.Clans;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Clans;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_CLAN_ADD_REJECTED)]
public class ClanAddRejected(IPendingClanService pendingClanService) : IAsynchronousCommandProcessor<ChatSession>
{
    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        Account? account = session.Account;
        if (account == null)
        {
            return;
        }

        pendingClanService.RemoveObsoledPendingClanInvites();

        string? inviteKey = pendingClanService.GetPendingClanInviteKeyForUser(account);

        if (inviteKey != null)
        {
            PendingClanInvite? invite = pendingClanService.GetPendingClanInvite(inviteKey);

            if (invite != null)
            {
                // Notify Inviter
                ChatSession? inviterSession =
                    Context.ClientChatSessions.Values.FirstOrDefault(cs => cs.Account?.ID == invite.InitiatorAccountId);
                if (inviterSession != null)
                {
                    inviterSession.Send(new ClanAddRejectedResponse(account.Name));
                }
            }

            pendingClanService.RemovePendingClanInvite(inviteKey);
        }
    }
}