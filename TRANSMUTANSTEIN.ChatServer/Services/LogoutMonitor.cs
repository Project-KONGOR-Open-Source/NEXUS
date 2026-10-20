namespace TRANSMUTANSTEIN.ChatServer.Services;

/// <summary>
///     Subscribes to <see cref="DistributedCacheExtensions.AccountLogoutChannel"/> on the distributed cache and force-terminates the chat session which used the logout's session cookie whenever the master server publishes an account logout notification.
///     A logout ends only the session which used its cookie, because the newer login of the same account must survive the old machine's logout.
///     This guarantees that a logged-out account is no longer visible to peers in chat channels or friend/clan lists, even in scenarios where the client did not close its chat socket cleanly.
/// </summary>
public class LogoutMonitor(IConnectionMultiplexer distributedCacheProvider, ILogger<LogoutMonitor> logger) : IHostedService
{
    private ChannelMessageQueue? LogoutQueue { get; set; }

    private static readonly RedisChannel AccountLogoutChannel = RedisChannel.Literal(DistributedCacheExtensions.AccountLogoutChannel);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        ISubscriber subscriber = distributedCacheProvider.GetSubscriber();

        LogoutQueue = await subscriber.SubscribeAsync(AccountLogoutChannel);

        LogoutQueue.OnMessage(HandleAccountLogout);

        logger.LogInformation(@"Subscribed To Account Logout Event Channel ""{Channel}""", DistributedCacheExtensions.AccountLogoutChannel);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (LogoutQueue is null)
            return;

        await LogoutQueue.UnsubscribeAsync();

        logger.LogInformation(@"Unsubscribed From Account Logout Event Channel ""{Channel}""", DistributedCacheExtensions.AccountLogoutChannel);
    }

    private async Task HandleAccountLogout(ChannelMessage channelMessage)
    {
        AccountLogout? logout;

        try
        {
            logout = channelMessage.Message.IsNullOrEmpty ? null : JsonSerializer.Deserialize<AccountLogout>(channelMessage.Message.ToString());
        }

        catch (JsonException exception)
        {
            logger.LogWarning(exception, @"Received Malformed Payload ""{Payload}"" On Account Logout Event Channel ""{Channel}""", channelMessage.Message.ToString(), DistributedCacheExtensions.AccountLogoutChannel);

            return;
        }

        if (logout is null)
        {
            logger.LogWarning(@"Received Empty Or Null Payload On Account Logout Event Channel ""{Channel}""", DistributedCacheExtensions.AccountLogoutChannel);

            return;
        }

        string accountName = logout.AccountName;

        if (Context.ClientChatSessions.TryGetValue(accountName, out ClientChatSession? session) is false)
        {
            // No Active Session For This Account; The Client Likely Already Closed Its Chat Socket
            logger.LogDebug(@"No Active Chat Session Found For Logged-Out Account ""{AccountName}""", accountName);

            return;
        }

        if (string.Equals(session.Metadata.SessionCookie, logout.SessionCookie, StringComparison.Ordinal) is false)
        {
            // The Account Has Logged In Again Since, So This Is The Old Machine's Logout And The Newer Session Must Survive It
            logger.LogDebug(@"Chat Session Of Account ""{AccountName}"" Used Another Session Cookie Than The Logout, So It Is Left Alone", accountName);

            return;
        }

        logger.LogInformation(@"Force-Terminating Chat Session For Logged-Out Account ""{AccountName}""", accountName);

        try
        {
            await session.Terminate();
        }

        catch (Exception exception)
        {
            logger.LogError(exception, @"[BUG] Failed To Force-Terminate Chat Session For Logged-Out Account ""{AccountName}""", accountName);
        }
    }
}
