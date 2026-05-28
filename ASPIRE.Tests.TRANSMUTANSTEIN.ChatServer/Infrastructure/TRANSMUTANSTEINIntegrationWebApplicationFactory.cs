namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;

/// <summary>
///     Integration test <see cref="WebApplicationFactory{TEntryPoint}"/> for the TRANSMUTANSTEIN chat server.
///     The chat server is a background-worker host with no HTTP surface beyond <c>/health</c>; tests exercise behaviour by resolving the registered <see cref="IHostedService"/> instances directly rather than driving traffic through an <see cref="HttpClient"/>.
/// </summary>
public sealed class TRANSMUTANSTEINIntegrationWebApplicationFactory(ServiceContainerContext containerContext)
    : ServiceIntegrationWebApplicationFactory<TRANSMUTANSTEINIntegrationWebApplicationFactory, TRANSMUTANSTEINAssemblyMarker>(containerContext)
{
    protected override void ConfigureEnvironment(IWebHostBuilder builder)
    {
        // Production Reads These Via Environment.GetEnvironmentVariable Rather Than IConfiguration, So builder.UseSetting Would Not Reach Them
        Environment.SetEnvironmentVariable("INFRASTRUCTURE_GATEWAY", "localhost");
    }

    protected override void ConfigureAdditionalServices(IServiceCollection services)
    {
        // The Production Hosted Services Bind Raw TCP Sockets For Chat And Matchmaking Traffic; Starting Them Inside The Test Host Would Create Port Conflicts Between Parallel Tests And Fail If The Chat Server Port Environment Variables Are Not Set To Real, Unused Ports
        // The Smoke Tests Exercise The Infrastructure Wiring (Database, Redis) Rather Than The Sockets, So We Strip The Hosted Services Here
        ServiceDescriptor[] hostedServices = services.Where(descriptor => descriptor.ServiceType == typeof(IHostedService)).ToArray();

        foreach (ServiceDescriptor descriptor in hostedServices)
        {
            services.Remove(descriptor);
        }

        services.RemoveAll<FloodPreventionService>();
    }
}
