namespace TRANSMUTANSTEIN.ChatServer.Domain.Communication;

using KINESIS.Client;
using Context = global::TRANSMUTANSTEIN.ChatServer.Internals.Context;
using global::TRANSMUTANSTEIN.ChatServer.Domain.Core;
using global::TRANSMUTANSTEIN.ChatServer.Utilities;

public class Whisper
{
    private readonly string _message;

    private Whisper(string message)
    {
        _message = message;
    }

    public static Whisper Create(string message)
    {
        return new Whisper(message);
    }

    public Whisper Send(ClientChatSession senderSession, string recipientName)
    {
        // Debugging: Log the lookup attempt
        Log.Error("DEBUG: Whisper Sender={Sender} Recipient={Recipient}", senderSession.Account.Name, recipientName);
        
        // Log all available names in the dictionary to see what we are comparing against
        string availableUsers = string.Join(", ", Context.ClientChatSessions.Keys);
        Log.Error("DEBUG: Available Users in Context.ClientChatSessions: {Users}", availableUsers);

        // Try to find the recipient session (Case insensitive lookup)
        ClientChatSession? recipientSession = Context.ClientChatSessions.FirstOrDefault(x => x.Key.Equals(recipientName, StringComparison.OrdinalIgnoreCase)).Value;
        
        if (recipientSession == null)
        {
            Log.Error("DEBUG: Whisper Recipient not found. Sent 'Offline' response.");
            return SendWhisperFailure(senderSession);
        }

        // Check Recipient's Chat Mode
        switch (recipientSession.Metadata.ClientChatModeState)
        {
            // DND: Block Whisper And Send Auto-Response
            case ChatProtocol.ChatModeType.CHAT_MODE_DND:
                SendWhisperFailure(senderSession)
                    .SendAutomaticResponse(senderSession, recipientSession, "Do Not Disturb");

                return this;

            // Invisible: Treat As Offline
            case ChatProtocol.ChatModeType.CHAT_MODE_INVISIBLE:
                return SendWhisperFailure(senderSession);

            // AFK: Deliver Message But Send Auto-Response
            case ChatProtocol.ChatModeType.CHAT_MODE_AFK:
                SendWhisperSuccess(senderSession.Account.Name, recipientSession)
                    .SendAutomaticResponse(senderSession, recipientSession, "Away From Keyboard");

                return this;

            // Available: Normal Delivery
            case ChatProtocol.ChatModeType.CHAT_MODE_AVAILABLE:
                return SendWhisperSuccess(senderSession.Account.Name, recipientSession);

            default:
                throw new ArgumentOutOfRangeException(nameof(recipientSession.Metadata.ClientChatModeState), recipientSession.Metadata.ClientChatModeState,
                    $@"Unknown Chat Mode State ""{recipientSession.Metadata.ClientChatModeState}"" For Recipient ""{recipientSession.Account.Name}""");
        }
    }

    private Whisper SendWhisperSuccess(string senderName, ClientChatSession recipientSession)
    {
        recipientSession.Send(new WhisperResponse(senderName, _message));

        return this;
    }


    private Whisper SendWhisperFailure(ClientChatSession senderSession)
    {
        senderSession.Send(new WhisperFailedResponse());

        return this;
    }

    private Whisper SendAutomaticResponse(ClientChatSession senderSession, ClientChatSession recipientSession, string messageAutomaticResponse)
    {
        senderSession.Send(new ChatModeAutoResponse(
            (ChatMode)recipientSession.Metadata.ClientChatModeState,
            recipientSession.Account.Name,
            messageAutomaticResponse));

        return this;
    }
}
