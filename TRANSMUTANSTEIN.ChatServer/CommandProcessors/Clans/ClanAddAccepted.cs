using TRANSMUTANSTEIN.ChatServer.Domain.Clans;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Clans;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_CLAN_ADD_ACCEPTED)]
public class ClanAddAccepted(MerrickContext merrick, IPendingClanService pendingClanService) : IAsynchronousCommandProcessor<ChatSession>
{
    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        Account? account = session.Account;
        if (account == null) return;

        pendingClanService.RemoveObsoledPendingClans();
        pendingClanService.RemoveObsoledPendingClanInvites();

        PendingClan? pendingClan = pendingClanService.GetPendingClanForUser(account);
        string? inviteKey = pendingClanService.GetPendingClanInviteKeyForUser(account);

        if (pendingClan != null)
        {
            // FOUNDING MEMBER ACCEPTANCE
            bool allMembersAccepted = true;
            
            int memberIndex = pendingClan.MembersAccountId.IndexOf(account.ID);
            if (memberIndex >= 0)
            {
                pendingClan.Accepted[memberIndex] = true;
                
                // Notify Creator
                ChatSession? creatorSession = Context.ClientChatSessions.Values.FirstOrDefault(cs => cs.Account?.ID == pendingClan.CreatorAccountId);
                if (creatorSession != null)
                {
                    creatorSession.Send(new ClanCreateAcceptedResponse(account.Name));
                }

                // Check if all accepted
                if (pendingClan.Accepted.Any(a => !a))
                {
                    allMembersAccepted = false;
                }
            }

            if (allMembersAccepted)
            {
                await CreateClan(merrick, pendingClan);
            }
        }
        else if (inviteKey != null)
        {
            // EXISTING CLAN INVITE ACCEPTANCE
            PendingClanInvite? invite = pendingClanService.GetPendingClanInvite(inviteKey);

            if (invite is null)
            {
                 Log.Warning("ClanAddAccepted: Invite Key {Key} found but Convert failed (Race Condition?)", inviteKey);
                 return;
            }
            
            // 1. Re-fetch Account to attach to current Context (Fix Persistence)
            Account? dbAccount = await merrick.Accounts.FirstOrDefaultAsync(a => a.ID == account.ID);
            if (dbAccount == null) return; // Should not happen

            // 2. Fetch Clan with Members
            Clan? clan = await merrick.Clans
                .Include(c => c.Members)
                .FirstOrDefaultAsync(c => c.ID == invite.ClanId);

            if (clan == null)
            {
                 pendingClanService.RemovePendingClanInvite(inviteKey);
                 session.Send(new ClanCreateFailResponse(ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_FAIL_CLAN)); // Generic error
                 return;
            }

            // 3. Update DB
            dbAccount.Clan = clan;
            dbAccount.ClanTier = ClanTier.Member;
            
            await merrick.SaveChangesAsync();
            
            // 4. Update Session Account Reference (So future commands see the change immediately)
            account.Clan = clan;
            account.ClanTier = ClanTier.Member;

            // 5. Join Clan Channel FIRST (Matches CreateClan pattern)
            ChatChannel clanChannel = ChatChannel.GetOrCreate(session, $"Clan {clan.Name}");
            clanChannel.Join(session);

            // 6. Notify Inviter (The one who sent the request) - "Request Accepted"
            // Find inviter session
            ChatSession? inviterSession = Context.ClientChatSessions.Values
                .FirstOrDefault(cs => cs.Account?.ID == invite.InitiatorAccountId);
            
            if (inviterSession != null)
            {
                // Send 0x004F to Inviter: "Your invite to [Name] was accepted"
                inviterSession.Send(new ClanAddAcceptedResponse(account.Name));
            }

            // 7. Notify User (Self) - "You joined"
            // Note: Sending this AFTER joining channel prevents potential state issues?
            session.Send(new ClanNewMemberResponse(account.ID, clan.ID, clan.Name, clan.Tag));
            
            // 8. Populate Roster (Send existing members to new user)
            foreach (Account member in clan.Members)
            {
                if (member.ID == account.ID) continue; // Skip self
                
                // Send "This person is a member" to ME
                session.Send(new ClanNewMemberResponse(member.ID, clan.ID, clan.Name, clan.Tag));
            }

            // 9. Notify Online Members "New user joined"
            ClanNewMemberResponse newMemberPacket = new(account.ID, clan.ID, clan.Name, clan.Tag);
            
            // Broadcast to all online clan members (EXCLUDING SELF)
            foreach (ChatSession clientSession in Context.ClientChatSessions.Values)
            {
                 if (clientSession.Account?.ID != account.ID && clientSession.Account?.Clan?.ID == clan.ID)
                 {
                      clientSession.Send(newMemberPacket);
                 }
            }

            // 10. Remove Invite
            pendingClanService.RemovePendingClanInvite(inviteKey);
            
            // 11. Broadcast Status Update (So others see ME as online)
            Log.Information($"[Clans] Broadcasting Online Status for {account.Name} and populating peer status list...");
            session.BroadcastConnectionStatusUpdate(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_CONNECTED);

            // 12. Receive Status (So I see others as online)
            session.ReceiveFriendAndClanMemberConnectionStatus();

            Log.Information("User {Name} accepted invite to Clan {Clan}", account.Name, clan.Name);
        }
    }

    private async Task CreateClan(MerrickContext merrick, PendingClan pendingClan)
    {
        Clan newClan = new()
        {
            Name = pendingClan.ClanName,
            Tag = pendingClan.ClanTag,
            TimestampCreated = DateTime.UtcNow
        };

        merrick.Clans.Add(newClan);
        // Note: Save immediately to get ClanID? Or add members then save?
        // EF Core can handle graph add. But we need to fetch existing accounts.
        
        List<int> allMemberIds = new List<int> { pendingClan.CreatorAccountId };
        allMemberIds.AddRange(pendingClan.MembersAccountId);
        
        List<Account> allAccounts = await merrick.Accounts
            .Where(a => allMemberIds.Contains(a.ID))
            .ToListAsync();
            
        foreach (Account acc in allAccounts)
        {
            acc.Clan = newClan;
            if (acc.ID == pendingClan.CreatorAccountId)
                acc.ClanTier = ClanTier.Leader;
            else
                acc.ClanTier = ClanTier.Officer;
        }

        await merrick.SaveChangesAsync();
        
        // Notify Creator
        ChatSession? creatorSession = Context.ClientChatSessions.Values.FirstOrDefault(cs => cs.Account?.ID == pendingClan.CreatorAccountId);

        // Notify All
        // Create Channel (In memory)
        // Ensure creator session has Clan data for GetOrCreate to detect it as Clan Channel
         if (creatorSession != null && creatorSession.Account != null) {
                creatorSession.Account.Clan = newClan;
                creatorSession.Account.ClanTier = ClanTier.Leader;
        }

        ChatChannel clanChannel = ChatChannel.GetOrCreate(creatorSession!, $"Clan {newClan.Name}");
        clanChannel.Topic = $"Welcome To The Clan {newClan.Name} Channel";
        
        // Add Creator to Channel
        if (creatorSession != null)
        {
             clanChannel.Join(creatorSession);
             
             creatorSession.Send(new ClanCreateCompleteResponse());
             creatorSession.Send(new ClanNewMemberResponse(pendingClan.CreatorAccountId, newClan.ID, pendingClan.ClanName, pendingClan.ClanTag));
        }
        
        // Notify Members
        foreach (int memberId in pendingClan.MembersAccountId)
        {
             ChatSession? memberSession = Context.ClientChatSessions.Values.FirstOrDefault(cs => cs.Account?.ID == memberId);
             if (memberSession != null)
             {
                 // Tell creator about this member
                 if (creatorSession != null) creatorSession.Send(new ClanNewMemberResponse(memberId, newClan.ID, pendingClan.ClanName, pendingClan.ClanTag));
                 
                  // Tell member about themselves and clan
                  memberSession.Send(new ClanNewMemberResponse(memberId, newClan.ID, pendingClan.ClanName, pendingClan.ClanTag));
                  
                  if (memberSession.Account != null) {
                      memberSession.Account.Clan = newClan;
                      memberSession.Account.ClanTier = ClanTier.Officer;
                  }
                  clanChannel.Join(memberSession);
                  
                  // Tell member about creator
                  memberSession.Send(new ClanNewMemberResponse(pendingClan.CreatorAccountId, newClan.ID, pendingClan.ClanName, pendingClan.ClanTag));
              }
        }
        
        string key = $"[{pendingClan.ClanTag.ToLower()}]{pendingClan.ClanName.ToLower()}";
        pendingClanService.RemovePendingClan(key);
    }
}
