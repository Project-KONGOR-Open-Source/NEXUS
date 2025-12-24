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

        // Set Required Environment Variables
        Environment.SetEnvironmentVariable("CHAT_SERVER_HOST", "localhost");
        Environment.SetEnvironmentVariable("CHAT_SERVER_PORT_CLIENT", "11031");
        Environment.SetEnvironmentVariable("CHAT_SERVER_PORT_MATCH_SERVER", "11032");
        Environment.SetEnvironmentVariable("CHAT_SERVER_PORT_MATCH_SERVER_MANAGER", "11033");
        Environment.SetEnvironmentVariable("APPLICATION_URL", "http://0.0.0.0/");

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

            // Register In-Process Distributed Cache Database
            services.AddSingleton<IDatabase, InProcess.InProcessDistributedCacheStore>();

            // Add Middleware To Set Fake Remote IP Address
            services.AddSingleton<IStartupFilter>(new RemoteIPAddressStartupFilter());
        }));

        // Ensure That OnModelCreating From MerrickContext Has Been Called
        webApplicationFactory.Services.GetRequiredService<MerrickContext>().Database.EnsureCreated();

        return webApplicationFactory;
    }
}

/// <summary>
///     Startup Filter To Set Fake Remote IP Address
/// </summary>
file class RemoteIPAddressStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.Use(next => async context =>
            {
                context.Connection.RemoteIpAddress = System.Net.IPAddress.Loopback;

                await next(context);
            });

            next(app);
        };
    }
}
