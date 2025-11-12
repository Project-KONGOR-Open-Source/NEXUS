namespace ASPIRE.Tests.ZORGATH.WebPortal.API.Infrastructure;

/// <summary>
///     Provides Test Dependencies For ZORGATH Web Portal API Tests
/// </summary>
public static class ZORGATHServiceProvider
{
    /// <summary>
    ///     Creates An Orchestrated Instance Of The ZORGATH Web Portal API
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
