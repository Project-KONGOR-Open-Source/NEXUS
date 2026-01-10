namespace TRANSMUTANSTEIN.ChatServer.Services;

public class ChatServerHealthCheck(ChatService chatService) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (chatService.ChatServer is null)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Chat Server Is Not Running"));
        }

        return chatService.ChatServer.IsStarted switch
        {
            true when chatService.ChatServer.IsAccepting => Task.FromResult(
                HealthCheckResult.Healthy("[HEALTHY] Chat Server Is Running And Accepting Connections")),
            true when chatService.ChatServer.IsAccepting is false => Task.FromResult(
                HealthCheckResult.Degraded("[DEGRADED] Chat Server Is Running But Is Not Accepting Connections")),
            _ => Task.FromResult(HealthCheckResult.Unhealthy("[UNHEALTHY] Unknown Chat Server Status"))
        };
    }
}