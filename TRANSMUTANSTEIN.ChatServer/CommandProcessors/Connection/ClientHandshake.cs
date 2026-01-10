namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

[ChatCommand(ChatProtocol.ClientToChatServer.NET_CHAT_CL_CONNECT)]
public class ClientHandshake(MerrickContext merrick, IDatabase distributedCacheStore)
    : IAsynchronousCommandProcessor<ChatSession>
{
    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        Log.Information($"[DEBUG] ClientHandshake.Process ENTERED for Session {session.ID}");

        ClientHandshakeRequestData requestData = new(buffer);
        Log.Information($"[DEBUG] Request Data Parsed. AccountID: {requestData.AccountID}");

        // Set Client Metadata On Session
        // This Needs To Be Set At The First Opportunity So That Any Subsequent Code Logic Can Have Access To The Client's Metadata
        session.ClientMetadata = requestData.ToMetadata();

        Log.Information($"[DEBUG] Querying Redis for SessionCookie: {requestData.SessionCookie}");
        Log.Information($"[DEBUG] Querying Redis for SessionCookie: {requestData.SessionCookie}");
        string? cachedAccountName =
            await distributedCacheStore.GetAccountNameForSessionCookie(requestData.SessionCookie);
        Log.Information($"[DEBUG] Redis Result: {cachedAccountName ?? "NULL"}");

        // Ensure Session Cookie Exists In Cache
        if (cachedAccountName is null)
        {
            Log.Warning(
                @"Authentication Failed For Account ID ""{RequestData.AccountID}"": Session Cookie ""{RequestData.SessionCookie}"" Not Found In Cache",
                requestData.AccountID, requestData.SessionCookie);

            session
                .Reject(ChatProtocol.ChatRejectReason.ECR_AUTH_FAILED)
                .TerminateClient();

            return;
        }

        try
        {
            Account? account = await merrick.Accounts
                .Include(account => account.User).ThenInclude(user => user.Accounts)
                .Include(account => account.FriendedPeers)
                .Include(account => account.Clan).ThenInclude(clan => clan!.Members)
                .SingleOrDefaultAsync(account => account.ID == requestData.AccountID);
            Log.Information($"[DEBUG] SQL Result: {(account != null ? "FOUND" : "NULL")}");

            if (account is null)
            {
                Log.Error(@"[BUG] Account With ID ""{RequestData.AccountID}"" Could Not Be Found",
                    requestData.AccountID);

                session
                    .Reject(ChatProtocol.ChatRejectReason.ECR_UNKNOWN)
                    .TerminateClient();

                return;
            }

            // Ensure Account Name Matches Cached Account Name From Session Cookie
            if (account.Name.Equals(cachedAccountName, StringComparison.OrdinalIgnoreCase).Equals(false))
            {
                Log.Warning(
                    @"Authentication Failed: Account ID ""{RequestData.AccountID}"" Does Not Match Cached Account Name ""{CachedAccountName}""",
                    requestData.AccountID, cachedAccountName);

                session
                    .Reject(ChatProtocol.ChatRejectReason.ECR_AUTH_FAILED)
                    .TerminateClient();

                return;
            }

            // Ensure Authentication Hash (AccountID + RemoteIP + Cookie + Salt) Matches Expected Value
            string expectedSessionAuthenticationHash =
                SRPAuthenticationHandlers.ComputeChatServerCookieHash(requestData.AccountID, requestData.RemoteIP,
                    requestData.SessionCookie);

            if (requestData.SessionAuthenticationHash
                .Equals(expectedSessionAuthenticationHash, StringComparison.OrdinalIgnoreCase).Equals(false))
            {
                Log.Warning(@"Authentication Failed For Account ""{Account.Name}"": Invalid Authentication Hash",
                    account.Name);

                session
                    .Reject(ChatProtocol.ChatRejectReason.ECR_AUTH_FAILED)
                    .TerminateClient();

                return;
            }

            // Check For Concurrent Connections: Disconnect Any Other Existing Sessions For This Account Or For Any Sub-Account Of The Same User
            // Ignore Staff And Guest Accounts For Concurrent Connection Checks
            if (account.Type is not AccountType.Staff and not AccountType.Guest)
            {
                List<int> subAccountIDs = [.. account.User.Accounts.Select(subAccount => subAccount.ID)];

                foreach (int subAccountID in subAccountIDs)
                {
                    ChatSession? existingSessionMatch = Context.ClientChatSessions.Values
                        .SingleOrDefault(existingSession => existingSession.Account?.ID == subAccountID);

                    if (existingSessionMatch is not null)
                    {
                        Log.Information(
                            @"Disconnecting Existing Session For Account ID ""{SubAccountID}"" (Account ""{ExistingSessionMatch.Account.Name}"") Due To Concurrent Connection Attempt",
                            subAccountID, existingSessionMatch.Account.Name);

                        existingSessionMatch
                            .Reject(ChatProtocol.ChatRejectReason.ECR_ACCOUNT_SHARING)
                            .TerminateClient();
                    }
                }
            }

            if (Context.ClientChatSessions.Values.SingleOrDefault(existingSession =>
                    existingSession.Account?.ID == account.ID) is { } existingSession)
            {
                Log.Information(
                    @"Disconnecting Existing Session For Account ""{Account.Name}"" Due To Concurrent Connection Attempt",
                    account.Name);

                existingSession
                    .Reject(ChatProtocol.ChatRejectReason.ECR_ACCOUNT_SHARING)
                    .TerminateClient();
            }

            // Validate Client Version
            if ($"{requestData.ClientVersionMajor}.{requestData.ClientVersionMinor}.{requestData.ClientVersionPatch}.{requestData.ClientVersionRevision}"
                is not "4.10.1.0")
            {
                Log.Warning("Authentication Failed: Bad Version {Version}",
                    $"{requestData.ClientVersionMajor}.{requestData.ClientVersionMinor}.{requestData.ClientVersionPatch}.{requestData.ClientVersionRevision}");
                session
                    .Reject(ChatProtocol.ChatRejectReason.ECR_BAD_VERSION)
                    .TerminateClient();

                return;
            }

            Log.Information("Authentication Successful For Account {AccountID}. Accepting Connection.",
                requestData.AccountID);

            // Accept Connection, Send Options, And Broadcast Connection To Friends And Clan Members
            session
                .Accept(account);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[CRITICAL] ClientHandshake Processing Failed!");
            throw;
        }
    }
}

