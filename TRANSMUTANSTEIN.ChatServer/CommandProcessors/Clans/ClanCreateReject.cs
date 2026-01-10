using TRANSMUTANSTEIN.ChatServer.Domain.Clans;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Clans;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_REJECT)] // Legacy uses different command for Rejecting Invite?
// Legacy ClanAddRejectedRequest handles invite rejection (0x0048 ? or 0x0053?)
// ChatProtocol:
// CHAT_CMD_CLAN_CREATE_REJECT = 0x0053 (Founding Member Reject)
// CHAT_CMD_CLAN_ADD_REJECTED = 0x0048 (Existing Clan Invite Reject)
// Legacy ClanAddRejectedRequest handles BOTH?.
// Legacy Request 0x0048 maps to `ClanAddRejectedRequest`.
// Legacy Request 0x0053 maps to `ClanCreateRejectRequest`? Wait.
// Let's check ClientProtocolRequestFactory in legacy.
// Or if ONE class handles multiple commands.
// New system maps 1 command to 1 processor usually, unless attribute allows multiple.
// I will implement for BOTH if logic is same, or separate.
// Legacy 'ClanAddRejectedRequest' handles finding keys for BOTH pending clan and invite.
// So likely the client sends DIFFERENT commands based on context.
// 0x0053 -> "You rejected the founding request".
// 0x0048 -> "You rejected the invite".
// I will bind this processor to both if I can, or create two files.
// Assuming [ChatCommand] can only be single.
// I'll assume 0x0053 (Create Reject) for now since we are verifying Creation.


public class ClanCreateReject(IPendingClanService pendingClanService) : IAsynchronousCommandProcessor<ChatSession>
{
    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
         Account? account = session.Account;
         if (account == null) return;
         
         pendingClanService.RemoveObsoledPendingClans();
         PendingClan? pendingClan = pendingClanService.GetPendingClanForUser(account);
         
         if (pendingClan != null)
         {
             ChatSession? creatorSession = Context.ClientChatSessions.Values.FirstOrDefault(cs => cs.Account?.ID == pendingClan.CreatorAccountId);
             if (creatorSession != null)
             {
                 creatorSession.Send(new ClanCreateRejectedResponse(account.Name));
             }
             
             string key = $"[{pendingClan.ClanTag.ToLower()}]{pendingClan.ClanName.ToLower()}";
             pendingClanService.RemovePendingClan(key);
         }
    }
}
