namespace TRANSMUTANSTEIN.ChatServer.Services;

public class MatchmakingService(IServiceProvider serviceProvider) : IHostedService, IDisposable
{
    private IServiceProvider ServiceProvider { get; set; } = serviceProvider;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // TODO: Do Something On Start

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // TODO: Do Something On Stop

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        // TODO: Do Something On Dispose

        return;
    }
}
