namespace TRANSMUTANSTEIN.ChatServer.Communication;

public class Whisper
{
    public static void Send(ChatSession senderSession, string recipientName, string message)
    {
        ChatSession recipientSession = Context.ChatSessions.Values
            .Single(chatSession => chatSession.Account.Name.Equals(recipientName, StringComparison.OrdinalIgnoreCase));

        // Check Recipient's Chat Mode
        switch (recipientSession.Metadata.ClientChatModeState)
        {
            // DND: Block Whisper And Send Auto-Response
            case ChatProtocol.ChatModeType.CHAT_MODE_DND:
                SendWhisperFailure(senderSession, recipientName, message);
                SendAutomaticResponse(senderSession, recipientSession, "Do Not Disturb");

                return;

            // Invisible: Treat As Offline
            case ChatProtocol.ChatModeType.CHAT_MODE_INVISIBLE:
                SendWhisperFailure(senderSession, recipientName, message);

                return;

            // AFK: Deliver Message But Send Auto-Response
            case ChatProtocol.ChatModeType.CHAT_MODE_AFK:
                SendWhisperSuccess(senderSession.Account.Name, recipientSession, message);
                SendAutomaticResponse(senderSession, recipientSession, "Away From Keyboard");

                return;

            // Available: Normal Delivery
            case ChatProtocol.ChatModeType.CHAT_MODE_AVAILABLE:
                SendWhisperSuccess(senderSession.Account.Name, recipientSession, message);

                return;

            default:
                Log.Error("[BUG] Unknown Chat Mode State {Recipient.ClientChatModeState} For Recipient {Recipient.Account.Name}",
                    recipientSession.Metadata.ClientChatModeState, recipientSession.Account.Name);

                break;
        }
    }

    private static void SendWhisperSuccess(string senderName, ChatSession recipientSession, string message)
    {
        ChatBuffer whisperSuccess = new ();

        whisperSuccess.WriteCommand(ChatProtocol.Command.CHAT_CMD_WHISPER);
        whisperSuccess.WriteString(senderName); // Sender Name
        whisperSuccess.WriteString(message);    // Message Content

        recipientSession.Send(whisperSuccess);
    }


    private static void SendWhisperFailure(ChatSession senderSession, string recipientName, string message)
    {
        ChatBuffer whisperFailed = new ();

        whisperFailed.WriteCommand(ChatProtocol.Command.CHAT_CMD_WHISPER_FAILED);
        whisperFailed.WriteString(recipientName); // Recipient's Account Name
        whisperFailed.WriteString(message);       // Message Content

        senderSession.Send(whisperFailed);
    }

    private static void SendAutomaticResponse(ChatSession senderSession, ChatSession recipientSession, string message)
    {
        ChatBuffer automaticResponse = new ();

        automaticResponse.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHAT_MODE_AUTO_RESPONSE);
        automaticResponse.WriteInt8(Convert.ToByte(recipientSession.Metadata.ClientChatModeState)); // Recipient's Chat Mode Type
        automaticResponse.WriteString(recipientSession.Account.Name);                               // Recipient's Account Name
        automaticResponse.WriteString(message);                                                     // Message Content

        senderSession.Send(automaticResponse);
    }
}
