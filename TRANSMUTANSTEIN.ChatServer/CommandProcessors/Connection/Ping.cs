namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

[ChatCommand(ChatProtocol.Bidirectional.NET_CHAT_PING)]
public class Ping : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        // Log.Information("[PING] Command Received from Session {SessionID}", session.ID);
        ChatBuffer pong = new();
        pong.WriteCommand(ChatProtocol.Bidirectional.NET_CHAT_PONG);

        // Echo back any payload (e.g. timestamp)
        // buffer.Data contains [CommandID (2 bytes)] + [Payload]
        // We skip the first 2 bytes of the incoming buffer
        if (buffer.Size > 2)
        {
            pong.Append(buffer.Data.Skip(2).ToArray());
        }

        session.Send(pong);
        // Log.Information("[PING] Pong Sent to Session {SessionID}", session.ID);
    }
}