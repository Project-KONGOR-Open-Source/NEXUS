using Microsoft.Extensions.Logging;

namespace TRANSMUTANSTEIN.ChatServer.Logging;

public static partial class Log
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Flood Prevention Service Starting With Threshold: {Threshold}, Decay Interval: {DecayInterval}s")]
    public static partial void LogServiceStarting(this ILogger logger, int threshold, int decayInterval);

    [LoggerMessage(Level = LogLevel.Information, Message = "Flood Prevention Service Stopping")]
    public static partial void LogServiceStopping(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Account {AccountID} Exempt From Flood Prevention (Staff Account)")]
    public static partial void LogAccountExempt_Debug(this ILogger logger, int accountID);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Account {AccountID} Exceeded Flood Threshold ({Threshold}), Request Count: {RequestCount}")]
    public static partial void LogFloodThresholdExceeded(this ILogger logger, int accountID, int threshold, int requestCount);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Flood Prevention Decay: {DecayedCount} Accounts Decayed, {RemovedCount} Accounts Cleaned Up")]
    public static partial void LogDecayCycle_Debug(this ILogger logger, int decayedCount, int removedCount);

    [LoggerMessage(Level = LogLevel.Error, Message = "Chat Server Is NULL During Stop")]
    public static partial void LogChatServerNullDuringStop(this ILogger logger);
}
