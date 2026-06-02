namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.ServerManagement;

/// <summary>
///     Tests for the single-holder hosting lease that restricts a host account to a single concurrent host.
/// </summary>
public sealed class HostLeaseTests(KONGORIntegrationWebApplicationFactory webApplicationFactory)
{
    [Before(HookType.Test)]
    public Task Before_Each_Test()
        => webApplicationFactory.WithSQLServerContainer().WithRedisContainer().InitialiseAsync();

    [Test]
    public async Task Claiming_An_Unheld_Host_Lease_Succeeds_And_Marks_It_Held()
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        IDatabase distributedCacheStore = scope.ServiceProvider.GetRequiredService<IDatabase>();

        const string hostAccountName = "HOST-CLAIM";

        bool claimed = await distributedCacheStore.TryClaimHostLease(hostAccountName);

        using (Assert.Multiple())
        {
            await Assert.That(claimed).IsTrue();
            await Assert.That(await distributedCacheStore.IsHostLeaseHeld(hostAccountName)).IsTrue();
        }
    }

    [Test]
    public async Task Claiming_An_Already_Held_Host_Lease_Fails()
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        IDatabase distributedCacheStore = scope.ServiceProvider.GetRequiredService<IDatabase>();

        const string hostAccountName = "HOST-DUPLICATE";

        bool firstClaim = await distributedCacheStore.TryClaimHostLease(hostAccountName);
        bool secondClaim = await distributedCacheStore.TryClaimHostLease(hostAccountName);

        using (Assert.Multiple())
        {
            await Assert.That(firstClaim).IsTrue();
            await Assert.That(secondClaim).IsFalse();
        }
    }

    [Test]
    public async Task Releasing_A_Host_Lease_Allows_It_To_Be_Claimed_Again()
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        IDatabase distributedCacheStore = scope.ServiceProvider.GetRequiredService<IDatabase>();

        const string hostAccountName = "HOST-RELEASE";

        await distributedCacheStore.TryClaimHostLease(hostAccountName);
        await distributedCacheStore.ReleaseHostLease(hostAccountName);

        bool isHeldAfterRelease = await distributedCacheStore.IsHostLeaseHeld(hostAccountName);
        bool reclaimed = await distributedCacheStore.TryClaimHostLease(hostAccountName);

        using (Assert.Multiple())
        {
            await Assert.That(isHeldAfterRelease).IsFalse();
            await Assert.That(reclaimed).IsTrue();
        }
    }

    [Test]
    public async Task Claiming_A_Host_Lease_Sets_A_Time_To_Live_So_It_Self_Heals()
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        IDatabase distributedCacheStore = scope.ServiceProvider.GetRequiredService<IDatabase>();

        const string hostAccountName = "HOST-TIME-TO-LIVE";

        await distributedCacheStore.TryClaimHostLease(hostAccountName);

        TimeSpan? timeToLive = await distributedCacheStore.KeyTimeToLiveAsync($"MATCH-HOST-LEASE:{hostAccountName}");

        using (Assert.Multiple())
        {
            await Assert.That(timeToLive).IsNotNull();
            await Assert.That(timeToLive.GetValueOrDefault()).IsGreaterThan(TimeSpan.Zero);
        }
    }

    [Test]
    public async Task Renewing_An_Unheld_Host_Lease_Does_Not_Create_It()
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        IDatabase distributedCacheStore = scope.ServiceProvider.GetRequiredService<IDatabase>();

        const string hostAccountName = "HOST-RENEW-UNHELD";

        await distributedCacheStore.RenewHostLease(hostAccountName);

        await Assert.That(await distributedCacheStore.IsHostLeaseHeld(hostAccountName)).IsFalse();
    }
}
