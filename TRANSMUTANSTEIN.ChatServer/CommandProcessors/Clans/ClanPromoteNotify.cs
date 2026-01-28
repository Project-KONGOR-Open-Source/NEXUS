using global::TRANSMUTANSTEIN.ChatServer.Domain.Clans;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Clans;

using global::TRANSMUTANSTEIN.ChatServer.Internals;
using global::TRANSMUTANSTEIN.ChatServer.Domain.Communication;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_CLAN_PROMOTE_NOTIFY)]
public class ClanPromoteNotify(MerrickContext merrick, IChatContext chatContext) : IAsynchronousCommandProcessor<ChatSession>
{
    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        Account? account = session.Account;
        if (account == null)
        {
            return;
        }

        // RAW PACKET DEBUG: Read first int (TargetID?)
        byte[] rawBytes = buffer.Peek(4);
        string rawHex = BitConverter.ToString(rawBytes);
        int targetAccountId = buffer.ReadInt32();

        Log.Information("[CLAN-DEBUG] ClanPromoteNotify RAW: First 4 bytes: {Hex}. Read Int: {Value}", rawHex,
            targetAccountId);

        Log.Information(
            "[CLAN] ClanPromoteNotify: Account {RequesterID} ({RequesterName}) attempting to promote Target {TargetID}",
            account.ID, account.Name, targetAccountId);

        // 1. Validate Requester Permissions
        if (account.Clan == null)
        {
            Log.Warning("[CLAN] ClanPromoteNotify Failed: Requester {RequesterID} is not in a clan.", account.ID);
            return;
        }

        if (account.ClanTier < ClanTier.Officer)
        {
            Log.Warning(
                "[CLAN] ClanPromoteNotify Failed: Requester {RequesterID} has insufficient tier {Tier}. Requires Officer+.",
                account.ID, account.ClanTier);
            return;
        }

        // 2. Fetch Target
        Account? targetAccount = await merrick.Accounts
            .Include(a => a.Clan)
            .FirstOrDefaultAsync(a => a.ID == targetAccountId);

        if (targetAccount == null)
        {
            Log.Warning("[CLAN] ClanPromoteNotify Failed: Target Account {TargetID} not found.", targetAccountId);
            return;
        }

        if (targetAccount.Clan == null)
        {
            Log.Warning("[CLAN] ClanPromoteNotify Failed: Target Account {TargetID} is not in a clan.",
                targetAccountId);
            return;
        }

        if (targetAccount.Clan.ID != account.Clan.ID)
        {
            Log.Warning(
                "[CLAN] ClanPromoteNotify Failed: Target {TargetID} (Clan {TargetClanID}) is not in Requester's Clan {RequesterClanID}.",
                targetAccountId, targetAccount.Clan.ID, account.Clan.ID);
            return;
        }

        // 3. Logic
        if (targetAccount.ClanTier >= ClanTier.Leader)
        {
            Log.Warning("[CLAN] ClanPromoteNotify Failed: Target {TargetID} is already Leader or higher.",
                targetAccountId);
            return;
        }

        bool isLeaderReferenceUpdate = false;
        Account? oldLeaderAccount = null;

        if (targetAccount.ClanTier == ClanTier.Member)
        {
            // Member -> Officer
            targetAccount.ClanTier = ClanTier.Officer;
            Log.Information("[CLAN] Promoting Member {TargetID} to Officer.", targetAccountId);
        }
        else if (targetAccount.ClanTier == ClanTier.Officer)
        {
            // Officer -> Leader
            if (account.ClanTier != ClanTier.Leader)
            {
                Log.Warning(
                    "[CLAN] ClanPromoteNotify Failed: Requester {RequesterID} (Officer) cannot promote Officer to Leader.",
                    account.ID);
                return;
            }

            targetAccount.ClanTier = ClanTier.Leader;
            Log.Information("[CLAN] Promoting Officer {TargetID} to Leader. Demoting current Leader {RequesterID}.",
                targetAccountId, account.ID);

            // Demote requester (Old Leader) logic
            Account? requesterDb = await merrick.Accounts.FindAsync(account.ID);
            if (requesterDb != null)
            {
                requesterDb.ClanTier = ClanTier.Officer;
                oldLeaderAccount = requesterDb;

                // Update Session
                if (session.Account != null)
                {
                    session.Account.ClanTier = ClanTier.Officer;
                }
            }

            isLeaderReferenceUpdate = true;
        }

        await merrick.SaveChangesAsync();
        Log.Information("[CLAN] Database Updated successfully.");

        // 4. Update Target Session if online
        ChatSession? targetSession =
            chatContext.ClientChatSessions.Values.FirstOrDefault(cs => cs.Account?.ID == targetAccountId);
        if (targetSession != null && targetSession.Account != null)
        {
            targetSession.Account.ClanTier = targetAccount.ClanTier;
            targetSession.BroadcastConnectionStatusUpdate(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_CONNECTED);
        }

        // 5. Broadcast to Clan Channel
        ChatChannel? clanChannel =
            chatContext.ChatChannels.Values.FirstOrDefault(c => c.Name == $"Clan {account.Clan.Name}");

        ClanRankChangeResponse rankChangeResponse = new(targetAccountId, targetAccount.ClanTier, account.ID);
        ClanRankChangeResponse? leaderDemoteResponse = null;
        if (isLeaderReferenceUpdate && oldLeaderAccount != null)
        {
            leaderDemoteResponse = new ClanRankChangeResponse(oldLeaderAccount.ID, ClanTier.Officer, account.ID);
        }

        if (clanChannel != null)
        {
            Log.Information("[CLAN] Broadcasting RankChange to Clan Channel {ChannelName} ({MemberCount} members).",
                clanChannel.Name, clanChannel.Members.Count);
            foreach (ChatChannelMember member in clanChannel.Members.Values)
            {
                member.Session.Send(rankChangeResponse);
                if (leaderDemoteResponse != null)
                {
                    member.Session.Send(leaderDemoteResponse);
                }
            }
        }
        else
        {
            Log.Warning("[CLAN] Clan Channel not found for broadcast.");
        }
    }
}