namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

[ChatCommand(ChatProtocol.Bidirectional.NET_CHAT_PONG)]
public class Pong : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        // No-op: The purpose of receiving a Pong is simply to keep the connection alive,
        // which happens implicitly by receiving data on the socket.
    }
}
