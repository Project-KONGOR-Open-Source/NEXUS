namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

public class ServerHandshakeRequestData
{
    public ServerHandshakeRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ServerId = buffer.ReadInt32();
        SessionCookie = buffer.ReadString();
        ChatProtocolVersion = buffer.ReadInt32();
    }

    public byte[] CommandBytes { get; init; }

    public int ServerId { get; }

    public string SessionCookie { get; }

    public int ChatProtocolVersion { get; }

    public MatchServerChatSessionMetadata ToMetadata()
    {
        return new MatchServerChatSessionMetadata
        {
            ServerID = ServerId, SessionCookie = SessionCookie, ChatProtocolVersion = ChatProtocolVersion
        };
    }
}