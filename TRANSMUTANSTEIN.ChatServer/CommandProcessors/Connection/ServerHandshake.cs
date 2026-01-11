namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_CONNECT)]
public class ServerHandshake(IDatabase distributedCacheStore, MerrickContext databaseContext)
    : IAsynchronousCommandProcessor<ChatSession>
{
    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        ServerHandshakeRequestData requestData = new(buffer);

        // Set Match Server Metadata On Session
        // This Needs To Be Set At The First Opportunity So That Any Subsequent Code Logic Can Have Access To The Match Server's Metadata
        session.ServerMetadata = requestData.ToMetadata();

        // Validate Protocol Version
        if (requestData.ChatProtocolVersion != ChatProtocol.CHAT_PROTOCOL_EXTERNAL_VERSION)
        {
            Log.Warning(
                @"Match Server ID ""{ServerID}"" Chat Protocol Version Mismatch (Expected: ""{ExpectedVersion}"", Received: ""{ReceivedVersion}"")",
                requestData.ServerId, ChatProtocol.CHAT_PROTOCOL_EXTERNAL_VERSION, requestData.ChatProtocolVersion);

            ChatBuffer rejectResponse = new();

            rejectResponse.WriteCommand(ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_REJECT);
            rejectResponse.WriteString(
                $@"Chat Protocol Version Mismatch: Expected ""{ChatProtocol.CHAT_PROTOCOL_EXTERNAL_VERSION}"", Received ""{requestData.ChatProtocolVersion}""");

            session.Send(rejectResponse);
            await session.TerminateMatchServer(distributedCacheStore);

            return;
        }

        MatchServer? server = await distributedCacheStore.GetMatchServerBySessionCookie(requestData.SessionCookie);

        // Validate Server Cookie Against Distributed Cache
        if (server is null)
        {
            Log.Warning(@"Match Server ID ""{ServerID}"" Cookie ""{Cookie}"" Is Invalid", requestData.ServerId,
                requestData.SessionCookie);

            ChatBuffer rejectResponse = new();

            rejectResponse.WriteCommand(ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_REJECT);
            rejectResponse.WriteString("The Session Cookie Provided For Chat Server Authentication Is Invalid");

            session.Send(rejectResponse);
            await session.TerminateMatchServer(distributedCacheStore);

            return;
        }

        // Validate Server ID Match
        if (server.ID != requestData.ServerId)
        {
            Log.Warning(@"Match Server ID ""{ServerID}"" Does Not Match The Server ID With Session Cookie ""{Cookie}""",
                requestData.ServerId, requestData.SessionCookie);

            ChatBuffer rejectResponse = new();

            rejectResponse.WriteCommand(ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_REJECT);
            rejectResponse.WriteString($@"Match Server ID Mismatch: Received ""{server.ID}""");

            session.Send(rejectResponse);
            await session.TerminateMatchServer(distributedCacheStore);

            return;
        }

        Account? hostAccount = await databaseContext.Accounts.FindAsync(server.HostAccountID);

        // Validate Host Account Against Database
        if (hostAccount is null)
        {
            Log.Warning(@"Could Not Find Host Account ID ""{HostAccountID}"" For Match Server ID ""{ServerID}"")",
                server.HostAccountID, requestData.ServerId);

            ChatBuffer rejectResponse = new();

            rejectResponse.WriteCommand(ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_REJECT);
            rejectResponse.WriteString("Match Server Host Account Not Found");

            session.Send(rejectResponse);
            await session.TerminateMatchServer(distributedCacheStore);

            return;
        }

        // Set Host Account On Match Server Session
        session.Account = hostAccount;

        // Validate Match Hosting Permissions
        if (hostAccount.Type != AccountType.ServerHost)
        {
            Log.Warning(
                @"Host Account ID ""{HostAccountID}"" For Match Server ID ""{ServerID}"" Does Not Have Match Hosting Permissions",
                requestData.ServerId, server.HostAccountID);

            ChatBuffer rejectResponse = new();

            rejectResponse.WriteCommand(ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_REJECT);
            rejectResponse.WriteString("The Match Server Host Account Does Not Have Match Hosting Permissions");

            session.Send(rejectResponse);
            await session.TerminateMatchServer(distributedCacheStore);

            return;
        }

        // Validate Host Account ID Match
        if (hostAccount.ID != server.HostAccountID)
        {
            Log.Warning(
                @"Match Server Host Account ID ""{ReceivedHostAccountID}"" Does Not Match The Host Account ID ""{ExpectedHostAccountID}"" For Match Server ID ""{ServerID}""",
                server.HostAccountID, hostAccount.ID, requestData.ServerId);

            ChatBuffer rejectResponse = new();

            rejectResponse.WriteCommand(ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_REJECT);
            rejectResponse.WriteString($@"Match Server Host Account ID Mismatch: Received ""{server.HostAccountID}""");

            session.Send(rejectResponse);
            await session.TerminateMatchServer(distributedCacheStore);

            return;
        }

        // Check For Duplicate Match Server Instances
        if (Context.MatchServerChatSessions.TryGetValue(requestData.ServerId, out ChatSession? existingSession))
        {
            Log.Information(
                @"Disconnecting Duplicate Match Server Instance With ID ""{ServerID}"" And Address ""{Address}:{Port}"")",
                requestData.ServerId, server.IPAddress, server.Port);

            await existingSession.TerminateMatchServer(distributedCacheStore);

            Context.MatchServerChatSessions.TryRemove(requestData.ServerId, out _);
        }

        // Register Match Server
        Context.MatchServerChatSessions[requestData.ServerId] = session;

        Log.Information(
            @"Match Server Connection Accepted - Server ID: ""{ServerID}"", Host Account: ""{HostAccountName}"", Address: ""{Address}:{Port}"", Location: ""{Location}""",
            requestData.ServerId, server.HostAccountName, server.IPAddress, server.Port, server.Location);

        ChatBuffer acceptResponse = new();

        acceptResponse.WriteCommand(ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_ACCEPT);

        session.Send(acceptResponse);

        string
            uniqueServerName =
                Random.Shared.Next()
                    .ToString("X8"); // TODO: Use The Original Name As Identifier, To Verify Server Binaries Checksum

        ChatBuffer remoteCommand = new();

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