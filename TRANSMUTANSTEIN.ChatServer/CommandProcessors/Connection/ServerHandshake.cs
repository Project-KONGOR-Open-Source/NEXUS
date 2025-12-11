namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_CONNECT)]
public class ServerHandshake(IDatabase distributedCacheStore, MerrickContext databaseContext) : IAsynchronousCommandProcessor<MatchServerChatSession>
{
    public async Task Process(MatchServerChatSession session, ChatBuffer buffer)
    {
        ServerHandshakeRequestData requestData = new (buffer);

        // Validate Protocol Version
        if (requestData.ChatProtocolVersion != ChatProtocol.CHAT_PROTOCOL_EXTERNAL_VERSION)
        {
            Log.Warning(@"Match Server ID ""{ServerID}"" Chat Protocol Version Mismatch (Expected: ""{ExpectedVersion}"", Received: ""{ReceivedVersion}"")",
                requestData.ServerID, ChatProtocol.CHAT_PROTOCOL_EXTERNAL_VERSION, requestData.ChatProtocolVersion);

            ChatBuffer rejectResponse = new ();

            rejectResponse.WriteCommand(ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_REJECT);
            rejectResponse.WriteString($@"Chat Protocol Version Mismatch: Expected ""{ChatProtocol.CHAT_PROTOCOL_EXTERNAL_VERSION}"", Received ""{requestData.ChatProtocolVersion}""");

            session.Send(rejectResponse);
            await session.Terminate(distributedCacheStore);

            return;
        }

        MatchServer? server = await distributedCacheStore.GetMatchServerBySessionCookie(requestData.SessionCookie);

        // Validate Server Cookie Against Distributed Cache
        if (server is null)
        {
            Log.Warning(@"Match Server ID ""{ServerID}"" Cookie ""{Cookie}"" Is Invalid", requestData.ServerID, requestData.SessionCookie);

            ChatBuffer rejectResponse = new ();

            rejectResponse.WriteCommand(ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_REJECT);
            rejectResponse.WriteString("The Session Cookie Provided For Chat Server Authentication Is Invalid");

            session.Send(rejectResponse);
            await session.Terminate(distributedCacheStore);

            return;
        }

        // Validate Server ID Match
        if (server.ID != requestData.ServerID)
        {
            Log.Warning(@"Match Server ID ""{ServerID}"" Does Not Match The Server ID With Session Cookie ""{Cookie}""",requestData.ServerID, requestData.SessionCookie);

            ChatBuffer rejectResponse = new ();

            rejectResponse.WriteCommand(ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_REJECT);
            rejectResponse.WriteString($@"Match Server ID Mismatch: Received ""{server.ID}""");

            session.Send(rejectResponse);
            await session.Terminate(distributedCacheStore);

            return;
        }

        Account? hostAccount = await databaseContext.Accounts.FindAsync(server.HostAccountID);

        // Validate Host Account Against Database
        if (hostAccount is null)
        {
            Log.Warning(@"Could Not Find Host Account ID ""{HostAccountID}"" For Match Server ID ""{ServerID}"")", server.HostAccountID, requestData.ServerID);

            ChatBuffer rejectResponse = new ();

            rejectResponse.WriteCommand(ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_REJECT);
            rejectResponse.WriteString("Match Server Host Account Not Found");

            session.Send(rejectResponse);
            await session.Terminate(distributedCacheStore);

            return;
        }

        // Set Host Account On Match Server Session
        session.Account = hostAccount;

        // Set Match Server Metadata On Session
        session.Metadata = requestData.ToMetadata();

        // Validate Match Hosting Permissions
        if (hostAccount.Type != AccountType.ServerHost)
        {
            Log.Warning(@"Host Account ID ""{HostAccountID}"" For Match Server ID ""{ServerID}"" Does Not Have Match Hosting Permissions", requestData.ServerID, server.HostAccountID);

            ChatBuffer rejectResponse = new ();

            rejectResponse.WriteCommand(ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_REJECT);
            rejectResponse.WriteString("The Match Server Host Account Does Not Have Match Hosting Permissions");

            session.Send(rejectResponse);
            await session.Terminate(distributedCacheStore);

            return;
        }

        // Validate Host Account ID Match
        if (hostAccount.ID != server.HostAccountID)
        {
            Log.Warning(@"Match Server Host Account ID ""{ReceivedHostAccountID}"" Does Not Match The Host Account ID ""{ExpectedHostAccountID}"" For Match Server ID ""{ServerID}""",
                server.HostAccountID, hostAccount.ID, requestData.ServerID);

            ChatBuffer rejectResponse = new ();

            rejectResponse.WriteCommand(ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_REJECT);
            rejectResponse.WriteString($@"Match Server Host Account ID Mismatch: Received ""{server.HostAccountID}""");

            session.Send(rejectResponse);
            await session.Terminate(distributedCacheStore);

            return;
        }

        // Check For Duplicate Match Server Instances
        if (Context.MatchServerChatSessions.TryGetValue(requestData.ServerID, out MatchServerChatSession? existingSession))
        {
            Log.Information(@"Disconnecting Duplicate Match Server Instance With ID ""{ServerID}"" And Address ""{Address}:{Port}"")", requestData.ServerID, server.IPAddress, server.Port);

            await existingSession.Terminate(distributedCacheStore);

            Context.MatchServerChatSessions.TryRemove(requestData.ServerID, out _);
        }

        // Register Match Server
        Context.MatchServerChatSessions[requestData.ServerID] = session;

        Log.Information(@"Match Server Connection Accepted - Server ID: ""{ServerID}"", Host Account: ""{HostAccountName}"", Address: ""{Address}:{Port}"", Location: ""{Location}""",
            requestData.ServerID, server.HostAccountName, server.IPAddress, server.Port, server.Location);

        ChatBuffer acceptResponse = new ();

        acceptResponse.WriteCommand(ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_ACCEPT);

        session.Send(acceptResponse);
    }
}

public class ServerHandshakeRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();

    public int ServerID = buffer.ReadInt32();

    public string SessionCookie = buffer.ReadString();

    public int ChatProtocolVersion = buffer.ReadInt32();

    public MatchServerChatSessionMetadata ToMetadata()
    {
        return new MatchServerChatSessionMetadata()
        {
            ServerID = ServerID,
            SessionCookie = SessionCookie,
            ChatProtocolVersion = ChatProtocolVersion
        };
    }
}
