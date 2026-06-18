namespace TRANSMUTANSTEIN.ChatServer.Services;

public class ChatServerHealthCheck(IHostApplicationLifetime applicationLifetime) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        // The Chat Server's Protocol And HTTP Endpoints Are Bound By Kestrel During Host Startup, So A Started Application Is One Whose Endpoints Are Listening
        return applicationLifetime.ApplicationStarted.IsCancellationRequested
            ? Task.FromResult(HealthCheckResult.Healthy("[HEALTHY] Chat Server Is Running And Accepting Connections"))
            : Task.FromResult(HealthCheckResult.Unhealthy("[UNHEALTHY] Chat Server Has Not Started"));
    }
}
