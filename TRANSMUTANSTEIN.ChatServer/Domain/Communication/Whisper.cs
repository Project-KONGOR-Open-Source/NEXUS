using TRANSMUTANSTEIN.ChatServer.Internals;

namespace TRANSMUTANSTEIN.ChatServer.Domain.Communication;

public class Whisper
{
    /// <summary>
    ///     Hidden Constructor Which Enforces <see cref="Create" /> As The Primary Mechanism For Creating Whispers
    /// </summary>
    private Whisper() { }

    public required string Message { get; init; }

    public static Whisper Create(string message)
    {
        return new Whisper { Message = message };
    }

    public Whisper Send(IChatContext chatContext, ChatSession senderSession, string recipientName)
    {
        ChatSession? recipientSession = chatContext.ClientChatSessions.Values
            .SingleOrDefault(chatSession => chatSession.Account.Name.Equals(recipientName, StringComparison.OrdinalIgnoreCase));

        if (recipientSession == null)
        {
            SendWhisperFailure(senderSession, recipientName);
            return this;
        }

        return Send(senderSession, recipientSession);
    }

    public Whisper Send(ChatSession senderSession, ChatSession recipientSession)
    {
        // Check Recipient's Chat Mode
        switch (recipientSession.ClientMetadata.ClientChatModeState)
        {
            // DND: Block Whisper And Send Auto-Response
            case ChatProtocol.ChatModeType.CHAT_MODE_DND:
                SendWhisperFailure(senderSession, recipientSession.Account.Name)
                    .SendAutomaticResponse(senderSession, recipientSession, "Do Not Disturb");

                return this;

            // Invisible: Treat As Offline
            case ChatProtocol.ChatModeType.CHAT_MODE_INVISIBLE:
                SendWhisperFailure(senderSession, recipientSession.Account.Name);

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
                throw new ArgumentOutOfRangeException(nameof(recipientSession.ClientMetadata.ClientChatModeState),
                    recipientSession.ClientMetadata.ClientChatModeState,
                    $@"Unknown Chat Mode State ""{recipientSession.ClientMetadata.ClientChatModeState}"" For Recipient ""{recipientSession.Account.Name}""");
        }
    }

    private Whisper SendWhisperSuccess(string senderName, ChatSession recipientSession)
    {
        ChatBuffer whisperSuccess = new();

        whisperSuccess.WriteCommand(ChatProtocol.Command.CHAT_CMD_WHISPER);
        whisperSuccess.WriteString(senderName); // Sender Name
        whisperSuccess.WriteString(Message); // Message Content

        recipientSession.Send(whisperSuccess);

        return this;
    }


    private Whisper SendWhisperFailure(ChatSession senderSession, string recipientName)
    {
        ChatBuffer whisperFailed = new();

        whisperFailed.WriteCommand(ChatProtocol.Command.CHAT_CMD_WHISPER_FAILED);
        whisperFailed.WriteString(recipientName); // Recipient's Account Name
        whisperFailed.WriteString(Message); // Message Content

        senderSession.Send(whisperFailed);

        return this;
    }

    private Whisper SendAutomaticResponse(ChatSession senderSession, ChatSession recipientSession,
        string messageAutomaticResponse)
    {
        ChatBuffer automaticResponse = new();

        automaticResponse.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHAT_MODE_AUTO_RESPONSE);
        automaticResponse.WriteInt8(Convert.ToByte(recipientSession.ClientMetadata
            .ClientChatModeState)); // Recipient's Chat Mode Type
        automaticResponse.WriteString(recipientSession.Account.Name); // Recipient's Account Name
        automaticResponse.WriteString(messageAutomaticResponse); // Message Content

        senderSession.Send(automaticResponse);

        return this;
    }
}