namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Communication;

/// <summary>
///     Handles whisper-to-all-buddies messages.
///     Broadcasts a message to all online friends, respecting DND/AFK chat modes.
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_WHISPER_BUDDIES)]
public class WhisperBuddies : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        WhisperBuddiesRequestData requestData = new (buffer);

        if (string.IsNullOrEmpty(requestData.Message))
            return;

        string truncatedMessage = requestData.Message.Length > ChatProtocol.CHAT_MESSAGE_MAX_LENGTH
            ? requestData.Message[..ChatProtocol.CHAT_MESSAGE_MAX_LENGTH]
            : requestData.Message;

        foreach (FriendedPeer friend in session.Account.FriendedPeers)
        {
            ClientChatSession? friendSession = Context.ClientChatSessions.Values
                .SingleOrDefault(chatSession => chatSession.Account.ID == friend.ID);

            if (friendSession is null)
                continue;

            // DND Recipients Are Skipped Without Notifying The Sender (Too Spammy)
            if (friendSession.Metadata.ClientChatModeState is ChatProtocol.ChatModeType.CHAT_MODE_DND)
                continue;

            // AFK Recipients Receive The Message But No Auto-Response Is Sent (Too Spammy)

            ChatBuffer whisperBuddies = new ();

            whisperBuddies.WriteCommand(ChatProtocol.Command.CHAT_CMD_WHISPER_BUDDIES);
            whisperBuddies.WriteString(session.Account.Name);
            whisperBuddies.WriteString(truncatedMessage);

            friendSession.Send(whisperBuddies);
        }
    }
}

file class WhisperBuddiesRequestData
{
    public byte[] CommandBytes { get; init; }

    public string Message { get; init; }

    public WhisperBuddiesRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        Message = buffer.ReadString();
    }
}
