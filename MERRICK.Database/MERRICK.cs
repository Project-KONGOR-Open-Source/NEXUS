namespace MERRICK.Database;

/// <summary>
///     My name is Merrick, and I manage the store ... the data store.
/// </summary>
public class MERRICK
{
    public static async Task Main(string[] args)
    {
        // Create The Application Builder
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add Aspire Service Defaults
        builder.AddServiceDefaults();

        // Add The Database Context
        builder.AddSqlServerDbContext<MerrickContext>("MERRICK", configureSettings: null, configureDbContextOptions: options =>
        {
            // Enable Detailed Error Messages In Development Environment
            options.EnableDetailedErrors(builder.Environment.IsDevelopment());

            // Suppress Warning Regarding Enabled Sensitive Data Logging, Since It Is Only Enabled In The Development Environment
            // https://github.com/dotnet/efcore/blob/main/src/EFCore/Properties/CoreStrings.resx (LogSensitiveDataLoggingEnabled)
            options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
                .ConfigureWarnings(warnings => warnings.Log((Id: CoreEventId.SensitiveDataLoggingEnabledWarning, Level: LogLevel.Trace)));

            // Enable Thread Safety Checks For Entity Framework
            options.EnableThreadSafetyChecks();
        });

        // Add Database Initializer Telemetry
        builder.Services.AddOpenTelemetry().WithTracing(tracing => tracing.AddSource(DatabaseInitializer.ActivitySourceName));

        // Register Database Initializer As Singleton Service For Dependency Injection
        builder.Services.AddSingleton<DatabaseInitializer>();

        // Register Database Initializer As Hosted Service For Background Execution At Application Startup
        builder.Services.AddHostedService(provider => provider.GetRequiredService<DatabaseInitializer>());

        // Add Database Health Check
        builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("MERRICK Database Health Check");

        // Build The Application
        WebApplication application = builder.Build();

        // Configure Development-Specific Middleware
        if (application.Environment.IsDevelopment())
        {
            // Show Detailed Error Pages In Development
            application.UseDeveloperExceptionPage();
        }

        else
        {
            // Use Global Exception Handler In Production
            application.UseExceptionHandler("/error");
        }

        // Automatically Redirect HTTP Requests To HTTPS
        application.UseHttpsRedirection();

        // Enforce HTTPS With Strict Transport Security
        application.UseHsts();

        // Add Basic Security Headers Middleware
        application.Use(async (context, next) =>
        {
            // Prevent MIME Type Sniffing
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            
            // Prevent Page From Being Displayed In Frames
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            
            // Enable XSS Protection
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
            
            await next();
        });

        // Map Aspire Default Health Check Endpoints
        application.MapDefaultEndpoints();

        // Run The Application
        await application.RunAsync();
    }
}
