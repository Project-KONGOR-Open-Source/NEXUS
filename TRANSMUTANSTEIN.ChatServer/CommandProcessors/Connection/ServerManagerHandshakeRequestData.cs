namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

public class ServerManagerHandshakeRequestData
{
    public ServerManagerHandshakeRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ServerManagerID = buffer.ReadInt32();
        SessionCookie = buffer.ReadString();
        ChatProtocolVersion = buffer.ReadInt32();
    }

    public byte[] CommandBytes { get; init; }

    public int ServerManagerID { get; }

    public string SessionCookie { get; }

    public int ChatProtocolVersion { get; }

    public MatchServerManagerChatSessionMetadata ToMetadata()
    {
        return new MatchServerManagerChatSessionMetadata
        {
            ServerManagerID = ServerManagerID,
            SessionCookie = SessionCookie,
            ChatProtocolVersion = ChatProtocolVersion
        };
    }
}