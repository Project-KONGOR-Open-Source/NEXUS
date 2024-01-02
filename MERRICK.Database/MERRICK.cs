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

        // TODO: Document The Services Below

        builder.Services.AddOpenTelemetry().WithTracing(tracing => tracing.AddSource(DatabaseInitializer.ActivitySourceName));
        builder.Services.AddSingleton<DatabaseInitializer>();
        builder.Services.AddHostedService(provider => provider.GetRequiredService<DatabaseInitializer>());
        builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("MERRICK Database Health Check", null);

        // Build The Application
        WebApplication application = builder.Build();

        // Map Aspire Default Endpoints
        application.MapDefaultEndpoints();

        // Run The Application
        await application.RunAsync();
    }
}
