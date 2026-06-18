namespace TRANSMUTANSTEIN.ChatServer.Services;

/// <summary>
///     Rate-limits the actions a client can spam, so that no single account can flood the chat server.
///     Each account has its own token bucket and an action consumes one token.
///     The bucket refills by one token every <see cref="ChatProtocol.FLOOD_DECAY_INTERVAL_SECONDS"/> seconds up to a capacity of <see cref="ChatProtocol.FLOOD_THRESHOLD"/> tokens, and an action attempted with no tokens left is rejected.
/// </summary>
public sealed class FloodPreventionService(ILogger<FloodPreventionService> logger) : IAsyncDisposable
{
    private PartitionedRateLimiter<int> RateLimiter { get; } = PartitionedRateLimiter.Create<int, int>(accountID =>
        RateLimitPartition.GetTokenBucketLimiter(accountID, _ => new TokenBucketRateLimiterOptions
        {
            TokenLimit = ChatProtocol.FLOOD_THRESHOLD,
            TokensPerPeriod = 1,
            ReplenishmentPeriod = TimeSpan.FromSeconds(ChatProtocol.FLOOD_DECAY_INTERVAL_SECONDS),
            QueueLimit = 0,
            AutoReplenishment = true
        }));

    /// <summary>
    ///     Determines whether the account may perform a rate-limited action right now, consuming one token if so.
    ///     Staff accounts are exempt for moderation and administration purposes.
    /// </summary>
    public bool RequestIsAllowed(int accountID, AccountType accountType)
    {
        // Staff Accounts Are Exempt From Flood Prevention For Moderation And Administration Purposes
        if (accountType is AccountType.Staff)
            return true;

        using RateLimitLease lease = RateLimiter.AttemptAcquire(accountID);

        return lease.IsAcquired;
    }

    /// <summary>
    ///     Checks whether the session may perform a rate-limited action.
    ///     When it may not, a flood warning is sent to the client via <see cref="ChatProtocol.Command.CHAT_CMD_FLOODING"/> and <see langword="false"/> is returned, otherwise <see langword="true"/> is returned.
    /// </summary>
    public bool CheckAndHandleFloodPrevention(ClientChatSession session)
    {
        if (RequestIsAllowed(session.Account.ID, session.Account.Type))
            return true;

        logger.LogWarning("Account {AccountID} Exceeded The Flood Threshold Of {Threshold} Actions", session.Account.ID, ChatProtocol.FLOOD_THRESHOLD);

        ChatBuffer floodWarning = new ();

        floodWarning.WriteCommand(ChatProtocol.Command.CHAT_CMD_FLOODING);

        session.Send(floodWarning);

        return false;
    }

    public async ValueTask DisposeAsync()
    {
        await RateLimiter.DisposeAsync();
    }
}
