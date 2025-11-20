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

        // Replace Database Context And Distributed Cache With In-Memory Implementations
        WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory = new WebApplicationFactory<KONGORAssemblyMarker>().WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            Func<ServiceDescriptor, bool> databaseContextPredicate = descriptor =>
                descriptor.ServiceType.FullName?.Contains(nameof(MerrickContext)) is true || descriptor.ImplementationType?.FullName?.Contains(nameof(MerrickContext)) is true;

            // Remove MerrickContext Registration
            foreach (ServiceDescriptor? descriptor in services.Where(databaseContextPredicate).ToList())
                services.Remove(descriptor);

            // Register In-Memory MerrickContext
            services.AddDbContext<MerrickContext>(options => options.UseInMemoryDatabase(databaseName).EnableServiceProviderCaching(false),
                ServiceLifetime.Singleton, ServiceLifetime.Singleton);

            Func<ServiceDescriptor, bool> distributedCachePredicate = descriptor =>
                descriptor.ServiceType == typeof(IConnectionMultiplexer) || descriptor.ServiceType == typeof(IDatabase);

            // Remove IConnectionMultiplexer And IDatabase Registrations
            foreach (ServiceDescriptor? descriptor in services.Where(distributedCachePredicate).ToList())
                services.Remove(descriptor);

            // Register In-Memory Test Doubles For Distributed Cache
            services.AddSingleton<IDatabase, InProcess.InProcessDistributedCacheStore>();
            services.AddSingleton<IConnectionMultiplexer>(serviceProvider => new InProcess.InProcessConnectionMultiplexer(serviceProvider.GetRequiredService<IDatabase>()));
        }));

        // Ensure That OnModelCreating From MerrickContext Has Been Called
        webApplicationFactory.Services.GetRequiredService<MerrickContext>().Database.EnsureCreated();

        return webApplicationFactory;
    }
}
