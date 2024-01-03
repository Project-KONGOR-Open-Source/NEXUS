namespace MERRICK.Database;

internal class MERRICK
{
    internal static async Task Main(string[] args)
    {
        // Create The Application Builder
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add Aspire Service Defaults
        builder.AddServiceDefaults();

        // Add The Database Context
        builder.AddSqlServerDbContext<MerrickContext>("MERRICK", configureSettings: null, configureDbContextOptions: options =>
        {
            options.EnableDetailedErrors(builder.Environment.IsDevelopment());
            options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
            options.EnableThreadSafetyChecks();
        });

        // Add Open Telemetry
        builder.Services.AddOpenTelemetry().WithTracing(tracing => tracing.AddSource(DatabaseInitializer.ActivitySourceName));

        // Add Singleton Service For Initializing The Database
        builder.Services.AddSingleton<DatabaseInitializer>();

        // Set Database Initializer Service To Run In The Background
        builder.Services.AddHostedService(provider => provider.GetRequiredService<DatabaseInitializer>());

        // Add Database Health Check
        builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("MERRICK Database Health Check");

        // Build The Application
        WebApplication application = builder.Build();

        // Map Aspire Default Endpoints
        application.MapDefaultEndpoints();

        // Run The Application
        await application.RunAsync();
    }
}
