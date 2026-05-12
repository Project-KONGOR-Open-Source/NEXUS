namespace TRANSMUTANSTEIN.ChatServer.Services;

/// <summary>
///     Subscribes to <see cref="DistributedCacheExtensions.AccountLogoutChannel"/> on the distributed cache and force-terminates the matching chat session whenever the master server publishes an account logout notification.
///     This guarantees that a logged-out account is no longer visible to peers in chat channels or friend/clan lists, even in scenarios where the client did not close its chat socket cleanly.
/// </summary>
public class LogoutMonitor(IConnectionMultiplexer distributedCacheProvider, ILogger<LogoutMonitor> logger) : IHostedService
{
    private ISubscriber? Subscriber { get; set; }

    private static readonly RedisChannel AccountLogoutChannel = RedisChannel.Literal(DistributedCacheExtensions.AccountLogoutChannel);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Subscriber = distributedCacheProvider.GetSubscriber();

        await Subscriber.SubscribeAsync(AccountLogoutChannel, HandleAccountLogout);

        logger.LogInformation(@"Subscribed To Account Logout Channel ""{Channel}""", DistributedCacheExtensions.AccountLogoutChannel);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (Subscriber is null)
            return;

        await Subscriber.UnsubscribeAsync(AccountLogoutChannel);

        logger.LogInformation(@"Unsubscribed From Account Logout Channel ""{Channel}""", DistributedCacheExtensions.AccountLogoutChannel);
    }

    private void HandleAccountLogout(RedisChannel channel, RedisValue message)
    {
        string? accountName = message.IsNullOrEmpty ? null : message.ToString();

        if (accountName is null)
        {
            logger.LogWarning(@"Received Empty Or Null Payload On Account Logout Channel ""{Channel}""", DistributedCacheExtensions.AccountLogoutChannel);

            return;
        }

        if (Context.ClientChatSessions.TryGetValue(accountName, out ClientChatSession? session) is false)
        {
            // No Active Session For This Account; The Client Likely Already Closed Its Chat Socket
            logger.LogDebug(@"No Active Chat Session Found For Logged-Out Account ""{AccountName}""", accountName);

            return;
        }

        logger.LogInformation(@"Force-Terminating Chat Session For Logged-Out Account ""{AccountName}""", accountName);

        try
        {
            session.Terminate();
        }

        catch (Exception exception)
        {
            logger.LogError(exception, @"[BUG] Failed To Force-Terminate Chat Session For Logged-Out Account ""{AccountName}""", accountName);
        }
    }
}
