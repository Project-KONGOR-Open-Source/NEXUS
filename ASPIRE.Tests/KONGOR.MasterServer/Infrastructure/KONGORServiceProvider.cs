namespace ASPIRE.Tests.KONGOR.MasterServer.Infrastructure;

/// <summary>
///     Provides Test Dependencies For KONGOR Master Server Tests
/// </summary>
public static class KONGORServiceProvider
{
    /// <summary>
    ///     Creates An Instance Of The KONGOR Master Server With In-Memory Dependencies
    /// </summary>
    public static WebApplicationFactory<KONGORAssemblyMarker> CreateOrchestratedInstance(string? identifier = null)
    {
        string databaseName = identifier ?? Guid.CreateVersion7().ToString();

        // Replace Database Context With In-Memory Database
        WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory = new WebApplicationFactory<KONGORAssemblyMarker>().WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            Func<ServiceDescriptor, bool> serviceDescriptorPredicate = descriptor =>
                descriptor.ServiceType.FullName?.Contains(nameof(MerrickContext)) is true || descriptor.ImplementationType?.FullName?.Contains(nameof(MerrickContext)) is true;

            foreach (ServiceDescriptor? descriptor in services.Where(serviceDescriptorPredicate).ToList())
                services.Remove(descriptor);

            services.AddDbContext<MerrickContext>(options => options.UseInMemoryDatabase(databaseName).EnableServiceProviderCaching(false),
                ServiceLifetime.Singleton, ServiceLifetime.Singleton);
        }));

        // Ensure That OnModelCreating From MerrickContext Has Been Called
        webApplicationFactory.Services.GetRequiredService<MerrickContext>().Database.EnsureCreated();

        return webApplicationFactory;
    }
}
