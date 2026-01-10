using TRANSMUTANSTEIN.ChatServer.Domain.Clans;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Clans;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_CLAN_ADD_MEMBER)]
public class ClanAddMember(MerrickContext merrick, IPendingClanService pendingClanService) : IAsynchronousCommandProcessor<ChatSession>
{
    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        Account? account = session.Account;
        if (account == null) return;

        // Skip Command ID (Header)
        buffer.ReadCommandBytes();

        string targetUsername = buffer.ReadString();

        // 1. Permissions Check
        // Requester must be in a clan and be Officer or Leader
        if (account.Clan == null || account.ClanTier < ClanTier.Officer)
        {
             session.Send(new ClanCreateFailResponse(ChatProtocol.Command.CHAT_CMD_CLAN_ADD_FAIL_PERMS));
             return;
        }

        // 2. Validate Target User
        Account? targetAccount = await merrick.Accounts
            .Include(a => a.Clan)
            .FirstOrDefaultAsync(a => a.Name == targetUsername);

        if (targetAccount == null)
        {
             Log.Warning("[CLAN-INVITE-DEBUG] FAIL_ONLINE: Target Username '{Name}' NOT FOUND in Database.", targetUsername);
             // Target does not exist (Legacy sends Offline error?)
             session.Send(new ClanCreateFailResponse(ChatProtocol.Command.CHAT_CMD_CLAN_ADD_FAIL_ONLINE));
             return;
        }
        
        // Target is already in a clan
        if (targetAccount.Clan != null)
        {
             session.Send(new ClanCreateFailResponse(ChatProtocol.Command.CHAT_CMD_CLAN_ADD_FAIL_CLAN));
             return;
        }

        // 3. Check Pending State
        pendingClanService.RemoveObsoledPendingClans();
        pendingClanService.RemoveObsoledPendingClanInvites();

        if (pendingClanService.IsUserInPendingClans(targetAccount) || 
            pendingClanService.GetPendingClanInviteKeyForUser(targetAccount) != null)
        {
             session.Send(new ClanCreateFailResponse(ChatProtocol.Command.CHAT_CMD_CLAN_ADD_FAIL_INVITED)); 
             return;
        }

        // 4. Check Online Status & Chat Mode
        ChatSession? targetSession = Context.ClientChatSessions.Values
                .FirstOrDefault(cs => cs.Account?.ID == targetAccount.ID);

        if (targetSession == null || targetSession.ClientMetadata.LastKnownClientState < ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_CONNECTED)
        {
             Log.Warning("[CLAN-INVITE-DEBUG] FAIL_ONLINE: Target {TargetID} ({TargetName}) not found in active sessions or not connected. Session Null? {SessionNull}. State: {State}", 
                 targetAccount.ID, targetAccount.Name, targetSession == null, targetSession?.ClientMetadata.LastKnownClientState);
             session.Send(new ClanCreateFailResponse(ChatProtocol.Command.CHAT_CMD_CLAN_ADD_FAIL_ONLINE));
             return;
        }

        if (targetSession.ClientMetadata.ClientChatModeState == ChatProtocol.ChatModeType.CHAT_MODE_DND)
        {
             Log.Warning("[CLAN-INVITE-DEBUG] FAIL_ONLINE: Target {TargetID} is in DND Mode.", targetAccount.ID);
             // Legacy sends AutoResponse DND
             // Subject.SendResponse(new ChatModeAutoResponse(invitedClient.ChatModeType, Username, invitedClient.ChatModeReason));
             // For now, fail with online error or specific DND if available. 
             // Legacy code actually sends `ChatModeAutoResponse`. I'll stick to FAIL_ONLINE for MVP or implement AutoResponse later.
             session.Send(new ClanCreateFailResponse(ChatProtocol.Command.CHAT_CMD_CLAN_ADD_FAIL_ONLINE));
             return; 
        }

        // 5. Create Pending Invite
        PendingClanInvite invite = new()
        {
             ClanName = account.Clan.Name,
             ClanTag = account.Clan.Tag,
             ClanId = account.Clan.ID,
             InitiatorAccountId = account.ID,
             InvitedAccountId = targetAccount.ID,
             CreationTime = DateTime.UtcNow
        };
        
        pendingClanService.InsertPendingClanInvite(targetUsername.ToLower(), invite);

        // 6. Notify Target
        targetSession.Send(new ClanAddMemberResponse(account.Name, account.Clan.Name));
    }
}
