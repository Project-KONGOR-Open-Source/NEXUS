using TRANSMUTANSTEIN.ChatServer.Domain.Core;
using TRANSMUTANSTEIN.ChatServer.Services;

namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;

public class NullFloodPreventionService(ILogger<FloodPreventionService> logger) : FloodPreventionService(logger)
{
    public override bool CheckAndHandleFloodPrevention(ChatSession session)
    {
        return true;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}