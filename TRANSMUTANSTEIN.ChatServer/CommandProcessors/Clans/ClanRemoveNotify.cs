using TRANSMUTANSTEIN.ChatServer.Domain.Clans;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Clans;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_CLAN_REMOVE_NOTIFY)]
public class ClanRemoveNotify(MerrickContext merrick) : IAsynchronousCommandProcessor<ChatSession>
{
    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        Account? account = session.Account;
        if (account == null) return;

        int targetAccountId = buffer.ReadInt32(); // The account to remove (can be self)
        
        // 1. Fetch Target
        Account? targetAccount = await merrick.Accounts
             .Include(a => a.Clan)
             .FirstOrDefaultAsync(a => a.ID == targetAccountId);

        if (targetAccount == null || targetAccount.Clan == null)
        {
            return;
        }

        // 2. Validate Permissions
        // Case A: Self-Leave
        if (targetAccountId == account.ID)
        {
            // Allowed. Leader cannot leave? Should promote someone first or disband?
            // Legacy check: `if (leavingAccount.ClanTier == Clan.Tier.Leader)` -> return? 
            // Usually Leader must disband or transfer.
            if (targetAccount.ClanTier == ClanTier.Leader) 
            {
                 // Check if only member? If so, disband?
                 // For now, prevent Leader leaving to match legacy.
                 return;
            }
        }
        else
        {
            // Case B: Kick
            // Sender must be in same clan
            if (account.Clan == null || account.Clan.ID != targetAccount.Clan.ID) return;
            
            // Sender must be Leader or Officer
            if (account.ClanTier < ClanTier.Officer) return;
            
            // Sender must outrank target?
            // Officer cannot kick Officer. Leader can kick Officer.
            // Officer can kick Member.
            if (targetAccount.ClanTier >= account.ClanTier) return;
        }

        // 3. Remove from Clan (DB)
        int oldClanId = targetAccount.Clan.ID;
        string clanName = targetAccount.Clan.Name; // Cache name for channel lookup
        
        targetAccount.Clan = null;
        targetAccount.ClanTier = ClanTier.None;
        targetAccount.TimestampJoinedClan = null; // Clear timestamp?

        await merrick.SaveChangesAsync();

        // 4. Update Target Session if online
        ChatSession? targetSession = Context.ClientChatSessions.Values.FirstOrDefault(cs => cs.Account?.ID == targetAccountId);
        
        if (targetSession != null && targetSession.Account != null)
        {
            targetSession.Account.Clan = null;
            targetSession.Account.ClanTier = ClanTier.None;
            // Broadcast change (Name color/flags might change)
            targetSession.BroadcastConnectionStatusUpdate(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_CONNECTED); 
        }
        
        // 5. Broadcast to Clan Channel AND Remove Target from Channel
        ChatChannel? clanChannel = Context.ChatChannels.Values.FirstOrDefault(c => c.Name == $"Clan {clanName}");
        
        ClanRankChangeResponse rankChangeResponse = new(targetAccountId, ClanTier.None, account.ID);

        if (clanChannel != null)
        {
            // Broadcast first
            foreach (ChatChannelMember member in clanChannel.Members.Values)
            {
                 member.Session.Send(rankChangeResponse);
            }
            
            // Remove target
            if (targetSession != null)
            {
                clanChannel.Leave(targetSession);
            }
            // If target is offline but still in channel memory (unlikely for ChatSession-based channel), 
            // ChatChannel cleans up on disconnect.
        }
    }
}
