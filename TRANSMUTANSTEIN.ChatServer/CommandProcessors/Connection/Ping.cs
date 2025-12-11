namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

/// <summary>
///     Handles ping requests from clients, match servers, and match server managers.
///     Responds with a pong message to keep the connection alive.
/// </summary>
[ChatCommand(ChatProtocol.Bidirectional.NET_CHAT_PING)]
public class Ping : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        PingRequestData requestData = new (buffer);

        ChatBuffer pongResponse = new ();

        pongResponse.WriteCommand(ChatProtocol.Bidirectional.NET_CHAT_PONG);

        session.Send(pongResponse);
    }
}

public class PingRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();
}
