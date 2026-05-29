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
        => MaybeLogger ?? throw new InvalidOperationException("Logger Has Not Been Initialised");

    public static ILogger ForContext(string propertyName, object? value, bool destructureObjects = false)
        => Get().ForContext(propertyName, value, destructureObjects);

    public static ILogger ForContext<TSource>()
        => Get().ForContext<TSource>();

    public static ILogger ForContext(Type source)
        => Get().ForContext(source);

    public static void Trace(Exception? exception, string message, params object?[] arguments)
        => Get().Verbose(exception, message, arguments);

    public static void Trace(string message, params object?[] arguments)
        => Get().Verbose(message, arguments);

    public static void Debug(Exception? exception, string message, params object?[] arguments)
        => Get().Debug(exception, message, arguments);

    public static void Debug(string message, params object?[] arguments)
        => Get().Debug(message, arguments);

    public static void Information(Exception? exception, string message, params object?[] arguments)
        => Get().Information(exception, message, arguments);

    public static void Information(string message, params object?[] arguments)
        => Get().Information(message, arguments);

    public static void Warning(Exception? exception, string message, params object?[] arguments)
        => Get().Warning(exception, message, arguments);

    public static void Warning(string message, params object?[] arguments)
        => Get().Warning(message, arguments);

    public static void Error(Exception? exception, string message, params object?[] arguments)
        => Get().Error(exception, message, arguments);

    public static void Error(string message, params object?[] arguments)
        => Get().Error(message, arguments);

    public static void Critical(Exception? exception, string message, params object?[] arguments)
        => Get().Fatal(exception, message, arguments);

    public static void Critical(string message, params object?[] arguments)
        => Get().Fatal(message, arguments);
}
