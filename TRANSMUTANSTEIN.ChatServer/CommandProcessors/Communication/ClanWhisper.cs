namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Communication;

/// <summary>
///     Handles clan-wide whisper messages.
///     Broadcasts a message to all online clan members, respecting DND/AFK chat modes.
///     C++ reference: <c>c_client.cpp:1580</c> — <c>HandleClanWhisper</c>.
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

        // C++ Reference: Sends AccountID (Not Name) Then Message
        ChatBuffer clanWhisper = new ();

        clanWhisper.WriteCommand(ChatProtocol.Command.CHAT_CMD_CLAN_WHISPER);
        clanWhisper.WriteInt32(session.Account.ID);
        clanWhisper.WriteString(truncatedMessage);

        foreach (Account clanMember in session.Account.Clan.Members)
        {
            // Skip Self
            if (clanMember.ID == session.Account.ID)
                continue;

            ClientChatSession? memberSession = Context.ClientChatSessions.Values
                .SingleOrDefault(chatSession => chatSession.Account.ID == clanMember.ID);

            if (memberSession is null)
                continue;

            // C++ Reference: DND Recipients Are Skipped Without Notifying The Sender (Too Spammy)
            if (memberSession.Metadata.ClientChatModeState is ChatProtocol.ChatModeType.CHAT_MODE_DND)
                continue;

            // C++ Reference: AFK Recipients Receive The Message But No Auto-Response Is Sent (Too Spammy)

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
