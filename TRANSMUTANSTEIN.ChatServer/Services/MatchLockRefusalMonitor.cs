namespace TRANSMUTANSTEIN.ChatServer.Services;

/// <summary>
///     Subscribes to <see cref="DistributedCacheExtensions.MatchLockRefusalChannel"/> on the distributed cache and relays every refusal which the master server publishes to <see cref="MatchLockService.HandleRefusal"/>.
///     Neither a locked player's refused server list nor a match server which is refused a locked player's connection sends a chat message about it, so this relay is what sends the player back to the locked match, or frees them when they can no longer return to it.
/// </summary>
public class MatchLockRefusalMonitor(IConnectionMultiplexer distributedCacheProvider, MatchLockService matchLockService, ILogger<MatchLockRefusalMonitor> logger) : IHostedService
{
    private ChannelMessageQueue? RefusalQueue { get; set; }

    private static readonly RedisChannel MatchLockRefusalChannel = RedisChannel.Literal(DistributedCacheExtensions.MatchLockRefusalChannel);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        ISubscriber subscriber = distributedCacheProvider.GetSubscriber();

        RefusalQueue = await subscriber.SubscribeAsync(MatchLockRefusalChannel);

        RefusalQueue.OnMessage(HandleMatchLockRefusal);

        logger.LogInformation(@"Subscribed To Match Lock Refusal Event Channel ""{Channel}""", DistributedCacheExtensions.MatchLockRefusalChannel);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (RefusalQueue is null)
            return;

        await RefusalQueue.UnsubscribeAsync();

        logger.LogInformation(@"Unsubscribed From Match Lock Refusal Event Channel ""{Channel}""", DistributedCacheExtensions.MatchLockRefusalChannel);
    }

    private async Task HandleMatchLockRefusal(ChannelMessage channelMessage)
    {
        MatchLockRefusal? refusal;

        try
        {
            refusal = channelMessage.Message.IsNullOrEmpty ? null : JsonSerializer.Deserialize<MatchLockRefusal>(channelMessage.Message.ToString());
        }

        catch (JsonException exception)
        {
            logger.LogWarning(exception, @"Received Malformed Payload ""{Payload}"" On Match Lock Refusal Event Channel ""{Channel}""", channelMessage.Message.ToString(), DistributedCacheExtensions.MatchLockRefusalChannel);

            return;
        }

        if (refusal is null)
        {
            logger.LogWarning(@"Received Empty Or Null Payload On Match Lock Refusal Event Channel ""{Channel}""", DistributedCacheExtensions.MatchLockRefusalChannel);

            return;
        }

        // Every Failure Is Caught Here, Because The Subscription Discards A Handler's Exception Without Logging It
        try
        {
            await matchLockService.HandleRefusal(refusal);
        }

        catch (Exception exception)
        {
            logger.LogError(exception, @"Failed To Handle The Match Lock Refusal Of Account ""{AccountName}""", refusal.AccountName);
        }
    }
}
