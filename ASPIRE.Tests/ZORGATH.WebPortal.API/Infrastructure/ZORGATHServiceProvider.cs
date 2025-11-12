namespace ASPIRE.Tests.ZORGATH.WebPortal.API.Infrastructure;

/// <summary>
///     Provides Test Dependencies For ZORGATH Web Portal Tests
/// </summary>
public static class ZORGATHServiceProvider
{
    /// <summary>
    ///     Creates An Instance Of The ZORGATH Web Portal
    /// </summary>
    public static WebApplicationFactory<ZORGATHAssemblyMarker> CreateInstance(string? identifier = null)
    {
        WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory = new WebApplicationFactory<ZORGATHAssemblyMarker>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ASPIRE:Orchestrated", "false");

            builder.ConfigureServices(services =>
            {
                List<ServiceDescriptor> databaseContextDescriptors = [.. services.Where(descriptor => descriptor.ServiceType == typeof(DbContextOptions<MerrickContext>))];

                if (databaseContextDescriptors.Any())
                    foreach (ServiceDescriptor databaseContextDescriptor in databaseContextDescriptors)
                        services.Remove(databaseContextDescriptor);

                services.AddDbContext<MerrickContext>(options => options.UseInMemoryDatabase(identifier ?? Guid.CreateVersion7().ToString()));

                List<ServiceDescriptor> distributedCacheDescriptors = [.. services.Where(descriptor => descriptor.ServiceType == typeof(IDistributedCache))];

                if (distributedCacheDescriptors.Any())
                    foreach (ServiceDescriptor distributedCacheDescriptor in distributedCacheDescriptors)
                        services.Remove(distributedCacheDescriptor);

                services.AddDistributedMemoryCache();
            });
        });

        return webApplicationFactory;
    }

    /// <summary>
    ///     Creates An Orchestrated Instance Of The ZORGATH Web Portal
    /// </summary>
    public static async Task<WebApplicationFactory<ZORGATHAssemblyMarker>> CreateOrchestratedInstance(string? identifier = null)
    {
        IDistributedApplicationTestingBuilder applicationTestingBuilder = await DistributedApplicationTestingBuilder.CreateAsync<AppHost.ASPIRE>();

        applicationTestingBuilder.Services.ConfigureHttpClientDefaults(httpClientBuilder =>
        {
            httpClientBuilder.AddStandardResilienceHandler();
        });

        DistributedApplication distributedApplication = await applicationTestingBuilder.BuildAsync();

        await distributedApplication.StartAsync();

        WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory = new WebApplicationFactory<ZORGATHAssemblyMarker>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ASPIRE:Orchestrated", "true");

            builder.ConfigureServices(services =>
            {
                List<ServiceDescriptor> databaseContextDescriptors = [.. services.Where(descriptor => descriptor.ServiceType == typeof(DbContextOptions<MerrickContext>))];

                if (databaseContextDescriptors.Any())
                    foreach (ServiceDescriptor databaseContextDescriptor in databaseContextDescriptors)
                        services.Remove(databaseContextDescriptor);

                services.AddDbContext<MerrickContext>(options => options.UseInMemoryDatabase(identifier ?? Guid.CreateVersion7().ToString()));

                List<ServiceDescriptor> distributedCacheDescriptors = [.. services.Where(descriptor => descriptor.ServiceType == typeof(IDistributedCache))];

                if (distributedCacheDescriptors.Any())
                    foreach (ServiceDescriptor distributedCacheDescriptor in distributedCacheDescriptors)
                        services.Remove(distributedCacheDescriptor);

                services.AddDistributedMemoryCache();
            });
        });

        return webApplicationFactory;
    }
}
