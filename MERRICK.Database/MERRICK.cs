WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddDbContext<MerrickContext>(options =>
{
    options.UseSqlServer("MERRICK", connection => connection.MigrationsAssembly(typeof(Program).Assembly.GetName().Name));
});

builder.Services.AddOpenTelemetry().WithTracing(tracing => tracing.AddSource(DatabaseInitialiser.ActivitySourceName));
builder.Services.AddSingleton<DatabaseInitialiser>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<DatabaseInitialiser>());
builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("DatabaseHealthCheck", null);

WebApplication application = builder.Build();

application.MapDefaultEndpoints();

await application.RunAsync();
