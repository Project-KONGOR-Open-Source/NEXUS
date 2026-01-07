namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

[ChatCommand(ChatProtocol.ClientToChatServer.NET_CHAT_CL_CONNECT)]
public class ClientHandshake(MerrickContext merrick, IDatabase distributedCacheStore) : IAsynchronousCommandProcessor<ClientChatSession>
{
    public async Task Process(ClientChatSession session, ChatBuffer buffer)
    {
        ClientHandshakeRequestData requestData = new (buffer);

        // Set Client Metadata On Session
        // This Needs To Be Set At The First Opportunity So That Any Subsequent Code Logic Can Have Access To The Client's Metadata
        session.Metadata = requestData.ToMetadata();

        string? cachedAccountName = await distributedCacheStore.GetAccountNameForSessionCookie(requestData.SessionCookie);

        // Ensure Session Cookie Exists In Cache
        if (cachedAccountName is null)
        {
            Log.Warning(@"Authentication Failed For Account ID ""{RequestData.AccountID}"": Session Cookie ""{RequestData.SessionCookie}"" Not Found In Cache",
                requestData.AccountID, requestData.SessionCookie);

            session
                .Reject(ChatProtocol.ChatRejectReason.ECR_AUTH_FAILED)
                .Terminate();

            return;
        }

        Account? account = await merrick.Accounts
            .Include(account => account.User).ThenInclude(user => user.Accounts)
            .Include(account => account.FriendedPeers)
            .Include(account => account.Clan).ThenInclude(clan => clan!.Members)
            .SingleOrDefaultAsync(account => account.ID == requestData.AccountID);

        if (account is null)
        {
            Log.Error(@"[BUG] Account With ID ""{RequestData.AccountID}"" Could Not Be Found", requestData.AccountID);

            session
                .Reject(ChatProtocol.ChatRejectReason.ECR_UNKNOWN)
                .Terminate();

            return;
        }

        // Ensure Account Name Matches Cached Account Name From Session Cookie
        if (account.Name.Equals(cachedAccountName, StringComparison.OrdinalIgnoreCase).Equals(false))
        {
            Log.Warning(@"Authentication Failed: Account ID ""{RequestData.AccountID}"" Does Not Match Cached Account Name ""{CachedAccountName}""",
                requestData.AccountID, cachedAccountName);

            session
                .Reject(ChatProtocol.ChatRejectReason.ECR_AUTH_FAILED)
                .Terminate();

            return;
        }

        // Ensure Authentication Hash (AccountID + RemoteIP + Cookie + Salt) Matches Expected Value
        string expectedSessionAuthenticationHash = SRPAuthenticationHandlers.ComputeChatServerCookieHash(requestData.AccountID, requestData.RemoteIP, requestData.SessionCookie);

        if (requestData.SessionAuthenticationHash.Equals(expectedSessionAuthenticationHash, StringComparison.OrdinalIgnoreCase).Equals(false))
        {
            Log.Warning(@"Authentication Failed For Account ""{Account.Name}"": Invalid Authentication Hash", account.Name);

            session
                .Reject(ChatProtocol.ChatRejectReason.ECR_AUTH_FAILED)
                .Terminate();

            return;
        }

        // Check For Concurrent Connections: Disconnect Any Other Existing Sessions For This Account Or For Any Sub-Account Of The Same User
        // Ignore Staff And Guest Accounts For Concurrent Connection Checks
        if (account.Type is not AccountType.Staff and not AccountType.Guest)
        {
            List<int> subAccountIDs = [.. account.User.Accounts.Select(subAccount => subAccount.ID)];

            foreach (int subAccountID in subAccountIDs)
            {
                ClientChatSession? existingSessionMatch = Context.ClientChatSessions.Values
                    .SingleOrDefault(existingSession => existingSession.Account?.ID == subAccountID);

                if (existingSessionMatch is not null)
                {
                    Log.Information(@"Disconnecting Existing Session For Account ID ""{SubAccountID}"" (Account ""{ExistingSessionMatch.Account.Name}"") Due To Concurrent Connection Attempt",
                        subAccountID, existingSessionMatch.Account.Name);

                    existingSessionMatch
                        .Reject(ChatProtocol.ChatRejectReason.ECR_ACCOUNT_SHARING)
                        .Terminate();
                }
            }
        }

        if (Context.ClientChatSessions.Values.SingleOrDefault(existingSession => existingSession.Account?.ID == account.ID) is { } existingSession)
        {
            Log.Information(@"Disconnecting Existing Session For Account ""{Account.Name}"" Due To Concurrent Connection Attempt", account.Name);

            existingSession
                .Reject(ChatProtocol.ChatRejectReason.ECR_ACCOUNT_SHARING)
                .Terminate();
        }

        // Validate Client Version
        if ($"{requestData.ClientVersionMajor}.{requestData.ClientVersionMinor}.{requestData.ClientVersionPatch}.{requestData.ClientVersionRevision}" is not "4.10.1.0")
        {
            session
                .Reject(ChatProtocol.ChatRejectReason.ECR_BAD_VERSION)
                .Terminate();

            return;
        }

        // Accept Connection, Send Options, And Broadcast Connection To Friends And Clan Members
        session
            .Accept(account)
            .SendOptionsAndRemoteCommands()
            .BroadcastConnection();
    }
}

file class ClientHandshakeRequestData
{
    public byte[] CommandBytes { get; init; }

    public int AccountID { get; init; }

    public string SessionCookie { get; init; }

    public string RemoteIP { get; init; }

    public string SessionAuthenticationHash { get; init; }

    public int ChatProtocolVersion { get; init; }

    public byte OperatingSystemIdentifier { get; init; }

    public byte OperatingSystemVersionMajor { get; init; }

    public byte OperatingSystemVersionMinor { get; init; }

    public byte OperatingSystemVersionPatch { get; init; }

    public string OperatingSystemBuildCode { get; init; }

    public string OperatingSystemArchitecture { get; init; }

    public byte ClientVersionMajor { get; init; }

    public byte ClientVersionMinor { get; init; }

    public byte ClientVersionPatch { get; init; }

    public byte ClientVersionRevision { get; init; }

    public ChatProtocol.ChatClientStatus LastKnownClientState { get; init; }

    public ChatProtocol.ChatModeType ClientChatModeState { get; init; }

    public string ClientRegion { get; init; }

    public string ClientLanguage { get; init; }

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
