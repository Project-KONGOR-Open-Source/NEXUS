namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

[ChatCommand(ChatProtocol.ServerManagerToChatServer.NET_CHAT_SM_CONNECT)]
public class ServerManagerHandshake(IDatabase distributedCacheStore, MerrickContext databaseContext)
    : IAsynchronousCommandProcessor<ChatSession>
{
    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        ServerManagerHandshakeRequestData requestData = new(buffer);

        // Set Match Server Manager Metadata On Session
        // This Needs To Be Set At The First Opportunity So That Any Subsequent Code Logic Can Have Access To The Match Server Manager's Metadata
        session.ManagerMetadata = requestData.ToMetadata();

        // Validate Protocol Version
        if (requestData.ChatProtocolVersion != ChatProtocol.CHAT_PROTOCOL_EXTERNAL_VERSION)
        {
            Log.Warning(
                @"Match Server Manager ID ""{ServerManagerID}"" Chat Protocol Version Mismatch (Expected: ""{ExpectedVersion}"", Received: ""{ReceivedVersion}"")",
                requestData.ServerManagerID, ChatProtocol.CHAT_PROTOCOL_EXTERNAL_VERSION,
                requestData.ChatProtocolVersion);

            ChatBuffer rejectResponse = new();

            rejectResponse.WriteCommand(ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_REJECT);
            rejectResponse.WriteString(
                $@"Chat Protocol Version Mismatch: Expected ""{ChatProtocol.CHAT_PROTOCOL_EXTERNAL_VERSION}"", Received ""{requestData.ChatProtocolVersion}""");

            session.Send(rejectResponse);
            await session.TerminateMatchServerManager(distributedCacheStore);

            return;
        }

        MatchServerManager? manager =
            await distributedCacheStore.GetMatchServerManagerBySessionCookie(requestData.SessionCookie);

        // Validate Server Manager Cookie Against Distributed Cache
        if (manager is null)
        {
            Log.Warning(@"Match Server Manager ID ""{ServerManagerID}"" Cookie ""{Cookie}"" Is Invalid",
                requestData.ServerManagerID, requestData.SessionCookie);

            ChatBuffer rejectResponse = new();

            rejectResponse.WriteCommand(ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_REJECT);
            rejectResponse.WriteString("The Session Cookie Provided For Chat Server Manager Authentication Is Invalid");

            session.Send(rejectResponse);
            await session.TerminateMatchServerManager(distributedCacheStore);

            return;
        }

        // Validate Server Manager ID Match
        if (manager.ID != requestData.ServerManagerID)
        {
            Log.Warning(
                @"Match Server Manager ID ""{ServerManagerID}"" Does Not Match The Server Manager ID With Session Cookie ""{Cookie}""",
                requestData.ServerManagerID, requestData.SessionCookie);

            ChatBuffer rejectResponse = new();

            rejectResponse.WriteCommand(ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_REJECT);
            rejectResponse.WriteString($@"Match Server Manager ID Mismatch: Received ""{manager.ID}""");

            session.Send(rejectResponse);
            await session.TerminateMatchServerManager(distributedCacheStore);

            return;
        }

        Account? hostAccount = await databaseContext.Accounts.FindAsync(manager.HostAccountID);

        // Validate Host Account Against Database
        if (hostAccount is null)
        {
            Log.Warning(
                @"Could Not Find Host Account ID ""{HostAccountID}"" For Match Server Manager ID ""{ServerManagerID}"")",
                manager.HostAccountID, requestData.ServerManagerID);

            ChatBuffer rejectResponse = new();

            rejectResponse.WriteCommand(ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_REJECT);
            rejectResponse.WriteString("Match Server Manager Host Account Not Found");

            session.Send(rejectResponse);
            await session.TerminateMatchServerManager(distributedCacheStore);

            return;
        }

        // Set Host Account On Match Server Manager Session
        session.Account = hostAccount;

        // Validate Match Hosting Permissions
        if (hostAccount.Type != AccountType.ServerHost)
        {
            Log.Warning(
                @"Host Account ID ""{HostAccountID}"" For Match Server Manager ID ""{ServerManagerID}"" Does Not Have Match Hosting Permissions",
                requestData.ServerManagerID, manager.HostAccountID);

            ChatBuffer rejectResponse = new();

            rejectResponse.WriteCommand(ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_REJECT);
            rejectResponse.WriteString("The Match Server Manager Host Account Does Not Have Match Hosting Permissions");

            session.Send(rejectResponse);
            await session.TerminateMatchServerManager(distributedCacheStore);

            return;
        }

        // Validate Host Account ID Match
        if (hostAccount.ID != manager.HostAccountID)
        {
            Log.Warning(
                @"Match Server Manager Host Account ID ""{ReceivedHostAccountID}"" Does Not Match The Host Account ID ""{ExpectedHostAccountID}"" For Match Server Manager ID ""{ServerManagerID}""",
                manager.HostAccountID, hostAccount.ID, requestData.ServerManagerID);

            ChatBuffer rejectResponse = new();

            rejectResponse.WriteCommand(ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_REJECT);
            rejectResponse.WriteString(
                $@"Match Server Manager Host Account ID Mismatch: Received ""{manager.HostAccountID}""");

            session.Send(rejectResponse);
            await session.TerminateMatchServerManager(distributedCacheStore);

            return;
        }

        // Check For Duplicate Match Server Manager Instances
        if (Context.MatchServerManagerChatSessions.TryGetValue(requestData.ServerManagerID,
                out ChatSession? existingSession))
        {
            Log.Information(
                @"Disconnecting Duplicate Match Server Manager Instance With ID ""{ServerID}"" And Address ""{Address}"")",
                requestData.ServerManagerID, manager.IPAddress);

            await existingSession.TerminateMatchServerManager(distributedCacheStore);

            Context.MatchServerManagerChatSessions.TryRemove(requestData.ServerManagerID, out _);
        }

        // Register Match Server Manager
        Context.MatchServerManagerChatSessions[requestData.ServerManagerID] = session;

        Log.Information(
            @"Match Server Manager Connection Accepted - Manager ID: ""{ManagerID}"", Host Account: ""{HostAccountName}"", Address: ""{Address}""",
            requestData.ServerManagerID, manager.HostAccountName, manager.IPAddress);

        ChatBuffer acceptResponse = new();

        acceptResponse.WriteCommand(ChatProtocol.ChatServerToServerManager.NET_CHAT_SM_ACCEPT);

        session.Send(acceptResponse);

        // Send Match Server Manager Options
        ChatBuffer optionsResponse = new();

        optionsResponse.WriteCommand(ChatProtocol.ChatServerToServerManager.NET_CHAT_SM_OPTIONS);

        optionsResponse.WriteInt8(Convert.ToByte(true)); // Submit Stats Enabled
        optionsResponse.WriteInt8(Convert.ToByte(true)); // Upload Replays Enabled
        optionsResponse.WriteInt8(Convert.ToByte(false)); // Upload To FTP On Demand Enabled
        optionsResponse.WriteInt8(Convert.ToByte(true)); // Upload To HTTP On Demand Enabled
        optionsResponse.WriteInt8(Convert.ToByte(true)); // Resubmit Stats Enabled
        optionsResponse.WriteInt32(1); // Stats Resubmit Match ID Cut-Off (Minimum Match ID For Resubmission)

        // TODO: Set Stats Resubmit Match ID Cut-Off To Highest Match ID Stored In The Database

        session.Send(optionsResponse);

        Log.Debug(
            @"Match Server Manager ID ""{ManagerID}"" Options - Statistics: Enabled, Replays: Enabled, HTTP Upload: Enabled",
            requestData.ServerManagerID);
    }
}