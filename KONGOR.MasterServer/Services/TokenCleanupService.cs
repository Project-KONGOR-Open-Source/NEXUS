namespace KONGOR.MasterServer.Services;

/// <summary>
///     Background service that periodically purges expired <see cref="Token"/> rows from the database.
///     A token is considered expired when it has not yet been consumed and the sum of <see cref="Token.TimestampCreated"/> and <see cref="Token.Validity"/> is in the past.
///     Consumed tokens are retained for audit purposes and are not affected by this service.
/// </summary>
public class TokenCleanupService(IServiceProvider serviceProvider, ILogger<TokenCleanupService> logger) : BackgroundService
{
    private static readonly TimeSpan PurgeInterval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Token Cleanup Service Has Started; Purge Interval Is {Interval}", PurgeInterval);

        while (stoppingToken.IsCancellationRequested is false)
        {
            try
            {
                await PurgeExpiredTokens(stoppingToken);
            }

            catch (Exception exception)
            {
                logger.LogError(exception, "[BUG] Token Cleanup Pass Failed");
            }

            try
            {
                await Task.Delay(PurgeInterval, stoppingToken);
            }

            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    private async Task PurgeExpiredTokens(CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceProvider.CreateScope();

        MerrickContext context = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        // Load All Unconsumed Tokens Because Entity Framework Cannot Translate "TimestampCreated + Validity" To SQL With The Ticks-Based Value Converter For The Validity Property
        List<Token> unconsumedTokens = await context.Tokens
            .Where(token => token.TimestampConsumed == null)
            .ToListAsync(cancellationToken);

        DateTimeOffset now = DateTimeOffset.UtcNow;

        // Filter By "TimestampCreated + Validity" Which Can Now Be Performed In Memory Following The Conversion Of The Validity Property From Ticks Back To TimeSpan
        List<Token> expiredTokens = unconsumedTokens
            .Where(token => token.TimestampCreated + token.Validity < now)
            .ToList();

        if (expiredTokens.Count is 0)
            return;

        context.Tokens.RemoveRange(expiredTokens);

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Purged {Count} Expired Token(s)", expiredTokens.Count);
    }
}
