namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_INVITE)]
public class GroupInvite(MerrickContext merrick) : ISynchronousCommandProcessor
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        GroupInviteRequestData requestData = new (buffer);

        MatchmakingGroup group = MatchmakingService.GetMatchmakingGroup(session.Account.ID)
            ?? throw new NullReferenceException($@"No Matchmaking Group Found For Invite Issuer ID ""{session.Account.ID}""");

        ChatBuffer invite = new ();

        invite.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_INVITE);

        invite.WriteString(session.Account.Name);                                                     // Invite Issuer Name
        invite.WriteInt32(session.Account.ID);                                                        // Invite Issuer ID
        invite.WriteInt8(Convert.ToByte(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_CONNECTED)); // Invite Issuer Status
        invite.WriteInt8(session.Account.GetChatClientFlags());                                       // Invite Issuer Chat Flags
        invite.WriteString(session.Account.NameColour);                                               // Invite Issuer Chat Name Colour
        invite.WriteString(session.Account.Icon);                                                     // Invite Issuer Icon
        invite.WriteString(group.Information.MapName);                                                // Map Name
        invite.WriteInt8(Convert.ToByte(group.Information.GameType));                                 // Game Type
        invite.WriteString(string.Join('|', group.Information.GameModes));                            // Game Modes
        invite.WriteString(string.Join('|', group.Information.GameRegions));                          // Game Regions

        invite.PrependBufferSize();

        ChatSession inviteReceiverSession = Context.ChatSessions
            .Values.Single(session => session.Account.Name.Equals(requestData.InviteReceiverName));

        inviteReceiverSession.SendAsync(invite.Data);

        ChatBuffer inviteBroadcast = new ();

        inviteBroadcast.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_INVITE_BROADCAST);

        Account inviteReceiver = merrick.Accounts.Include(account => account.Clan)
            .Single(account => account.Name.Equals(requestData.InviteReceiverName));

        inviteBroadcast.WriteString(inviteReceiver.NameWithClanTag);                                  // Invite Receiver Name
        inviteBroadcast.WriteString(session.Account.NameWithClanTag);                                 // Invite Issuer Name

        inviteBroadcast.PrependBufferSize();

        Parallel.ForEach(group.Members, (member) => member.Session.SendAsync(inviteBroadcast.Data));
    }
}

public class GroupInviteRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();

    public string InviteReceiverName = buffer.ReadString();
}
