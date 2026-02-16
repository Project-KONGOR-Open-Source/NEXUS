namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

/// <summary>
///     Handles clan creation requests.
///     Validates the founder, clan parameters, and 4 founding members, then sends invites to all.
///     C++ reference: <c>c_clientmanager.cpp:1966</c> — <c>HandleCreateClan</c>.
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_REQUEST)]
public class ClanCreate : ISynchronousCommandProcessor<ClientChatSession>
{
    private const int RequiredFoundingMembers = 4;

    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        ClanCreateRequestData requestData = new (buffer);

        // Validate Clan Name And Tag
        if (string.IsNullOrEmpty(requestData.ClanName) || string.IsNullOrEmpty(requestData.ClanTag) || requestData.ClanTag.Length > 4)
        {
            SendFailure(session, ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_FAIL_PARAM);

            return;
        }

        // Founder Must Not Already Be In A Clan
        if (session.Account.Clan is not null)
        {
            SendFailureWithName(session, ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_FAIL_CLAN, session.Account.Name);

            return;
        }

        // Founder Must Not Have A Pending Clan Invite
        if (PendingClan.Invites.ContainsKey(session.Account.ID))
        {
            SendFailureWithName(session, ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_FAIL_INVITE, session.Account.Name);

            return;
        }

        // Resolve All 4 Founding Members
        ClientChatSession?[] memberSessions = new ClientChatSession?[RequiredFoundingMembers];

        for (int index = 0; index < RequiredFoundingMembers; index++)
        {
            ClientChatSession? memberSession = Context.ClientChatSessions.Values
                .SingleOrDefault(chatSession => chatSession.Account.Name.Equals(requestData.MemberNames[index], StringComparison.OrdinalIgnoreCase));

            // Member Not Found, Already In A Clan, Or DND
            if (memberSession is null
                || memberSession.Account.Clan is not null
                || memberSession.Metadata.ClientChatModeState is ChatProtocol.ChatModeType.CHAT_MODE_DND)
            {
                SendFailureWithName(session, ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_FAIL_FIND, requestData.MemberNames[index]);

                return;
            }

            memberSessions[index] = memberSession;
        }

        // Check None Of The Members Have Pending Clan Invites
        for (int index = 0; index < RequiredFoundingMembers; index++)
        {
            if (PendingClan.Invites.ContainsKey(memberSessions[index]!.Account.ID))
            {
                SendFailureWithName(session, ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_FAIL_INVITE, memberSessions[index]!.Account.Name);

                return;
            }
        }

        // Check None Of The Members Are Already Involved In A Pending Clan Creation
        foreach (PendingClanCreation existingCreation in PendingClan.Creations.Values)
        {
            if (existingCreation.FounderAccountID == session.Account.ID)
            {
                SendFailureWithName(session, ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_FAIL_INVITE, session.Account.Name);

                return;
            }

            bool conflict = false;

            for (int index = 0; index < RequiredFoundingMembers; index++)
            {
                int memberAccountID = memberSessions[index]!.Account.ID;

                if (existingCreation.FounderAccountID == memberAccountID || existingCreation.IsFoundingMember(memberAccountID))
                {
                    SendFailureWithName(session, ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_FAIL_INVITE, memberSessions[index]!.Account.Name);

                    conflict = true;

                    break;
                }
            }

            if (conflict)
                return;
        }

        // Store The Pending Clan Creation
        int[] targetAccountIDs = new int[RequiredFoundingMembers];

        for (int index = 0; index < RequiredFoundingMembers; index++)
            targetAccountIDs[index] = memberSessions[index]!.Account.ID;

        PendingClanCreation creation = new ()
        {
            FounderAccountID = session.Account.ID,
            ClanName         = requestData.ClanName,
            ClanTag          = requestData.ClanTag,
            TargetAccountIDs = targetAccountIDs
        };

        PendingClan.Creations[session.Account.ID] = creation;

        // Send The Invite To All Founding Members (Reuses The CHAT_CMD_CLAN_ADD_MEMBER Packet)
        ChatBuffer invite = new ();

        invite.WriteCommand(ChatProtocol.Command.CHAT_CMD_CLAN_ADD_MEMBER);
        invite.WriteString(session.Account.Name);
        invite.WriteString(requestData.ClanName);

        for (int index = 0; index < RequiredFoundingMembers; index++)
            memberSessions[index]!.Send(invite);
    }

    private static void SendFailure(ClientChatSession session, ushort failureCommand)
    {
        ChatBuffer failure = new ();

        failure.WriteCommand(failureCommand);

        session.Send(failure);
    }

    private static void SendFailureWithName(ClientChatSession session, ushort failureCommand, string name)
    {
        ChatBuffer failure = new ();

        failure.WriteCommand(failureCommand);
        failure.WriteString(name);

        session.Send(failure);
    }
}

file class ClanCreateRequestData
{
    public byte[] CommandBytes { get; init; }

    public string ClanName { get; init; }

    public string ClanTag { get; init; }

    public string[] MemberNames { get; init; }

    public ClanCreateRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ClanName = buffer.ReadString();
        ClanTag = buffer.ReadString();
        MemberNames = new string[4];

        for (int index = 0; index < 4; index++)
            MemberNames[index] = buffer.ReadString();
    }
}
