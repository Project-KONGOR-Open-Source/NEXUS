namespace ASPIRE.Tests.ZORGATH.WebPortal.API.Infrastructure;

/// <summary>
///     Provides Test Dependencies For ZORGATH Web Portal API Tests
/// </summary>
public static class ZORGATHServiceProvider
{
    /// <summary>
    ///     Creates An Instance Of The ZORGATH Web Portal API With In-Memory Dependencies
    /// </summary>
    public static WebApplicationFactory<ZORGATHAssemblyMarker> CreateOrchestratedInstance(string? identifier = null)
    {
        string databaseName = identifier ?? Guid.CreateVersion7().ToString();

        // Set Required Environment Variables
        Environment.SetEnvironmentVariable("INFRASTRUCTURE_GATEWAY", "localhost");

        // Replace Database Context And Distributed Cache With In-Memory Implementations
        WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory = new WebApplicationFactory<ZORGATHAssemblyMarker>().WithWebHostBuilder(builder =>
            builder.UseSetting("INFRASTRUCTURE_GATEWAY", "localhost")
                   .ConfigureServices(services =>
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

            // Register In-Process Distributed Cache Database
            services.AddSingleton<IDatabase, InProcess.InProcessDistributedCacheStore>();
        }));

        // Ensure That OnModelCreating From MerrickContext Has Been Called
        webApplicationFactory.Services.GetRequiredService<MerrickContext>().Database.EnsureCreated();

        return webApplicationFactory;
    }
}
