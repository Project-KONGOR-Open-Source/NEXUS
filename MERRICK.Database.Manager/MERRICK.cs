namespace MERRICK.Database.Manager;

internal class MERRICK
{
    internal static async Task Main(string[] args)
    {
        // Create The Application Builder
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add Aspire Service Defaults
        builder.AddServiceDefaults();

        // Get The Database Connection String
        string connectionString = builder.Configuration.GetConnectionString("MERRICK") ?? throw new NullReferenceException("MERRICK Connection String Is NULL");

        // Add The Database Context
        builder.Services.AddDbContext<MerrickContext>(options =>
        {
            // Set The Database Connection Options
            options.UseSqlServer(connectionString, connection => connection.MigrationsAssembly(typeof(MERRICK).Assembly.GetName().Name));

            // Enable Comprehensive Database Query Logging
            options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
        });

        // TODO: Document The Services Below

        builder.Services.AddOpenTelemetry().WithTracing(tracing => tracing.AddSource(DatabaseInitializer.ActivitySourceName));
        builder.Services.AddSingleton<DatabaseInitializer>();
        builder.Services.AddHostedService(provider => provider.GetRequiredService<DatabaseInitializer>());
        builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("DatabaseHealthCheck", null);

        // Build The Application
        WebApplication application = builder.Build();

        // Map Aspire Default Endpoints
        application.MapDefaultEndpoints();

        // Run The Application
        await application.RunAsync();
    }
}
