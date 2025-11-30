namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

[ChatCommand(ChatProtocol.ClientToChatServer.NET_CHAT_CL_CONNECT)]
public class ClientHandshake(MerrickContext merrick, IDatabase distributedCacheStore) : IAsynchronousCommandProcessor
{
    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        ClientHandshakeRequestData requestData = new (buffer);

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
        if (account.Name.Equals(cachedAccountName, StringComparison.Ordinal).Equals(false))
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

        List<int> subAccountIDs = [.. account.User.Accounts.Select(subAccount => subAccount.ID)];

        // Check For Concurrent Connections: Disconnect Any Other Existing Sessions For This Account Or For Any Sub-Account Of The Same User
        foreach (int subAccountID in subAccountIDs)
        {
            ChatSession? existingSessionMatch = Context.ChatSessions.Values
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
            .Accept(new ChatSessionMetadata(requestData), account)
            .SendOptionsAndRemoteCommands()
            .BroadcastConnection();
    }
}

public class ClientHandshakeRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();

    public int AccountID = buffer.ReadInt32();

    public string SessionCookie = buffer.ReadString();

    public string RemoteIP = buffer.ReadString();

    public string SessionAuthenticationHash = buffer.ReadString();

    public int ChatProtocolVersion = buffer.ReadInt32();

    public byte OperatingSystemIdentifier = buffer.ReadInt8();

    public byte OperatingSystemVersionMajor = buffer.ReadInt8();

    public byte OperatingSystemVersionMinor = buffer.ReadInt8();

    public byte OperatingSystemVersionPatch = buffer.ReadInt8();

    public string OperatingSystemBuildCode = buffer.ReadString();

    public string OperatingSystemArchitecture = buffer.ReadString();

    public byte ClientVersionMajor = buffer.ReadInt8();

    public byte ClientVersionMinor = buffer.ReadInt8();

    public byte ClientVersionPatch = buffer.ReadInt8();

    public byte ClientVersionRevision = buffer.ReadInt8();

    public ChatProtocol.ChatClientStatus LastKnownClientState = (ChatProtocol.ChatClientStatus) buffer.ReadInt8();

    public ChatProtocol.ChatModeType ClientChatModeState = (ChatProtocol.ChatModeType) buffer.ReadInt8();

    public string ClientRegion = buffer.ReadString();

    public string ClientLanguage = buffer.ReadString();
}
