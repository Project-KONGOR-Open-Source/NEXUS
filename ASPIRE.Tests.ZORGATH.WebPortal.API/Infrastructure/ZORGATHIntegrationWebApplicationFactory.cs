namespace ASPIRE.Tests.ZORGATH.WebPortal.API.Infrastructure;

/// <summary>
///     Integration test <see cref="WebApplicationFactory{TEntryPoint}"/> for the ZORGATH web portal API.
///     Swaps the production <see cref="IEmailService"/> for <see cref="InMemoryEmailService"/> so tests can assert on what the controllers dispatched without a live SMTP round-trip.
/// </summary>
public sealed class ZORGATHIntegrationWebApplicationFactory(ServiceContainerContext containerContext)
    : ServiceIntegrationWebApplicationFactory<ZORGATHIntegrationWebApplicationFactory, ZORGATHAssemblyMarker>(containerContext)
{
    protected override void ConfigureEnvironment(IWebHostBuilder builder)
    {
        // Production Reads These Via Environment.GetEnvironmentVariable Rather Than IConfiguration, So builder.UseSetting Would Not Reach Them.
        Environment.SetEnvironmentVariable("INFRASTRUCTURE_GATEWAY", "localhost");
    }

    protected override void ConfigureAdditionalServices(IServiceCollection services)
    {
        services.RemoveAll<IEmailService>();
        services.AddSingleton<InMemoryEmailService>();
        services.AddSingleton<IEmailService>(serviceProvider => serviceProvider.GetRequiredService<InMemoryEmailService>());
    }

    /// <summary>
    ///     Returns the <see cref="InMemoryEmailService"/> instance that intercepted all email dispatches from the host under test, so assertions can read the captured entries.
    /// </summary>
    public InMemoryEmailService GetInMemoryEmailService()
        => Services.GetRequiredService<InMemoryEmailService>();
}
