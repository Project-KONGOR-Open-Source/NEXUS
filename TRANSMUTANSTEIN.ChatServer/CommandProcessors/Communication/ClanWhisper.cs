namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Communication;

/// <summary>
///     Handles clan-wide whisper messages.
///     Broadcasts a message to all online clan members, respecting DND/AFK chat modes.
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_CLAN_WHISPER)]
public class ClanWhisper : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        ClanWhisperRequestData requestData = new (buffer);

        if (string.IsNullOrEmpty(requestData.Message))
            return;

        if (session.Account.Clan is null)
        {
            ChatBuffer failure = new ();

            failure.WriteCommand(ChatProtocol.Command.CHAT_CMD_CLAN_WHISPER_FAILED);

            session.Send(failure);

            return;
        }

        string truncatedMessage = requestData.Message.Length > ChatProtocol.CHAT_MESSAGE_MAX_LENGTH
            ? requestData.Message[..ChatProtocol.CHAT_MESSAGE_MAX_LENGTH]
            : requestData.Message;

        // Sends AccountID (Not Name) Then Message
        ChatBuffer clanWhisper = new ();

        clanWhisper.WriteCommand(ChatProtocol.Command.CHAT_CMD_CLAN_WHISPER);
        clanWhisper.WriteInt32(session.Account.ID); // Recipient's Account ID
        clanWhisper.WriteString(truncatedMessage);  // Message Content

        foreach (Account clanMember in session.Account.Clan.Members)
        {
            // Skip Self
            if (clanMember.ID == session.Account.ID)
                continue;

            ClientChatSession? memberSession = Context.ClientChatSessions.Values
                .SingleOrDefault(chatSession => chatSession.Account.ID == clanMember.ID);

            if (memberSession is null)
                continue;

            // DND Recipients Are Skipped Without Notifying The Sender
            if (memberSession.Metadata.ClientChatModeState is ChatProtocol.ChatModeType.CHAT_MODE_DND)
                continue;

            // AFK Recipients Receive The Message But No Auto-Response Is Sent

            memberSession.Send(clanWhisper);
        }
    }
}

file class ClanWhisperRequestData
{
    public byte[] CommandBytes { get; init; }

    public string Message { get; init; }

    public ClanWhisperRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        Message = buffer.ReadString();
    }
}
