namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_SET_CHAT_MODE_TYPE)]
public class SetChatMode : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        SetChatModeRequestData requestData = new (buffer);

        ChatProtocol.ChatModeType oldMode = session.ClientMetadata.ClientChatModeState;
        ChatProtocol.ChatModeType newMode = (ChatProtocol.ChatModeType)requestData.ChatModeType;

        // Transition TO Invisible: Tell friends I disconnected
        if (oldMode != ChatProtocol.ChatModeType.CHAT_MODE_INVISIBLE && newMode == ChatProtocol.ChatModeType.CHAT_MODE_INVISIBLE)
        {
            // We must broadcast 'Disconnected' status BEFORE we change the state?
            // Actually, if we change state to Invisible, 'BroadcastConnectionStatusUpdate' will skip us.
            // So we can't use 'BroadcastConnectionStatusUpdate'.
            // We need to manually broadcast a 'Disconnected' packet to friends.
            // But 'BroadcastConnectionStatusUpdate' logic is complex (getting friends lists etc).
            
            // Hack: Temporarily set status to DISCONNECTED? No.
            // Better: Extract logic or just replicate "Send Disconnect To Friends" logic.
            // Since I cannot change 'ChatSession.Client.cs' easily to expose private 'BroadcastConnectionStatusUpdate' logic for this specific case without refactoring.
            
            // Alternative:
            // 1. Send 'Disconnected' status while still Visible.
            // 2. Change to Invisible.
            
             session.BroadcastConnectionStatusUpdate(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_DISCONNECTED);
        }

        session.ClientMetadata.ClientChatModeState = newMode;

        // Transition FROM Invisible: Tell friends I connected
        if (oldMode == ChatProtocol.ChatModeType.CHAT_MODE_INVISIBLE && newMode != ChatProtocol.ChatModeType.CHAT_MODE_INVISIBLE)
        {
             session.BroadcastConnectionStatusUpdate(session.ClientMetadata.LastKnownClientState);
        }

        // Send Confirmation To Client
        ChatBuffer response = new ();
        response.WriteCommand(ChatProtocol.Command.CHAT_CMD_SET_CHAT_MODE_TYPE);
        response.WriteInt8(requestData.ChatModeType);
        response.WriteString(requestData.Reason);
        
        session.Send(response);
    }
}

file class SetChatModeRequestData
{
    public byte[] CommandBytes { get; init; }
    public byte ChatModeType { get; init; }
    public string Reason { get; init; }

    public SetChatModeRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ChatModeType = buffer.ReadInt8();
        Reason = buffer.ReadString();
    }
}

