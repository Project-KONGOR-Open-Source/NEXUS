using TRANSMUTANSTEIN.ChatServer.Domain.Clans;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Clans;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_CLAN_PROMOTE_NOTIFY)]
public class ClanPromoteNotify(MerrickContext merrick) : IAsynchronousCommandProcessor<ChatSession>
{
    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        Account? account = session.Account;
        if (account == null) return;

        int targetAccountId = buffer.ReadInt32();
        
        // 1. Validate Requester Permissions
        if (account.Clan == null || account.ClanTier < ClanTier.Officer)
        {
            // Legacy apparently returns nothing on failure here? Or sends generic fail?
            // "ClanPromoteNotifyRequest" just returns.
            return;
        }

        // 2. Fetch Target
        // Must include Clan to verify they are in same clan
        Account? targetAccount = await merrick.Accounts
             .Include(a => a.Clan)
             .FirstOrDefaultAsync(a => a.ID == targetAccountId);

        if (targetAccount == null || targetAccount.Clan == null || targetAccount.Clan.ID != account.Clan.ID)
        {
            return;
        }

        // 3. Logic
        // Legacy: Cannot promote Leader.
        if (targetAccount.ClanTier >= ClanTier.Leader) return;

        bool isLeaderReferenceUpdate = false;
        Account? oldLeaderAccount = null;

        if (targetAccount.ClanTier == ClanTier.Member)
        {
            // Member -> Officer (Only Leader? Or Officer can promote Member? Legacy allowed specific logic?)
            // Legacy: if (account.Clan == null || promotedAccount.Clan == null || promotedAccount.ClanTier == Clan.Tier.Leader) return;
            // It allows Officer to promote? Assuming Officer can promote Member.
            // Wait, usually Officer can invite but NOT promote. Leader promotes.
            // Legacy code DOES NOT explicitly check if requester is Leader. 
            // BUT usually only Leader sends this command?
            // Let's assume strict: Only Leader can promote to Officer?
            // "if (account.ClanTier < ClanTier.Leader)" -> Return?
            // Re-reading legacy: "if (account.Clan == null || promotedAccount.Clan == null || promotedAccount.ClanTier == Clan.Tier.Leader) return;"
            // It doesn't check requester tier strictly for Member->Officer? 
            // My code line 23 checks `ClanTier < ClanTier.Officer`. So Member cannot promote.
            // Officer promoting Member might be allowed.
            
            targetAccount.ClanTier = ClanTier.Officer;
        }
        else if (targetAccount.ClanTier == ClanTier.Officer)
        {
            // Officer -> Leader. Only Leader can do this.
            if (account.ClanTier != ClanTier.Leader) return;

            targetAccount.ClanTier = ClanTier.Leader;
            
            // Demote requester (Old Leader) logic
            // Legacy handles "connectedClientPastLeader".
            // We need to update DB for requester too.
            // session.Account is cached object? Or EF tracked due to context scope?
            // session.Account is tracked if loaded from same context? No, session.Account is usually set on Connect.
            // Re-fetch requester from context to ensure tracking.
            Account? requesterDb = await merrick.Accounts.FindAsync(account.ID);
            if (requesterDb != null)
            {
                requesterDb.ClanTier = ClanTier.Officer;
                oldLeaderAccount = requesterDb;
                
                // Update Session
                if (session.Account != null) session.Account.ClanTier = ClanTier.Officer;
            }
            
            isLeaderReferenceUpdate = true;
        }

        await merrick.SaveChangesAsync();

        // 4. Update Target Session if online
        ChatSession? targetSession = Context.ClientChatSessions.Values.FirstOrDefault(cs => cs.Account?.ID == targetAccountId);
        if (targetSession != null && targetSession.Account != null)
        {
            targetSession.Account.ClanTier = targetAccount.ClanTier;
            // Notify Status Changed?
            // BroadcastConnectionStatusUpdate
            targetSession.BroadcastConnectionStatusUpdate(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_CONNECTED); // Re-broadcast flags
        }
        
        // 5. Broadcast to Clan Channel (Wait, Legacy iterates ALL clan members connected)
        // Legacy: "foreach (Account clanAccount in allClanMembers)... clanClient.SendResponse..."
        
        // We can broadcast to Clan Channel? 
        // Or get all sessions in CLan.
        // Using ChatChannel members might be easier if everyone is in it.
        
        ChatChannel? clanChannel = Context.ChatChannels.Values.FirstOrDefault(c => c.Name == $"Clan {account.Clan.Name}");
        
        // Construct keys/responses
        ClanRankChangeResponse rankChangeResponse = new(targetAccountId, targetAccount.ClanTier, account.ID);
        ClanRankChangeResponse? leaderDemoteResponse = null;
        if (isLeaderReferenceUpdate && oldLeaderAccount != null)
        {
             leaderDemoteResponse = new(oldLeaderAccount.ID, ClanTier.Officer, account.ID);
        }

        if (clanChannel != null)
        {
            foreach (ChatChannelMember member in clanChannel.Members.Values)
            {
                 member.Session.Send(rankChangeResponse);
                 if (leaderDemoteResponse != null) member.Session.Send(leaderDemoteResponse);
            }
        }
    }
}
