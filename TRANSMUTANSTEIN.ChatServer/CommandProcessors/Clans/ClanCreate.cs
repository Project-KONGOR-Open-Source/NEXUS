using System.Text.RegularExpressions;

using TRANSMUTANSTEIN.ChatServer.Domain.Clans;
using TRANSMUTANSTEIN.ChatServer.Internals;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Clans;

public readonly struct ClanCreateRequestData
{
    public readonly string Name;
    public readonly string Tag;
    public readonly List<string> Members;

    public ClanCreateRequestData(ChatBuffer buffer)
    {
        // Skip Header
        buffer.ReadCommandBytes();

        List<string> members = new();
        Name = buffer.ReadString();
        Tag = buffer.ReadString();

        // Legacy: NumberOfFoundingMembers = 4
        for (int i = 0; i < 4; i++)
        {
            members.Add(buffer.ReadString());
        }

        Members = members;
    }
}

[ChatCommand(ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_REQUEST)]
public class ClanCreate(MerrickContext merrick, IPendingClanService pendingClanService, IChatContext chatContext)
    : IAsynchronousCommandProcessor<ChatSession>
{
    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        Account? account = session.Account;
        if (account == null)
        {
            return;
        }

        ClanCreateRequestData requestData = new(buffer);

        pendingClanService.RemoveObsoledPendingClans();

        // Validation - Name/Tag
        ChatBuffer? failResponse = await ClanTagNameAndDuplicatesSanityCheck(merrick, account, requestData.Name,
            requestData.Tag, requestData.Members, pendingClanService);
        if (failResponse != null)
        {
            session.Send(failResponse);
            return;
        }

        // Validation - Members
        List<Account> clanMemberAccounts = new();
        failResponse = await MembersSanityCheck(merrick, account, requestData.Members, clanMemberAccounts,
            pendingClanService);
        if (failResponse != null)
        {
            session.Send(failResponse);
            return;
        }

        // Check if all founding members are connected
        // Note: clanMemberAccounts contains the 4 invitees.
        // We act on their Names.
        foreach (Account memberAccount in clanMemberAccounts)
        {
            // Verify online status
            // Legacy checked ConnectedClients.TryGetValue(accountId).
            // In Aspire, we check ClientChatSessions.

            bool isOnline = chatContext.ClientChatSessions.Values.Any(cs =>
                cs.Account.ID == memberAccount.ID); // Check by ID is safest

            if (!isOnline)
            {
                session.Send(new ClanCreateFailResponse(ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_FAIL_INVITE,
                    memberAccount.Name));
                return;
            }
        }

        // Add creator to member IDs list for PendingClan structure?
        // Legacy: Creator is separate (CreatorAccountId), MembersAccountId has the 4 members.
        // Legacy pendingClan.MembersAccountId = ClanMembersAccountIds (from SanityCheck).

        List<int> memberIds = clanMemberAccounts.Select(a => a.ID).ToList();

        PendingClan pendingClan = new()
        {
            ClanName = requestData.Name,
            ClanTag = requestData.Tag,
            Accepted = new List<bool> { false, false, false, false }, // 4 slots
            CreationTime = DateTime.UtcNow,
            CreatorAccountId = account.ID,
            MembersAccountId = memberIds
        };

        pendingClanService.InsertPendingClan(pendingClan);

        // Send all invites
        foreach (Account memberAccount in clanMemberAccounts)
        {
            ChatSession? targetSession = chatContext.ClientChatSessions.Values
                .FirstOrDefault(cs => cs.Account.ID == memberAccount.ID);

            if (targetSession != null)
            {
                targetSession.Send(new ClanAddMemberResponse(account.Name, requestData.Name));
            }
        }

        // Note: Legacy CreatorResponse is sent if Sanity Check fails.
        // If success, nothing is sent to Creator immediately?
        // Legacy: "if (CreatorResponse != null) Subject.SendResponse..."
        // Then loop checks connections.
        // Then InsertPendingClan.
        // Then Send invites.
        // It seems Creator gets no response until members accept?
        // Or maybe Creator assumes success if no Fail response?
        // Correct.
    }

    private async Task<ChatBuffer?> MembersSanityCheck(MerrickContext merrick, Account account, List<string> members,
        List<Account> clanMemberAccounts, IPendingClanService pendingClanService)
    {
        // check if Creator is in a clan
        if (account.Clan != null)
        {
            return new ClanCreateFailResponse(ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_FAIL_CLAN, account.Name);
        }

        // check if Creator is in any pending invites or clan creation
        if (pendingClanService.IsUserInPendingClans(account))
        {
            return new ClanCreateFailResponse(ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_FAIL_INVITE, account.Name);
        }

        // We act on the strings provided in members list
        foreach (string member in members)
        {
            Account? memberAccount = await merrick.Accounts
                .Include(acc => acc.Clan)
                .FirstOrDefaultAsync(acc => acc.Name == member);

            if (memberAccount == null)
            {
                return new ClanCreateFailResponse(ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_FAIL_FIND, member);
            }

            if (memberAccount.Clan != null)
            {
                return new ClanCreateFailResponse(ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_FAIL_CLAN,
                    memberAccount.Name);
            }

            if (pendingClanService.IsUserInPendingClans(memberAccount))
            {
                return new ClanCreateFailResponse(ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_FAIL_INVITE,
                    memberAccount.Name);
            }

            clanMemberAccounts.Add(memberAccount);
        }

        return null;
    }

    private async Task<ChatBuffer?> ClanTagNameAndDuplicatesSanityCheck(MerrickContext merrick, Account account,
        string name, string tag, List<string> members, IPendingClanService pendingClanService)
    {
        Regex clanNameRegex = new(@"^[a-zA-Z0-9 ]{1,25}$"); /* 25 characters max */
        Regex clanTagRegex = new(@"^[a-zA-Z0-9]{1,4}$"); /* 4 characters max */

        Match match = clanNameRegex.Match(name);

        if (!match.Success)
        {
            return new ClanCreateFailResponse(ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_FAIL_NAME);
        }

        # region Clan Name Visual Formatting Checks

        const int availableScreenSpaceInPixels = 220;

        bool tooWide = EstimateClanNameWidth(name) > availableScreenSpaceInPixels;

        bool tooManyConsecutiveSpaces = name.Contains("  ");

        if (tooWide || tooManyConsecutiveSpaces)
        {
            return new ClanCreateFailResponse(ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_FAIL_NAME);
        }

        # endregion

        match = clanTagRegex.Match(tag);

        if (!match.Success)
        {
            return new ClanCreateFailResponse(ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_FAIL_TAG);
        }

        bool clanNameExists = await merrick.Clans.AnyAsync(clan => clan.Name == name);
        if (clanNameExists)
        {
            return new ClanCreateFailResponse(ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_FAIL_NAME);
        }

        bool clanTagExists = await merrick.Clans.AnyAsync(clan => clan.Tag == tag);
        if (clanTagExists)
        {
            return new ClanCreateFailResponse(ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_FAIL_TAG);
        }

        if (pendingClanService.IsClanInPendingClans(name, tag))
        {
            return new ClanCreateFailResponse(ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_FAIL_PARAM);
        }

        // check if creator included himself
        if (members.Contains(account.Name, StringComparer.OrdinalIgnoreCase))
        {
            return new ClanCreateFailResponse(ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_FAIL_DUPE);
        }

        // check if there are duplicated members
        if (members.Count > members.Distinct(StringComparer.OrdinalIgnoreCase).Count())
        {
            return new ClanCreateFailResponse(ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_FAIL_DUPE);
        }

        return null;
    }

    private static int EstimateClanNameWidth(string name)
    {
        int clanNameWidthInPixels = 0;

        foreach (char letter in name)
        {
            if (letter is 'i' or 'I' or 'l' or ' ')
            {
                clanNameWidthInPixels += 6; // Skinny Letter Or Space: 6 pixels
            }
            else if (letter is 'm' or 'w')
            {
                clanNameWidthInPixels += 16; // Lowercase Wide Letter: 16 pixels
            }
            else if (letter is 'M' or 'W')
            {
                clanNameWidthInPixels += 19; // Uppercase Wide Letter: 19 pixels
            }
            else if (letter is 'O')
            {
                clanNameWidthInPixels += 17; // Uppercase Semi-Wide Letter: 17 pixels
            }
            else if (char.IsLower(letter))
            {
                clanNameWidthInPixels += 10; // Average Lowercase Regular Letter: 10 pixels
            }
            else if (char.IsUpper(letter))
            {
                clanNameWidthInPixels += 15; // Average Uppercase Regular Letter: 15 pixels
            }
        }

        return clanNameWidthInPixels;
    }
}