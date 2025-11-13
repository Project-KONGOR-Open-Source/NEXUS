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

        // Replace Database Context With In-Memory Database
        WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory = new WebApplicationFactory<ZORGATHAssemblyMarker>().WithWebHostBuilder(builder => builder.ConfigureServices(services =>
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
