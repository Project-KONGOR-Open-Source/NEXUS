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
}

public static class WhisperExtensions
{
    extension (Whisper whisper)
    {
        public Whisper Send(ChatSession senderSession, string recipientName)
        {
            ChatSession recipientSession = Context.ChatSessions.Values
                .Single(chatSession => chatSession.Account.Name.Equals(recipientName, StringComparison.OrdinalIgnoreCase));

            // Check Recipient's Chat Mode
            switch (recipientSession.Metadata.ClientChatModeState)
            {
                // DND: Block Whisper And Send Auto-Response
                case ChatProtocol.ChatModeType.CHAT_MODE_DND:
                    whisper
                        .SendWhisperFailure(senderSession, recipientName)
                        .SendAutomaticResponse(senderSession, recipientSession, "Do Not Disturb");

                    return whisper;

                // Invisible: Treat As Offline
                case ChatProtocol.ChatModeType.CHAT_MODE_INVISIBLE:
                    whisper
                        .SendWhisperFailure(senderSession, recipientName);

                    return whisper;

                // AFK: Deliver Message But Send Auto-Response
                case ChatProtocol.ChatModeType.CHAT_MODE_AFK:
                    whisper
                        .SendWhisperSuccess(senderSession.Account.Name, recipientSession)
                        .SendAutomaticResponse(senderSession, recipientSession, "Away From Keyboard");

                    return whisper;

                // Available: Normal Delivery
                case ChatProtocol.ChatModeType.CHAT_MODE_AVAILABLE:
                    whisper
                        .SendWhisperSuccess(senderSession.Account.Name, recipientSession);

                    return whisper;

                default:
                    throw new ArgumentOutOfRangeException(nameof(recipientSession.Metadata.ClientChatModeState), recipientSession.Metadata.ClientChatModeState,
                        $@"Unknown Chat Mode State ""{recipientSession.Metadata.ClientChatModeState}"" For Recipient ""{recipientSession.Account.Name}""");
            }
        }

        private Whisper SendWhisperSuccess(string senderName, ChatSession recipientSession)
        {
            ChatBuffer whisperSuccess = new ();

            whisperSuccess.WriteCommand(ChatProtocol.Command.CHAT_CMD_WHISPER);
            whisperSuccess.WriteString(senderName);      // Sender Name
            whisperSuccess.WriteString(whisper.Message); // Message Content

            recipientSession.Send(whisperSuccess);

            return whisper;
        }


        private Whisper SendWhisperFailure(ChatSession senderSession, string recipientName)
        {
            ChatBuffer whisperFailed = new ();

            whisperFailed.WriteCommand(ChatProtocol.Command.CHAT_CMD_WHISPER_FAILED);
            whisperFailed.WriteString(recipientName);   // Recipient's Account Name
            whisperFailed.WriteString(whisper.Message); // Message Content

            senderSession.Send(whisperFailed);

            return whisper;
        }

        private Whisper SendAutomaticResponse(ChatSession senderSession, ChatSession recipientSession, string message)
        {
            ChatBuffer automaticResponse = new ();

            automaticResponse.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHAT_MODE_AUTO_RESPONSE);
            automaticResponse.WriteInt8(Convert.ToByte(recipientSession.Metadata.ClientChatModeState)); // Recipient's Chat Mode Type
            automaticResponse.WriteString(recipientSession.Account.Name);                               // Recipient's Account Name
            automaticResponse.WriteString(message);                                                     // Message Content

            senderSession.Send(automaticResponse);

            return whisper;
        }
    }
}
