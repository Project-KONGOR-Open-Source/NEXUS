namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

public class ServerHandshakeRequestData
{
    public ServerHandshakeRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ServerID = buffer.ReadInt32();
        SessionCookie = buffer.ReadString();
        ChatProtocolVersion = buffer.ReadInt32();
    }

    public byte[] CommandBytes { get; init; }

    public int ServerID { get; }

    public string SessionCookie { get; }

    public int ChatProtocolVersion { get; }

    public MatchServerChatSessionMetadata ToMetadata()
    {
        return new MatchServerChatSessionMetadata
        {
            ServerID = ServerID, SessionCookie = SessionCookie, ChatProtocolVersion = ChatProtocolVersion
        };
    }
}
