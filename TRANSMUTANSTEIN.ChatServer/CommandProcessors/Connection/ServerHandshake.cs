namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_CONNECT)]
public class ServerHandshake(MerrickContext merrick, ILogger<ServerHandshake> logger) : CommandProcessorsBase, ICommandProcessor
{
    private MerrickContext MerrickContext { get; set; } = merrick;
    private ILogger<ServerHandshake> Logger { get; set; } = logger;

    public async Task Process(TCPSession session, ChatBuffer buffer)
    {
        ServerHandshakeRequestData requestData = new(buffer);

        // TODO: Check Cookie

        // TODO: Run Other Checks

        /*
            // Must update session cookie first so that if we call Disconnect(), remote command would execute properly.
            Subject.SessionCookie = SessionCookie;

            if (Server is null)
            {
                Console.WriteLine("Rejected server chat connection because could not validate the cookie {0}", SessionCookie);

                // Close the server so that it re-obtains a new cookie.
                Subject.SendResponse(new RemoteCommandResponse(SessionCookie, "quit"));
                Subject.Disconnect("Failed to resolve a server with provided Id and SessionCookie.");
                return;
            }

            if (ServerAccountType != AccountType.RankedMatchHost && ServerAccountType != AccountType.UnrankedMatchHost)
            {
                Console.WriteLine("Rejected server chat connection because account doens't have the hosting permissions for cookie {0}", SessionCookie);
                Subject.Disconnect("The provided account is not allowed to host games, please contact support if you believe this is wrong.");
                return;
            }

            ConnectedServer? previousInstance = KongorContext.ConnectedServers.SingleOrDefault(s => s.Address == Server.Address && s.Port == Server.Port);
            if (previousInstance != null)
            {
                previousInstance.Disconnect("A new game server instance with an identical address and port has replaced this instance.");
            }
         */

        Response.WriteCommand(ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_ACCEPT);
        Response.PrependBufferSize();

        session.SendAsync(Response.Data);
    }
}

public class ServerHandshakeRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();
    public int ServerID = buffer.ReadInt32();
    public string SessionCookie = buffer.ReadString();
    public int ChatProtocolVersion = buffer.ReadInt32();
}
