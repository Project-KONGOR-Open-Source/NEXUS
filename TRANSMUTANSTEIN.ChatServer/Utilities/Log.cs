namespace TRANSMUTANSTEIN.ChatServer.Utilities;

public class Log
{
    private static ILogger? MaybeLogger { get; set; }

    public static ILogger Initialise(ILogger logger)
    {
        MaybeLogger = logger;

        return MaybeLogger;
    }

    private static ILogger Get()
        => MaybeLogger is null ? throw new InvalidOperationException("Logger Has Not Been Initialised") : MaybeLogger;

    public static void Debug(Exception? exception, string? message, params object?[] args)
    {
        ILogger logger = Get();

        if (logger.IsEnabled(LogLevel.Debug))
            logger.Log(LogLevel.Debug, exception, message, args);
    }

    public static void Debug(string? message, params object?[] args)
    {
        ILogger logger = Get();

        if (logger.IsEnabled(LogLevel.Debug))
            logger.Log(LogLevel.Debug, message, args);
    }

    public static void Trace(Exception? exception, string? message, params object?[] args)
    {
        ILogger logger = Get();

        if (logger.IsEnabled(LogLevel.Trace))
            logger.Log(LogLevel.Trace, exception, message, args);
    }

    public static void Trace(string? message, params object?[] args)
    {
        ILogger logger = Get();

        if (logger.IsEnabled(LogLevel.Trace))
            logger.Log(LogLevel.Trace, message, args);
    }

    public static void Information(Exception? exception, string? message, params object?[] args)
    {
        ILogger logger = Get();

        if (logger.IsEnabled(LogLevel.Information))
            logger.Log(LogLevel.Information, exception, message, args);
    }

    public static void Information(string? message, params object?[] args)
    {
        ILogger logger = Get();

        if (logger.IsEnabled(LogLevel.Information))
            logger.Log(LogLevel.Information, message, args);
    }

    public static void Warning(Exception? exception, string? message, params object?[] args)
    {
        ILogger logger = Get();

        if (logger.IsEnabled(LogLevel.Warning))
            logger.Log(LogLevel.Warning, exception, message, args);
    }

    public static void Warning(string? message, params object?[] args)
    {
        ILogger logger = Get();

        if (logger.IsEnabled(LogLevel.Warning))
            logger.Log(LogLevel.Warning, message, args);
    }

    public static void Error(Exception? exception, string? message, params object?[] args)
    {
        ILogger logger = Get();

        if (logger.IsEnabled(LogLevel.Error))
            logger.Log(LogLevel.Error, exception, message, args);
    }

    public static void Error(string? message, params object?[] args)
    {
        ILogger logger = Get();

        if (logger.IsEnabled(LogLevel.Error))
            logger.Log(LogLevel.Error, message, args);
    }

    public static void Critical(Exception? exception, string? message, params object?[] args)
    {
        ILogger logger = Get();

        if (logger.IsEnabled(LogLevel.Critical))
            logger.Log(LogLevel.Critical, exception, message, args);
    }

    public static void Critical(string? message, params object?[] args)
    {
        ILogger logger = Get();

        if (logger.IsEnabled(LogLevel.Critical))
            logger.Log(LogLevel.Critical, message, args);
    }
}
