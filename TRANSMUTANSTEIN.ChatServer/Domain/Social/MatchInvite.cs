namespace TRANSMUTANSTEIN.ChatServer.Domain.Social;

/// <summary>
///     Shared logic for match invite handling.
///     Validates the invite and sends the inviter's client info to the target.
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

        ChatBuffer invite = new ();

        invite.WriteCommand(ChatProtocol.Command.CHAT_CMD_INVITED_TO_SERVER);
        invite.WriteString(senderSession.Account.Name);                                     // Name
        invite.WriteInt32(senderSession.Account.ID);                                        // Account ID
        invite.WriteInt8(Convert.ToByte(senderSession.Metadata.LastKnownClientState));       // Status
        invite.WriteInt8(senderSession.Account.GetChatClientFlags());                        // Flags
        invite.WriteString(senderSession.Account.NameColourNoPrefixCode);                    // Name Colour
        invite.WriteString(senderSession.Account.IconNoPrefixCode);                          // Icon

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
