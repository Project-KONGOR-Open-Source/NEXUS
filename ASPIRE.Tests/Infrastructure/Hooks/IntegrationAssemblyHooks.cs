namespace ASPIRE.Tests.Infrastructure.Hooks;

/// <summary>
///     Provides shared assembly-level hooks for integration tests.
///     Call <see cref="EnsureContainerImagesArePulled"/> from each downstream test project's <c>[Before(HookType.Assembly)]</c> method so that Docker images are pulled up front and test timings remain accurate.
///     This includes SQL Server, Redis, and any other containerised dependencies required by the test suite.
/// </summary>
public static class IntegrationAssemblyHooks
{
    /// <summary>
    ///     Pre-pulls every container image used by the integration test suite, and seeds the placeholder connection-string configuration that Aspire's service-specific integrations validate eagerly during host build.
    ///     <c>ConnectionStrings:MERRICK</c> satisfies the SQL Server integration (<c>AddSqlServerDbContext</c>), and <c>ConnectionStrings:DISTRIBUTED-CACHE</c> satisfies the Redis integration (<c>AddStackExchangeRedisDistributedCaching</c>).
    ///     The real per-test connection strings are wired up inside <see cref="ServiceIntegrationWebApplicationFactory{TSelf, TAssemblyMarker}.ConfigureWebHost"/> which removes and re-registers the relevant services before they are resolved.
    /// </summary>
    public static async Task EnsureContainerImagesArePulled()
    {
        // Non-Empty Values Are All That Aspire Needs To Pass Its Eager Connection-String Validation Inside The Program's Main
        // The Per-Test Container Connection Strings Replace These Before The SQL Server And Cache Services Are Resolved
        Environment.SetEnvironmentVariable("ConnectionStrings__MERRICK", "Server=localhost;Database=placeholder;TrustServerCertificate=True");
        Environment.SetEnvironmentVariable("ConnectionStrings__DISTRIBUTED-CACHE", "localhost:6379,abortConnect=false");

        string[] imagesToPull =
        [
            SQLServerContainer.Image,
            RedisContainer.Image,
            WireMockContainer.Image
        ];

        await DockerImageManager.EnsureImagesArePulled(imagesToPull);
    }
}
