namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests;

/// <summary>
///     Smoke tests verifying that the TRANSMUTANSTEIN host builds end-to-end against real SQL Server and Redis containers, migrations run successfully against the per-test database, and the distributed cache is reachable.
///     The production hosted services (<see cref="ChatService"/>, <see cref="MatchmakingService"/>, <see cref="FloodPreventionService"/>) bind raw TCP sockets, so <see cref="TRANSMUTANSTEINIntegrationWebApplicationFactory.ConfigureAdditionalServices"/> removes them before the test host starts; these smoke tests therefore exercise only the infrastructure wiring, not the hosted services themselves.
/// </summary>
public sealed class HostedServicesSmokeTests(TRANSMUTANSTEINIntegrationWebApplicationFactory webApplicationFactory)
{
    [Before(HookType.Test)]
    public Task Before_Each_Test()
        => webApplicationFactory.WithSQLServerContainer().WithRedisContainer().InitialiseAsync();

    [Test]
    public async Task DatabaseContext_ResolvesAndIsMigrated()
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        bool canConnect = await databaseContext.Database.CanConnectAsync();
        IEnumerable<string> appliedMigrations = await databaseContext.Database.GetAppliedMigrationsAsync();

        using (Assert.Multiple())
        {
            await Assert.That(canConnect).IsTrue();
            await Assert.That(appliedMigrations).IsNotEmpty();
        }
    }

    [Test]
    public async Task RedisMultiplexer_Resolves()
    {
        IConnectionMultiplexer connectionMultiplexer = webApplicationFactory.Services.GetRequiredService<IConnectionMultiplexer>();

        await Assert.That(connectionMultiplexer.IsConnected).IsTrue();
    }
}
