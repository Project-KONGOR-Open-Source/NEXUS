namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_CONNECT)]
public class ServerHandshake(IDatabase distributedCacheStore, MerrickContext databaseContext) : IAsynchronousCommandProcessor<MatchServerChatSession>
{
    public async Task Process(MatchServerChatSession session, ChatBuffer buffer)
    {
        ServerHandshakeRequestData requestData = new (buffer);

        // Set Match Server Metadata On Session
        // This Needs To Be Set At The First Opportunity So That Any Subsequent Code Logic Can Have Access To The Match Server's Metadata
        session.Metadata = requestData.ToMetadata();

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

        string uniqueServerName = Random.Shared.Next().ToString("X8"); // TODO: Use The Original Name As Identifier, To Verify Server Binaries Checksum

        ChatBuffer remoteCommand = new ();

        string[] commands =
        [
            "svr_submitMatchStatItems true",
            "svr_submitMatchStatAbilities true",
            "svr_submitMatchStatFrags true",
            $"svr_name {uniqueServerName}",
            "echo Project KONGOR Remote Configuration Was Injected Successfully"
        ];

        remoteCommand.WriteCommand(ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_REMOTE_COMMAND);
        remoteCommand.WriteString(requestData.SessionCookie);
        remoteCommand.WriteString(string.Join(";", commands));

        // Inject The Match Server With Remote Configuration
        session.Send(remoteCommand);

        server.Name = uniqueServerName;

        // Update The Match Server Name In The Distributed Cache
        await distributedCacheStore.SetMatchServer(server.HostAccountName, server);
    }
}

file class ServerHandshakeRequestData
{
    public byte[] CommandBytes { get; init; }

    public int ServerID { get; init; }

    public string SessionCookie { get; init; }

    public int ChatProtocolVersion { get; init; }

    public ServerHandshakeRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ServerID = buffer.ReadInt32();
        SessionCookie = buffer.ReadString();
        ChatProtocolVersion = buffer.ReadInt32();
    }

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