file class ClientHandshakeRequestData
{
    public ClientHandshakeRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        AccountID = buffer.ReadInt32();
        SessionCookie = buffer.ReadString();
        RemoteIP = buffer.ReadString();
        SessionAuthenticationHash = buffer.ReadString();
        ChatProtocolVersion = buffer.ReadInt32();
        OperatingSystemIdentifier = buffer.ReadInt8();
        OperatingSystemVersionMajor = buffer.ReadInt8();
        OperatingSystemVersionMinor = buffer.ReadInt8();
        OperatingSystemVersionPatch = buffer.ReadInt8();
        OperatingSystemBuildCode = buffer.ReadString();
        OperatingSystemArchitecture = buffer.ReadString();
        ClientVersionMajor = buffer.ReadInt8();
        ClientVersionMinor = buffer.ReadInt8();
        ClientVersionPatch = buffer.ReadInt8();
        ClientVersionRevision = buffer.ReadInt8();
        LastKnownClientState = (ChatProtocol.ChatClientStatus) buffer.ReadInt8();
        ClientChatModeState = (ChatProtocol.ChatModeType) buffer.ReadInt8();
        ClientRegion = buffer.ReadString();
        ClientLanguage = buffer.ReadString();
    }

    public byte[] CommandBytes { get; init; }

    public int AccountID { get; }

    public string SessionCookie { get; }

    public string RemoteIP { get; }

    public string SessionAuthenticationHash { get; }

    public int ChatProtocolVersion { get; }

    public byte OperatingSystemIdentifier { get; }

    public byte OperatingSystemVersionMajor { get; }

    public byte OperatingSystemVersionMinor { get; }

    public byte OperatingSystemVersionPatch { get; }

    public string OperatingSystemBuildCode { get; }

    public string OperatingSystemArchitecture { get; }

    public byte ClientVersionMajor { get; }

    public byte ClientVersionMinor { get; }

    public byte ClientVersionPatch { get; }

    public byte ClientVersionRevision { get; }

    public ChatProtocol.ChatClientStatus LastKnownClientState { get; }

    public ChatProtocol.ChatModeType ClientChatModeState { get; }

    public string ClientRegion { get; }

    public string ClientLanguage { get; }

    public ClientChatSessionMetadata ToMetadata()
    {
        return new ClientChatSessionMetadata
        {
            RemoteIP = RemoteIP,
            SessionCookie = SessionCookie,
            SessionAuthenticationHash = SessionAuthenticationHash,
            ChatProtocolVersion = ChatProtocolVersion,
            OperatingSystemIdentifier = OperatingSystemIdentifier,
            OperatingSystemVersionMajor = OperatingSystemVersionMajor,
            OperatingSystemVersionMinor = OperatingSystemVersionMinor,
            OperatingSystemVersionPatch = OperatingSystemVersionPatch,
            OperatingSystemBuildCode = OperatingSystemBuildCode,
            OperatingSystemArchitecture = OperatingSystemArchitecture,
            ClientVersionMajor = ClientVersionMajor,
            ClientVersionMinor = ClientVersionMinor,
            ClientVersionPatch = ClientVersionPatch,
            ClientVersionRevision = ClientVersionRevision,
            LastKnownClientState = LastKnownClientState,
            ClientChatModeState = ClientChatModeState,
            ClientRegion = ClientRegion,
            ClientLanguage = ClientLanguage
        };
    }
}