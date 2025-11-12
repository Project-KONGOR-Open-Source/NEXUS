namespace ASPIRE.Tests.KONGOR.MasterServer.Infrastructure;

/// <summary>
///     Provides Test Dependencies For KONGOR Master Server Tests
/// </summary>
public static class KONGORServiceProvider
{
    /// <summary>
    ///     Creates An Instance Of The KONGOR Master Server
    /// </summary>
    public static WebApplicationFactory<KONGORAssemblyMarker> CreateInstance(string? identifier = null)
    {
        WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory = new WebApplicationFactory<KONGORAssemblyMarker>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                List<ServiceDescriptor> databaseContextDescriptors = [.. services.Where(descriptor => descriptor.ServiceType == typeof(DbContextOptions<MerrickContext>))];

                foreach (ServiceDescriptor databaseContextDescriptor in databaseContextDescriptors)
                    services.Remove(databaseContextDescriptor);

                services.AddDbContext<MerrickContext>(options => options.UseInMemoryDatabase(identifier ?? Guid.CreateVersion7().ToString()));

                List<ServiceDescriptor> distributedCacheDescriptors = [.. services.Where(descriptor => descriptor.ServiceType == typeof(IDistributedCache))];

                foreach (ServiceDescriptor distributedCacheDescriptor in distributedCacheDescriptors)
                    services.Remove(distributedCacheDescriptor);

                services.AddDistributedMemoryCache();
            });
        });

        return webApplicationFactory;
    }

    /// <summary>
    ///     Creates An Orchestrated Instance Of The KONGOR Master Server
    /// </summary>
    public static async Task<WebApplicationFactory<KONGORAssemblyMarker>> CreateOrchestratedInstance(string? identifier = null)
    {
        IDistributedApplicationTestingBuilder applicationTestingBuilder = await DistributedApplicationTestingBuilder.CreateAsync<AppHost.ASPIRE>();

        applicationTestingBuilder.Services.ConfigureHttpClientDefaults(httpClientBuilder =>
        {
            httpClientBuilder.AddStandardResilienceHandler();
        });

        DistributedApplication distributedApplication = await applicationTestingBuilder.BuildAsync();

        await distributedApplication.StartAsync();

        WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory = new WebApplicationFactory<KONGORAssemblyMarker>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                List<ServiceDescriptor> databaseContextDescriptors = [.. services.Where(descriptor => descriptor.ServiceType == typeof(DbContextOptions<MerrickContext>))];

                foreach (ServiceDescriptor databaseContextDescriptor in databaseContextDescriptors)
                    services.Remove(databaseContextDescriptor);

                services.AddDbContext<MerrickContext>(options => options.UseInMemoryDatabase(identifier ?? Guid.CreateVersion7().ToString()));

                List<ServiceDescriptor> distributedCacheDescriptors = [.. services.Where(descriptor => descriptor.ServiceType == typeof(IDistributedCache))];

                foreach (ServiceDescriptor distributedCacheDescriptor in distributedCacheDescriptors)
                    services.Remove(distributedCacheDescriptor);

                services.AddDistributedMemoryCache();
            });
        });

        return webApplicationFactory;
    }
}
