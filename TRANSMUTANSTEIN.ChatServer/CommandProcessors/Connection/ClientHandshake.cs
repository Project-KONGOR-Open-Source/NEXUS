namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

[ChatCommand(ChatProtocol.ClientToChatServer.NET_CHAT_CL_CONNECT)]
public class ClientHandshake(MerrickContext merrick, IDatabase distributedCacheStore) : IAsynchronousCommandProcessor<ClientChatSession>
{
    /// <summary>
    ///     The sender name which the notice to a session that has just been logged out elsewhere carries.
    /// </summary>
    internal const string LoggedInElsewhereSenderName = "Login";

    /// <summary>
    ///     The notice text for a session that has been logged out because the user logged in elsewhere, naming the account which has just logged in.
    /// </summary>
    internal static string LoggedInElsewhereMessage(string accountName) => $@"You have logged in as ""{accountName}"" elsewhere, so you have been logged out here. Any game in progress here carries on.";

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
            Log.Warning(@"Authentication Failed For Account ID ""{AccountID}"": Session Cookie ""{SessionCookie}"" Not Found In Cache",
                requestData.AccountID, requestData.SessionCookie);

            await session
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
            Log.Error(@"[BUG] Account With ID ""{AccountID}"" Could Not Be Found", requestData.AccountID);

            await session
                .Reject(ChatProtocol.ChatRejectReason.ECR_UNKNOWN)
                .Terminate();

            return;
        }

        // Ensure Account Name Matches Cached Account Name From Session Cookie
        if (account.Name.Equals(cachedAccountName, StringComparison.Ordinal).Equals(false))
        {
            Log.Warning(@"Authentication Failed: Account ID ""{AccountID}"" Does Not Match Cached Account Name ""{CachedAccountName}""",
                requestData.AccountID, cachedAccountName);

            await session
                .Reject(ChatProtocol.ChatRejectReason.ECR_AUTH_FAILED)
                .Terminate();

            return;
        }

        // Ensure Authentication Hash (AccountID + RemoteIP + Cookie + Salt) Matches Expected Value
        string expectedSessionAuthenticationHash = SRPAuthenticationHandlers.ComputeChatServerCookieHash(requestData.AccountID, requestData.RemoteIP, requestData.SessionCookie);

        if (requestData.SessionAuthenticationHash.Equals(expectedSessionAuthenticationHash, StringComparison.OrdinalIgnoreCase).Equals(false))
        {
            Log.Warning(@"Authentication Failed For Account ""{AccountName}"": Invalid Authentication Hash", account.Name);

            await session
                .Reject(ChatProtocol.ChatRejectReason.ECR_AUTH_FAILED)
                .Terminate();

            return;
        }

        // The Game Client's Text For The Account Sharing Reject Talks About Sharing Linked Sub-Accounts, So A Plain Notice Precedes It
        // The Client Handles Frames In Order And Shows The Notice In The Same Chat Panes, So It Reads Before The Reject
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
                    Log.Information(@"Disconnecting Existing Session For Account ID ""{ExistingSessionAccountID}"" (Account ""{ExistingSessionAccountName}"") Due To Concurrent Connection Attempt",
                        subAccountID, existingSessionMatch.Account.Name);

                    await existingSessionMatch
                        .SendNotice(LoggedInElsewhereSenderName, LoggedInElsewhereMessage(account.Name))
                        .Reject(ChatProtocol.ChatRejectReason.ECR_ACCOUNT_SHARING)
                        .Terminate();
                }
            }
        }

        if (Context.ClientChatSessions.Values.SingleOrDefault(existingSession => existingSession.Account?.ID == account.ID) is { } existingSession)
        {
            Log.Information(@"Disconnecting Existing Session For Account ""{AccountName}"" Due To Concurrent Connection Attempt", account.Name);

            await existingSession
                .SendNotice(LoggedInElsewhereSenderName, LoggedInElsewhereMessage(account.Name))
                .Reject(ChatProtocol.ChatRejectReason.ECR_ACCOUNT_SHARING)
                .Terminate();
        }

        // Validate Client Version
        if ($"{requestData.ClientVersionMajor}.{requestData.ClientVersionMinor}.{requestData.ClientVersionPatch}.{requestData.ClientVersionRevision}" is not "4.10.1.0")
        {
            await session
                .Reject(ChatProtocol.ChatRejectReason.ECR_BAD_VERSION)
                .Terminate();

            return;
        }

        // Accept Connection, Send Options, Then Complete The Connection
        session
            .Accept(account)
            .SendOptionsAndRemoteCommands()
            .SetOnline();
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

    /// <summary>
    ///     The crash reporting state from the client's previous session.
    ///     The client writes its current stage to a local file as it progresses (idle → connecting → loading → in-game, etc.).
    ///     On next launch, it reads the file back: if the client exited cleanly the value is <see cref="ChatProtocol.CrashReportingClientState.CRCS_NO_CRASH"/>, otherwise it reports whichever stage it was in when it crashed.
    ///     The original chat server used this value solely for aggregate telemetry counters keyed by crash state, OS version, and account ID.
    /// </summary>
    public ChatProtocol.CrashReportingClientState CrashReportClientState { get; init; }

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
        CrashReportClientState = (ChatProtocol.CrashReportingClientState) buffer.ReadInt8();
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
            LastKnownClientState = ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_DISCONNECTED,
            ClientChatModeState = ClientChatModeState,
            ClientRegion = ClientRegion,
            ClientLanguage = ClientLanguage
        };
    }
}
