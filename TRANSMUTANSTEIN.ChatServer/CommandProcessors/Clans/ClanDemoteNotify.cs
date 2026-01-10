using TRANSMUTANSTEIN.ChatServer.Domain.Clans;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Clans;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_CLAN_DEMOTE_NOTIFY)]
public class ClanDemoteNotify(MerrickContext merrick) : IAsynchronousCommandProcessor<ChatSession>
{
    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        Account? account = session.Account;
        if (account == null) return;

        int targetAccountId = buffer.ReadInt32();
        
        // 1. Validate Requester Permissions
        // Only Leader can demote Officer? 
        // Can Officer demote Member? Assuming yes if Officer > Member.
        if (account.Clan == null || account.ClanTier < ClanTier.Officer)
        {
            return;
        }

        // 2. Fetch Target
        Account? targetAccount = await merrick.Accounts
             .Include(a => a.Clan)
             .FirstOrDefaultAsync(a => a.ID == targetAccountId);

        if (targetAccount == null || targetAccount.Clan == null || targetAccount.Clan.ID != account.Clan.ID)
        {
            return;
        }

        // 3. Logic
        // Cannot demote Leader (Self?) or Higher Rank.
        if (targetAccount.ClanTier >= account.ClanTier) return; // Strict hierarchy check
        
        // If Target is Member, cannot demote further?
        if (targetAccount.ClanTier == ClanTier.None || targetAccount.ClanTier == ClanTier.Member) return;
        
        if (targetAccount.ClanTier == ClanTier.Officer)
        {
            // Officer -> Member
            targetAccount.ClanTier = ClanTier.Member;
        }

        await merrick.SaveChangesAsync();

        // 4. Update Target Session if online
        ChatSession? targetSession = Context.ClientChatSessions.Values.FirstOrDefault(cs => cs.Account?.ID == targetAccountId);
        if (targetSession != null && targetSession.Account != null)
        {
            targetSession.Account.ClanTier = targetAccount.ClanTier;
            targetSession.BroadcastConnectionStatusUpdate(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_CONNECTED); 
        }
        
        // 5. Broadcast to Clan Channel
        ChatChannel? clanChannel = Context.ChatChannels.Values.FirstOrDefault(c => c.Name == $"Clan {account.Clan.Name}");
        
        ClanRankChangeResponse rankChangeResponse = new(targetAccountId, targetAccount.ClanTier, account.ID);

        if (clanChannel != null)
        {
            foreach (ChatChannelMember member in clanChannel.Members.Values)
            {
                 member.Session.Send(rankChangeResponse);
            }
        }
    }
}
