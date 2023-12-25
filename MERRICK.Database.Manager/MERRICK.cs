namespace MERRICK.Database.Manager;

internal class MERRICK
{
    internal static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.AddServiceDefaults();

        string connectionString = builder.Configuration.GetConnectionString("MERRICK") ?? throw new NullReferenceException("MERRICK Connection String Is NULL");

        builder.Services.AddDbContext<MerrickContext>(options =>
        {
            options.UseSqlServer(connectionString, connection => connection.MigrationsAssembly(typeof(MERRICK).Assembly.GetName().Name));
        });

        builder.Services.AddOpenTelemetry().WithTracing(tracing => tracing.AddSource(DatabaseInitializer.ActivitySourceName));
        builder.Services.AddSingleton<DatabaseInitializer>();
        builder.Services.AddHostedService(provider => provider.GetRequiredService<DatabaseInitializer>());
        builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("DatabaseHealthCheck", null);

        WebApplication application = builder.Build();

        application.MapDefaultEndpoints();

        await application.RunAsync();
    }
}
