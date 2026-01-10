using TRANSMUTANSTEIN.ChatServer.Domain.Clans;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Clans;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_CLAN_ADD_ACCEPTED)]
public class ClanAddAccepted(MerrickContext merrick, IPendingClanService pendingClanService) : IAsynchronousCommandProcessor<ChatSession>
{
    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        try 
        {
            Account? account = session.Account;
            if (account == null) return;

            Log.Information("[ClanAddAccepted] START processing for Account {AccountID} ({Name})", account.ID, account.Name);

            pendingClanService.RemoveObsoledPendingClans();
            pendingClanService.RemoveObsoledPendingClanInvites();

            PendingClan? pendingClan = pendingClanService.GetPendingClanForUser(account);
            string? inviteKey = pendingClanService.GetPendingClanInviteKeyForUser(account);

            if (pendingClan != null)
            {
                Log.Information("[ClanAddAccepted] Founding Member Path (Creator)");
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
                    Log.Information("[ClanAddAccepted] All members accepted. Creating Clan...");
                    await CreateClan(merrick, pendingClan);
                }
            }
            else if (inviteKey != null)
            {
                Log.Information("[ClanAddAccepted] Existing Invite Path. Key: {Key}", inviteKey);
                // EXISTING CLAN INVITE ACCEPTANCE
                PendingClanInvite? invite = pendingClanService.GetPendingClanInvite(inviteKey);

                if (invite is null)
                {
                     Log.Warning("[ClanAddAccepted] Invite Key {Key} found but Convert failed (Race Condition?)", inviteKey);
                     return;
                }
                
                // 1. Re-fetch Account
                Account? dbAccount = await merrick.Accounts.FirstOrDefaultAsync(a => a.ID == account.ID);
                if (dbAccount == null) return; 

                // 2. Fetch Clan with Members
                Log.Information("[ClanAddAccepted] Fetching Clan {ClanID}", invite.ClanId);
                Clan? clan = await merrick.Clans
                    .Include(c => c.Members)
                    .FirstOrDefaultAsync(c => c.ID == invite.ClanId);

                if (clan == null)
                {
                     Log.Warning("[ClanAddAccepted] Clan {ClanID} not found.", invite.ClanId);
                     pendingClanService.RemovePendingClanInvite(inviteKey);
                     session.Send(new ClanCreateFailResponse(ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_FAIL_CLAN)); 
                     return;
                }

                Log.Information("[ClanAddAccepted] Clan Found: {ClanName} ({Tag}). Members: {Count}", clan.Name, clan.Tag, clan.Members.Count);

                // 3. Update DB
                dbAccount.Clan = clan;
                dbAccount.ClanTier = ClanTier.Member;
                
                await merrick.SaveChangesAsync();
                Log.Information("[ClanAddAccepted] DB Updated.");
                
                // 4. Update Session Account Reference
                account.Clan = clan;
                account.ClanTier = ClanTier.Member;

                // 5. Join Clan Channel
                Log.Information("[ClanAddAccepted] Joining Channel 'Clan {ClanName}'", clan.Name);
                ChatChannel clanChannel = ChatChannel.GetOrCreate(session, $"Clan {clan.Name}");
                clanChannel.Join(session);

                // 6. Notify Inviter
                Log.Information("[ClanAddAccepted] Notifying Inviter {InviterID}", invite.InitiatorAccountId);
                ChatSession? inviterSession = Context.ClientChatSessions.Values
                    .FirstOrDefault(cs => cs.Account?.ID == invite.InitiatorAccountId);
                
                if (inviterSession != null)
                {
                    inviterSession.Send(new ClanAddAcceptedResponse(account.Name));
                }

                // 8. Send "You Joined Clan" Response to Self (Legacy sends NewMemberResponse Long Packet to self)
                // FIX: Do NOT send 0x004F (ClanAddAcceptedResponse), that is C2S. Send 0x004E (NetMember) instead.
                Log.Information("[ClanAddAccepted] Sending LONG ClanNewMemberResponse (0x004E) to Self.");
                session.Send(new ClanNewMemberResponse(account.ID, clan.ID, clan.Name, clan.Tag));
                
                // 8. Roster Population
                // REMOVED: Legacy does not send individual NewMember packets for existing members.
                // The client populates the list via InitialStatus or ClanList request.
                // Sending them causes "[PK]MODERATOR has joined" spam.

                // 9. Notify Online Members "New user joined"
                // REVERTED: User confirmed Long Packet was working for Name Display. Short Packet caused failure.
                Log.Information("[ClanAddAccepted] Broadcasting LONG NewMemberResponse to Clanmates.");
                ClanNewMemberResponse newMemberPacketLong = new(account.ID, clan.ID, clan.Name, clan.Tag); 
                
                foreach (ChatSession clientSession in Context.ClientChatSessions.Values)
                {
                     // Check if in same clan and NOT self
                     if (clientSession.Account?.ID != account.ID && clientSession.Account?.Clan?.ID == clan.ID)
                     {
                          // Send Long Packet
                          clientSession.Send(newMemberPacketLong);
                     }
                }

                // 10. Remove Invite
                pendingClanService.RemovePendingClanInvite(inviteKey);
                
                // 11. Broadcast Status Update
                Log.Information($"[Clans] Broadcasting Online Status for {account.Name}...");
                session.BroadcastConnectionStatusUpdate(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_CONNECTED);

                // 12. Receive Status
                session.ReceiveFriendAndClanMemberConnectionStatus();

                Log.Information("User {Name} accepted invite to Clan {Clan}. DONE.", account.Name, clan.Name);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ClanAddAccepted] CRITICAL ERROR/CRASH");
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
