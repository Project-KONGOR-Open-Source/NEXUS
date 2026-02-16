namespace TRANSMUTANSTEIN.ChatServer.Domain.Social;

/// <summary>
///     Shared logic for match invite handling.
///     Validates the invite and sends the inviter's client info to the target.
///     C++ reference: <c>c_client.cpp:1841</c> — <c>HandleInviteToServer</c>.
/// </summary>
public static class MatchInvite
{
    /// <summary>
    ///     Sends a match invite from the sender to the target.
    ///     Validates that the sender is in a match and the target is online and reachable.
    /// </summary>
    public static void Send(ClientChatSession senderSession, ClientChatSession? targetSession)
    {
        // Target Not Found, Is Self, Or Is Disconnected
        if (targetSession is null
            || targetSession.Account.ID == senderSession.Account.ID
            || targetSession.Metadata.LastKnownClientState < ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_CONNECTED)
        {
            ChatBuffer userFailed = new ();

            userFailed.WriteCommand(ChatProtocol.Command.CHAT_CMD_INVITE_FAILED_USER);

            senderSession.Send(userFailed);

            return;
        }

        // Sender Must Be In A Match (Joining Or In-Match)
        if (senderSession.Metadata.LastKnownClientState < ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_JOINING_GAME)
        {
            ChatBuffer matchFailed = new ();

            matchFailed.WriteCommand(ChatProtocol.Command.CHAT_CMD_INVITE_FAILED_GAME);

            senderSession.Send(matchFailed);

            return;
        }

        // Target Is DND — Silently Drop
        if (targetSession.Metadata.ClientChatModeState is ChatProtocol.ChatModeType.CHAT_MODE_DND)
            return;

        // C++ Reference: GetClientInfoBuffer(buf, TRUE, FALSE, FALSE, TRUE, FALSE)
        // Fields: Name, AccountID, Status, Flags, NameColour, Icon, ServerInfo
        ChatBuffer invite = new ();

        invite.WriteCommand(ChatProtocol.Command.CHAT_CMD_INVITED_TO_SERVER);
        invite.WriteString(senderSession.Account.Name);
        invite.WriteInt32(senderSession.Account.ID);
        invite.WriteInt8(Convert.ToByte(senderSession.Metadata.LastKnownClientState));
        invite.WriteInt8(senderSession.Account.GetChatClientFlags());
        invite.WriteString(senderSession.Account.NameColourNoPrefixCode);
        invite.WriteString(senderSession.Account.IconNoPrefixCode);

        // Server Info (Only If Joining Or In-Match)
        if (senderSession.Metadata.MatchServerConnectedTo is not null)
        {
            MatchServer matchServer = senderSession.Metadata.MatchServerConnectedTo;

            invite.WriteString($"{matchServer.IPAddress}:{matchServer.Port}");

            if (senderSession.Metadata.LastKnownClientState is ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_IN_GAME)
            {
                invite.WriteString(senderSession.MatchInformation?.MatchName ?? string.Empty);
                invite.WriteInt32(senderSession.MatchInformation?.MatchID ?? 0);
            }
        }

        targetSession.Send(invite);
    }
}
