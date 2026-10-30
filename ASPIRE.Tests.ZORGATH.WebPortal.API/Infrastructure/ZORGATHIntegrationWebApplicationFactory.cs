using System.Security.Cryptography;

namespace ASPIRE.Tests.ZORGATH.WebPortal.API.Infrastructure;

/// <summary>
///     Integration test <see cref="WebApplicationFactory{TEntryPoint}"/> for the ZORGATH web portal API.
///     Swaps the production <see cref="IEmailService"/> for <see cref="InMemoryEmailService"/> so tests can assert on what the controllers dispatched without a live SMTP round-trip.
/// </summary>
public sealed class ZORGATHIntegrationWebApplicationFactory(ServiceContainerContext containerContext)
    : ServiceIntegrationWebApplicationFactory<ZORGATHIntegrationWebApplicationFactory, ZORGATHAssemblyMarker>(containerContext)
{
    // No Settings File Holds The JWT Signing Key, Which The Application Host Generates In Production, So Each Factory Generates Its Own
    private readonly string jwtSigningKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    protected override void ConfigureEnvironment(IWebHostBuilder builder)
    {
        // Production Reads These Via Environment.GetEnvironmentVariable Rather Than IConfiguration, So builder.UseSetting Would Not Reach Them.
        Environment.SetEnvironmentVariable("INFRASTRUCTURE_GATEWAY", "localhost");

        builder.UseSetting("Operational:JWT:SigningKey", jwtSigningKey);
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
