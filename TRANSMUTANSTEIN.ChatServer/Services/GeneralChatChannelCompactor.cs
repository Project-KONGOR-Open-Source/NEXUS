namespace TRANSMUTANSTEIN.ChatServer.Services;

/// <summary>
///     Periodically compacts the general chat channels, so that the members who remain as others leave are not spread thinly across many general channels.
///     See <see cref="ChatChannel.CompactGeneralChannels"/> for how the members are redistributed.
/// </summary>
public sealed class GeneralChatChannelCompactor(ILogger<GeneralChatChannelCompactor> logger) : BackgroundService
{
    private static readonly TimeSpan CompactionInterval = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (stoppingToken.IsCancellationRequested is false)
        {
            try { await Task.Delay(CompactionInterval, stoppingToken); } catch (OperationCanceledException) { return; }

            try
            {
                ChatChannel.CompactGeneralChannels();
            }

            catch (Exception exception)
            {
                logger.LogError(exception, "General Chat Channel Compaction Failed");
            }
        }
    }
}
