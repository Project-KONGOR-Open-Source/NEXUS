namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

using global::TRANSMUTANSTEIN.ChatServer.Internals;
using global::TRANSMUTANSTEIN.ChatServer.Domain.Communication;

[ChatCommand(ChatProtocol.ClientToChatServer.NET_CHAT_CL_CONNECT)]
public class ClientHandshake(MerrickContext merrick, IDatabase distributedCacheStore, IChatContext chatContext)
    : IAsynchronousCommandProcessor<ChatSession>
{
    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        ClientHandshakeRequestData requestData = new(buffer);

        // Set Client Metadata On Session
        // This Needs To Be Set At The First Opportunity So That Any Subsequent Code Logic Can Have Access To The Client's Metadata
        session.ClientMetadata = requestData.ToMetadata();

        string? cachedAccountName =
            await distributedCacheStore.GetAccountNameForSessionCookie(requestData.SessionCookie);

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
                    @"Authentication Failed: Account ID ""{RequestData.AccountID}"" (Name: ""{Account.Name}"") Does Not Match Cached Account Name ""{CachedAccountName}""",
                    requestData.AccountID, account.Name, cachedAccountName);

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
                    ChatSession? existingSessionMatch = chatContext.ClientChatSessions.Values
                        .SingleOrDefault(eSession => eSession.Account.ID == subAccountID);

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

            if (chatContext.ClientChatSessions.Values.SingleOrDefault(eSession =>
                    eSession.Account.ID == account.ID) is { } existingSession)
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