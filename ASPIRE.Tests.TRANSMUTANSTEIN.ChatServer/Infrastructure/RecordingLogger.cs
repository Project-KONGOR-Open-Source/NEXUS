namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;

/// <summary>
///     Records the level and formatted message of every entry logged through it, so that a test can construct a service with it and assert on what the service logs.
/// </summary>
internal sealed class RecordingLogger<TCategory> : ILogger<TCategory>
{
    public ConcurrentQueue<(LogLevel Level, string Message)> Entries { get; } = new ();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        => Entries.Enqueue((logLevel, formatter(state, exception)));
}
