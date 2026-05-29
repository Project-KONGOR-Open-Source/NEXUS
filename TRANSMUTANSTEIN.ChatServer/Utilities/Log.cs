namespace TRANSMUTANSTEIN.ChatServer.Utilities;

public class Log
{
    private static Serilog.ILogger? MaybeLogger { get; set; }

    public static Serilog.ILogger Initialise(Serilog.ILogger logger)
    {
        MaybeLogger = logger;

        return MaybeLogger;
    }

    private static Serilog.ILogger Get()
        => MaybeLogger ?? throw new InvalidOperationException("Logger Has Not Been Initialised");

    // Creates A Derived Logger That Attaches The Given Structured Property To Every Event Written Through It
    public static Serilog.ILogger ForContext(string propertyName, object? value, bool destructureObjects = false)
        => Get().ForContext(propertyName, value, destructureObjects);

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
