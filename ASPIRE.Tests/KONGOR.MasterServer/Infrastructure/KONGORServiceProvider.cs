namespace ASPIRE.Tests.KONGOR.MasterServer.Infrastructure;

/// <summary>
///     Provides Test Dependencies For KONGOR Master Server Tests
/// </summary>
public sealed class KONGORServiceProvider : IAsyncDisposable
{
    private readonly MerrickContext merrickContext;
    private readonly DistributedApplication? distributedApplication;
    private readonly WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory;
    private readonly bool isOrchestrated;

    public KONGORServiceProvider()
    {
        merrickContext = InMemoryHelpers.GetInMemoryMerrickContext(Guid.CreateVersion7().ToString());

        webApplicationFactory = new WebApplicationFactory<KONGORAssemblyMarker>()
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
        distributedApplication = null;
    }

    private KONGORServiceProvider(MerrickContext context, DistributedApplication application, WebApplicationFactory<KONGORAssemblyMarker> factory, HttpClient client)
    {
        merrickContext = context;
        distributedApplication = application;
        webApplicationFactory = factory;
        isOrchestrated = true;
    }

    public MerrickContext DatabaseContext => merrickContext;
    public WebApplicationFactory<KONGORAssemblyMarker> WebApplicationFactory => webApplicationFactory;
    public bool IsOrchestrated => isOrchestrated;
    public HttpClient HttpClient => httpClient ?? throw new InvalidOperationException("HTTP Client Is NULL");

    /// <summary>
    ///     Creates an orchestrated instance using the Aspire service graph without replacing external resources.
    /// </summary>
    public static async Task<KONGORServiceProvider> CreateOrchestratedAsync()
    {
        IDistributedApplicationTestingBuilder appHostBuilder = await DistributedApplicationTestingBuilder.CreateAsync<ASPIRE.AppHost.ASPIRE>();

        appHostBuilder.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        DistributedApplication distributedApplication = await appHostBuilder.BuildAsync();
        await distributedApplication.StartAsync();

        HttpClient httpClient = distributedApplication.CreateHttpClient("master-server");

        MerrickContext context = InMemoryHelpers.GetInMemoryMerrickContext(Guid.CreateVersion7().ToString());

        WebApplicationFactory<KONGORAssemblyMarker> factory = new WebApplicationFactory<KONGORAssemblyMarker>()
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

        return new KONGORServiceProvider(context, distributedApplication, factory, httpClient);
    }

    public async ValueTask DisposeAsync()
    {
        httpClient?.Dispose();
        await merrickContext.DisposeAsync();
        if (distributedApplication is not null)
            await distributedApplication.DisposeAsync();
    }
}
