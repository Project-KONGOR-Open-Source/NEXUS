using TRANSMUTANSTEIN.ChatServer.Domain.Clans;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Clans;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_CLAN_REMOVE_NOTIFY)]
public class ClanRemoveNotify(MerrickContext merrick) : IAsynchronousCommandProcessor<ChatSession>
{
    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        Account? account = session.Account;
        if (account == null) return;

        // SKIP RAW PACKET DEBUG - REVERTING TO BASICS
        
        // REVERT: Was reading Command Bytes here. Now reading Int32 directly (Old Behavior)
        int targetAccountId = buffer.ReadInt32(); // The account to remove (can be self)

        Log.Information("[CLAN] ClanRemoveNotify: Account {RequesterID} ({RequesterName}) attempting to remove/leave Target {TargetID} (Legacy Read Mode)", 
            account.ID, account.Name, targetAccountId);
        
        // 1. Fetch Target
        Account? targetAccount = await merrick.Accounts
             .Include(a => a.Clan)
             .FirstOrDefaultAsync(a => a.ID == targetAccountId);

        if (targetAccount == null)
        {
             Log.Warning("[CLAN] ClanRemoveNotify Failed: Target Account {TargetID} not found.", targetAccountId);
             return;
        }
        
        if (targetAccount.Clan == null)
        {
             Log.Warning("[CLAN] ClanRemoveNotify Failed: Target Account {TargetID} is not in a clan.", targetAccountId);
             return;
        }

        // 2. Validate Permissions
        // Case A: Self-Leave
        if (targetAccountId == account.ID)
        {
            if (targetAccount.ClanTier == ClanTier.Leader) 
            {
                 Log.Warning("[CLAN] ClanRemoveNotify Failed: Leader {TargetID} cannot leave without disbanding/transferring.", targetAccountId);
                 return;
            }
            Log.Information("[CLAN] ClanRemoveNotify: User {TargetID} is leaving the clan (Self-Remove).", targetAccountId);
        }
        else
        {
            // Case B: Kick
            // Sender must be in same clan
            if (account.Clan == null || account.Clan.ID != targetAccount.Clan.ID)
            {
                Log.Warning("[CLAN] ClanRemoveNotify Failed: Requester Clan {ReqClan} != Target Clan {TgtClan}.", account.Clan?.ID, targetAccount.Clan.ID);
                return;
            }
            
            // Sender must be Leader or Officer
            if (account.ClanTier < ClanTier.Officer)
            {
                Log.Warning("[CLAN] ClanRemoveNotify Failed: Requester {RequesterID} Tier {Tier} is too low to kick (Requires Officer+).", account.ID, account.ClanTier);
                return;
            }
            
            // Sender must outrank target?
            if (targetAccount.ClanTier >= account.ClanTier)
            {
                Log.Warning("[CLAN] ClanRemoveNotify Failed: Requester Tier {ReqTier} cannot kick Target Tier {TgtTier}.", account.ClanTier, targetAccount.ClanTier);
                return;
            }
            Log.Information("[CLAN] ClanRemoveNotify: User {RequesterID} is KICKING {TargetID}.", account.ID, targetAccountId);
        }

        // 3. Remove from Clan (DB)
        int oldClanId = targetAccount.Clan.ID;
        string clanName = targetAccount.Clan.Name; // Cache name for channel lookup
        
        targetAccount.Clan = null;
        targetAccount.ClanTier = ClanTier.None;
        targetAccount.TimestampJoinedClan = null; 

        await merrick.SaveChangesAsync();
        Log.Information("[CLAN] Database Updated. Target {TargetID} removed from clan.", targetAccountId);

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
            Log.Information("[CLAN] Broadcasting Removal to Channel {ChannelName}.", clanChannel.Name);
            // Broadcast first
            foreach (ChatChannelMember member in clanChannel.Members.Values)
            {
                 member.Session.Send(rankChangeResponse);
            }
            
            // Remove target
            if (targetSession != null)
            {
                Log.Information("[CLAN] Removing Target Session from Channel.");
                clanChannel.Leave(targetSession);
            }
        }
        else
        {
            Log.Warning("[CLAN] Clan Channel not found for broadcast.");
        }
    }
}
