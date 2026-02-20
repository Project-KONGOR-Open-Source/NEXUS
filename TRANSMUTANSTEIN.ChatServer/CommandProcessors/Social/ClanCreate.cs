namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

/// <summary>
///     Handles clan creation requests.
///     Validates the founder, clan parameters, and 4 additional founding members, then sends invites to all.
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_REQUEST)]
public class ClanCreate : ISynchronousCommandProcessor<ClientChatSession>
{
    private const int RequiredAdditionalFoundingMembers = 4;

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

        // Resolve All 4 Additional Founding Members
        List<ClientChatSession> memberSessions = [];

        for (int index = 0; index < RequiredAdditionalFoundingMembers; index++)
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

            memberSessions.Add(memberSession);
        }

        // Check None Of The Members Have Pending Clan Invites
        foreach (ClientChatSession memberSession in memberSessions)
        {
            if (PendingClan.Invites.ContainsKey(memberSession.Account.ID))
            {
                SendFailureWithName(session, ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_FAIL_INVITE, memberSession.Account.Name);

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

            foreach (ClientChatSession memberSession in memberSessions)
            {
                if (existingCreation.FounderAccountID == memberSession.Account.ID || existingCreation.IsFoundingMember(memberSession.Account.ID))
                {
                    SendFailureWithName(session, ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_FAIL_INVITE, memberSession.Account.Name);

                    conflict = true;

                    break;
                }
            }

            if (conflict)
                return;
        }

        // Store The Pending Clan Creation
        int[] targetAccountIDs = [.. memberSessions.Select(memberSession => memberSession.Account.ID)];

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
        invite.WriteString(session.Account.Name); // Founder Name
        invite.WriteString(requestData.ClanName); // Clan Name

        foreach (ClientChatSession memberSession in memberSessions)
            memberSession.Send(invite);
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
