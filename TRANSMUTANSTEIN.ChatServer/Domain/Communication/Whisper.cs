namespace TRANSMUTANSTEIN.ChatServer.Domain.Communication;

public class Whisper
{
    public required string Message { get; init; }

    /// <summary>
    ///     Hidden Constructor Which Enforces <see cref="Create"/> As The Primary Mechanism For Creating Whispers
    /// </summary>
    private Whisper() { }

    public static Whisper Create(string message)
        => new () { Message = message };

    public Whisper Send(ClientChatSession senderSession, string recipientName)
    {
        ClientChatSession recipientSession = Context.ClientChatSessions.Values
            .Single(chatSession => chatSession.Account.Name.Equals(recipientName, StringComparison.OrdinalIgnoreCase));

        // Check Recipient's Chat Mode
        switch (recipientSession.Metadata.ClientChatModeState)
        {
            // DND: Block Whisper And Send Auto-Response
            case ChatProtocol.ChatModeType.CHAT_MODE_DND:
                SendWhisperFailure(senderSession, recipientName)
                    .SendAutomaticResponse(senderSession, recipientSession, "Do Not Disturb");

                return this;

            // Invisible: Treat As Offline
            case ChatProtocol.ChatModeType.CHAT_MODE_INVISIBLE:
                SendWhisperFailure(senderSession, recipientName);

                return this;

            // AFK: Deliver Message But Send Auto-Response
            case ChatProtocol.ChatModeType.CHAT_MODE_AFK:
                SendWhisperSuccess(senderSession.Account.Name, recipientSession)
                    .SendAutomaticResponse(senderSession, recipientSession, "Away From Keyboard");

                return this;

            // Available: Normal Delivery
            case ChatProtocol.ChatModeType.CHAT_MODE_AVAILABLE:
                SendWhisperSuccess(senderSession.Account.Name, recipientSession);

                return this;

            default:
                throw new ArgumentOutOfRangeException(nameof(recipientSession.Metadata.ClientChatModeState), recipientSession.Metadata.ClientChatModeState,
                    $@"Unknown Chat Mode State ""{recipientSession.Metadata.ClientChatModeState}"" For Recipient ""{recipientSession.Account.Name}""");
        }
    }

    private Whisper SendWhisperSuccess(string senderName, ClientChatSession recipientSession)
    {
        ChatBuffer whisperSuccess = new ();

        whisperSuccess.WriteCommand(ChatProtocol.Command.CHAT_CMD_WHISPER);
        whisperSuccess.WriteString(senderName); // Sender Name
        whisperSuccess.WriteString(Message);    // Message Content

        recipientSession.Send(whisperSuccess);

        return this;
    }


    private Whisper SendWhisperFailure(ClientChatSession senderSession, string recipientName)
    {
        ChatBuffer whisperFailed = new ();

        whisperFailed.WriteCommand(ChatProtocol.Command.CHAT_CMD_WHISPER_FAILED);
        whisperFailed.WriteString(recipientName); // Recipient's Account Name
        whisperFailed.WriteString(Message);       // Message Content

        senderSession.Send(whisperFailed);

        return this;
    }

    private Whisper SendAutomaticResponse(ClientChatSession senderSession, ClientChatSession recipientSession, string messageAutomaticResponse)
    {
        ChatBuffer automaticResponse = new ();

        automaticResponse.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHAT_MODE_AUTO_RESPONSE);
        automaticResponse.WriteInt8(Convert.ToByte(recipientSession.Metadata.ClientChatModeState)); // Recipient's Chat Mode Type
        automaticResponse.WriteString(recipientSession.Account.Name);                               // Recipient's Account Name
        automaticResponse.WriteString(messageAutomaticResponse);                                    // Message Content

        senderSession.Send(automaticResponse);

        return this;
    }
}
