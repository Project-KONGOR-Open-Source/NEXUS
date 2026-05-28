namespace ASPIRE.Tests.KONGOR.MasterServer.Infrastructure;

/// <summary>
///     Integration test <see cref="WebApplicationFactory{TEntryPoint}"/> for the KONGOR master server.
///     Sets the chat server and infrastructure-gateway settings that the host requires at start-up and registers a loopback remote IP filter so request-path code that reads <see cref="HttpContext.Connection"/> sees a real address rather than <see langword="null"/>.
/// </summary>
public sealed class KONGORIntegrationWebApplicationFactory(ServiceContainerContext containerContext)
    : ServiceIntegrationWebApplicationFactory<KONGORIntegrationWebApplicationFactory, KONGORAssemblyMarker>(containerContext)
{
    protected override void ConfigureEnvironment(IWebHostBuilder builder)
    {
        // Production Reads These Via Environment.GetEnvironmentVariable Rather Than IConfiguration, So builder.UseSetting Would Not Reach Them.
        Environment.SetEnvironmentVariable("CHAT_SERVER_HOST", "localhost");
        Environment.SetEnvironmentVariable("CHAT_SERVER_PORT_CLIENT", "11031");
        Environment.SetEnvironmentVariable("CHAT_SERVER_PORT_MATCH_SERVER", "11032");
        Environment.SetEnvironmentVariable("CHAT_SERVER_PORT_MATCH_SERVER_MANAGER", "11033");
        Environment.SetEnvironmentVariable("APPLICATION_URL", "http://0.0.0.0/");
        Environment.SetEnvironmentVariable("INFRASTRUCTURE_GATEWAY", "localhost");
    }

    protected override void ConfigureAdditionalServices(IServiceCollection services)
    {
        services.AddSingleton<IStartupFilter, LoopbackRemoteIPAddressStartupFilter>();

        if (UseWireMockContainer)
        {
            // Redirect The Chat Server Status Client To The Scoped WireMock URL So Tests Can Mock Its Responses Without Touching Production Configuration
            services.PostConfigure<ChatServerStatusSettings>(settings => settings.BaseURL = WireMockURL);
        }
    }
}
