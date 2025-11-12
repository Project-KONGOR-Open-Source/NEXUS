namespace ASPIRE.Tests.ZORGATH.WebPortal.API.Infrastructure;

/// <summary>
///     Provides Test Dependencies For ZORGATH Web Portal Tests
/// </summary>
public sealed class ZORGATHServiceProvider : IAsyncDisposable
{
    private readonly MerrickContext merrickContext;
    private readonly DistributedApplication? distributedApplication;
    private readonly WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory;
    private readonly bool isOrchestrated;

    public ZORGATHServiceProvider()
    {
        merrickContext = InMemoryHelpers.GetInMemoryMerrickContext(Guid.CreateVersion7().ToString());

        webApplicationFactory = new WebApplicationFactory<ZORGATHAssemblyMarker>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ASPIRE:Orchestrated", "false");
                builder.ConfigureServices(services =>
                {
                    ServiceDescriptor? existing = services.FirstOrDefault(d => d.ServiceType == typeof(DbContextOptions<MerrickContext>));
                    if (existing is not null)
                        services.Remove(existing);

                    services.AddDbContext<MerrickContext>(options => options.UseInMemoryDatabase("Test-" + Guid.CreateVersion7().ToString()));

                    ServiceDescriptor? distributedCacheDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IDistributedCache));
                    if (distributedCacheDescriptor is not null)
                        services.Remove(distributedCacheDescriptor);

                    services.AddDistributedMemoryCache();
                });
            });

        isOrchestrated = false;
    }

    private ZORGATHServiceProvider(MerrickContext context, DistributedApplication application, WebApplicationFactory<ZORGATHAssemblyMarker> factory)
    {
        merrickContext = context;
        distributedApplication = application;
        webApplicationFactory = factory;
        isOrchestrated = true;
    }

    public MerrickContext DatabaseContext => merrickContext;
    public WebApplicationFactory<ZORGATHAssemblyMarker> WebApplicationFactory => webApplicationFactory;
    public bool IsOrchestrated => isOrchestrated;

    /// <summary>
    ///     Creates an orchestrated instance using the Aspire service graph without replacing external resources.
    /// </summary>
    public static async Task<ZORGATHServiceProvider> CreateOrchestratedAsync()
    {
        IDistributedApplicationTestingBuilder appHostBuilder = await DistributedApplicationTestingBuilder.CreateAsync<ASPIRE.AppHost.ASPIRE>();

        appHostBuilder.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        DistributedApplication distributedApplication = await appHostBuilder.BuildAsync();
        await distributedApplication.StartAsync();

        MerrickContext context = InMemoryHelpers.GetInMemoryMerrickContext(Guid.CreateVersion7().ToString());

        WebApplicationFactory<ZORGATHAssemblyMarker> factory = new WebApplicationFactory<ZORGATHAssemblyMarker>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ASPIRE:Orchestrated", "true");
                builder.ConfigureServices(services =>
                {
                    ServiceDescriptor? existing = services.FirstOrDefault(d => d.ServiceType == typeof(DbContextOptions<MerrickContext>));
                    if (existing is not null)
                        services.Remove(existing);

                    services.AddDbContext<MerrickContext>(options => options.UseInMemoryDatabase("Test-" + Guid.CreateVersion7().ToString()));

                    ServiceDescriptor? distributedCacheDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IDistributedCache));
                    if (distributedCacheDescriptor is not null)
                        services.Remove(distributedCacheDescriptor);

                    services.AddDistributedMemoryCache();
                });
            });

        return new ZORGATHServiceProvider(context, distributedApplication, factory);
    }

    public async ValueTask DisposeAsync()
    {
        await merrickContext.DisposeAsync();
        if (distributedApplication is not null)
            await distributedApplication.DisposeAsync();
    }
}
