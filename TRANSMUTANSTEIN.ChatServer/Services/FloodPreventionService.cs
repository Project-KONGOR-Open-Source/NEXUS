namespace TRANSMUTANSTEIN.ChatServer.Services;

/// <summary>
///     Prevents flooding by tracking and rate-limiting user requests.
///     Request counter increments on each action and decays over time.
///     Requests exceeding the threshold are rejected with a warning sent to the client.
/// </summary>
public class FloodPreventionService(ILogger<FloodPreventionService> logger) : IHostedService, IDisposable
{
    private ConcurrentDictionary<int, FloodState> AccountFloodStates { get; } = new ();

    private Timer? DecayTimer { get; set; }

    /// <summary>
    ///     Checks if a message is allowed for the given session and handles flood prevention response.
    ///     Staff accounts are automatically exempted from flood prevention.
    ///     If the request count exceeds <see cref="ChatProtocol.FLOOD_THRESHOLD"/>, sends a warning to the client via <see cref="ChatProtocol.Command.CHAT_CMD_FLOODING"/> and returns FALSE.
    ///     Otherwise, increments the request counter and returns TRUE.
    /// </summary>
    public bool CheckAndHandleFloodPrevention(ClientChatSession session)
    {
        // Staff Accounts Are Exempt From Flood Prevention For Moderation And Administration Purposes
        if (session.Account.Type is AccountType.Staff)
        {
            logger.LogDebug("Account {AccountID} Exempt From Flood Prevention (Staff Account)", session.Account.ID);

            return true;
        }

        FloodState state = AccountFloodStates.GetOrAdd(session.Account.ID, _ => new FloodState());

        lock (state.Lock)
        {
            // Check If Request Count Exceeds Threshold
            if (state.RequestCount > ChatProtocol.FLOOD_THRESHOLD)
            {
                logger.LogWarning("Account {AccountID} Exceeded Flood Threshold ({Threshold}), Request Count: {RequestCount}",
                    session.Account.ID, ChatProtocol.FLOOD_THRESHOLD, state.RequestCount);

                ChatBuffer floodWarning = new ();

                floodWarning.WriteCommand(ChatProtocol.Command.CHAT_CMD_FLOODING);

                // Send Flood Warning To Client
                session.Send(floodWarning);

                return false;
            }

            // Increment Request Counter
            state.RequestCount++;

            // Update Last Request Time
            state.LastRequestTime = DateTime.UtcNow;

            return true;
        }
    }

    /// <summary>
    ///     Starts the background decay timer that reduces request counts over time.
    ///     The timer runs every <see cref="ChatProtocol.FLOOD_DECAY_INTERVAL_SECONDS"/> seconds and decrements each account's request count by one.
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Flood Prevention Service Starting With Threshold: {Threshold}, Decay Interval: {DecayInterval}s",
            ChatProtocol.FLOOD_THRESHOLD, ChatProtocol.FLOOD_DECAY_INTERVAL_SECONDS);

        // Start Decay Timer
        DecayTimer = new Timer
        (
            DecayRequestCounts,
            state: null,
            TimeSpan.FromSeconds(ChatProtocol.FLOOD_DECAY_INTERVAL_SECONDS),
            TimeSpan.FromSeconds(ChatProtocol.FLOOD_DECAY_INTERVAL_SECONDS)
        );

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Stops the background decay timer.
    /// </summary>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Flood Prevention Service Stopping");

        DecayTimer?.Change(Timeout.Infinite, Timeout.Infinite);

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Periodically decays request counts by one for all accounts.
    ///     This method is called every <see cref="ChatProtocol.FLOOD_DECAY_INTERVAL_SECONDS"/> seconds by the background timer.
    ///     Accounts with zero request count and no recent activity in <see cref="ChatProtocol.FLOOD_GARBAGE_COLLECTION_SECONDS"/> are removed, to prevent memory leaks.
    /// </summary>
    private void DecayRequestCounts(object? state)
    {
        int decayedAccounts = 0;
        int removedAccounts = 0;

        foreach (KeyValuePair<int, FloodState> entry in AccountFloodStates)
        {
            FloodState floodState = entry.Value;

            lock (floodState.Lock)
            {
                // Decay Request Count By One
                if (floodState.RequestCount > 0)
                {
                    floodState.RequestCount--;

                    decayedAccounts++;
                }

                // Remove Accounts With Zero Request Count And No Recent Activity, To Prevent Memory Leaks
                if (floodState.RequestCount == 0 && (DateTime.UtcNow - floodState.LastRequestTime).TotalSeconds > ChatProtocol.FLOOD_GARBAGE_COLLECTION_SECONDS)
                {
                    AccountFloodStates.TryRemove(entry.Key, out _);

                    removedAccounts++;
                }
            }
        }

        if (decayedAccounts > 0 || removedAccounts > 0)
        {
            logger.LogDebug("Flood Prevention Decay: {DecayedCount} Accounts Decayed, {RemovedCount} Accounts Cleaned Up",
                decayedAccounts, removedAccounts);
        }
    }

    public void Dispose()
    {
        DecayTimer?.Dispose();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Represents the flood state for a single account.
    /// </summary>
    private class FloodState
    {
        public int RequestCount { get; set; } = 0;

        public DateTime LastRequestTime { get; set; } = DateTime.UtcNow;

        public Lock Lock { get; } = new ();
    }
}
