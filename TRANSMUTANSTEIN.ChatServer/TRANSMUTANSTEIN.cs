namespace TRANSMUTANSTEIN.ChatServer;

public class TRANSMUTANSTEIN
{
    public static void Main(string[] args)
    {
        // Create The Application Builder
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add Aspire Service Defaults
        builder.AddServiceDefaults();

        // Add The Database Context
        builder.AddSqlServerDbContext<MerrickContext>("MERRICK", configureSettings: null, configureDbContextOptions: options =>
        {
            // Specify Migrations History Table And Schema
            options.UseSqlServer(sqlServerOptionsAction: sqlServerOptions => sqlServerOptions.MigrationsHistoryTable("MigrationsHistory", MerrickContext.MetadataSchema));

            // Enable Detailed Error Messages In Development Environment
            options.EnableDetailedErrors(builder.Environment.IsDevelopment());

            // Suppress Warning Regarding Enabled Sensitive Data Logging, Since It Is Only Enabled In The Development Environment
            // https://github.com/dotnet/efcore/blob/main/src/EFCore/Properties/CoreStrings.resx (LogSensitiveDataLoggingEnabled)
            options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
                .ConfigureWarnings(warnings => warnings.Log((Id: CoreEventId.SensitiveDataLoggingEnabledWarning, Level: LogLevel.Trace)));

            // Enable Thread Safety Checks For Entity Framework
            options.EnableThreadSafetyChecks();
        });

        // Add Distributed Cache; The Connection String Maps To The "distributed-cache" Resource Defined In ASPIRE.ApplicationHost
        builder.AddRedisClient("DISTRIBUTED-CACHE");

        // Register IDatabase From IConnectionMultiplexer
        builder.Services.AddSingleton<IDatabase>(serviceProvider => serviceProvider.GetRequiredService<IConnectionMultiplexer>().GetDatabase());

        // Register Chat Service As Background Hosted Service
        builder.Services.AddHostedService<ChatService>();

        // Register Matchmaking Service As Background Hosted Service
        builder.Services.AddHostedService<MatchmakingService>();

        // Register Flood Prevention Service As Background Hosted Service With Support For Dependency Injection
        builder.Services.AddSingleton<FloodPreventionService>();
        builder.Services.AddHostedService(serviceProvider => serviceProvider.GetRequiredService<FloodPreventionService>());

        // Register Database Context Service
        builder.Services.AddTransient<MerrickContext>();

        // Add Chat Server Health Check
        builder.Services.AddHealthChecks().AddCheck<ChatServerHealthCheck>("TRANSMUTANSTEIN Chat Server Health Check");

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

        // Enforce HTTPS With Strict Transport Security
        application.UseHsts();

        // Automatically Redirect HTTP Requests To HTTPS
        application.UseHttpsRedirection();

        // Add Security Headers Middleware
        application.Use(async (context, next) =>
        {
            IHeaderDictionary headers = context.Response.Headers;

            // Prevent MIME Type Sniffing
            headers["X-Content-Type-Options"] = "nosniff";

            // Control Referrer Information
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            // Apply Restrictive CSP Only To API Endpoints
            if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
            {
                headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none';";
            }

            await next();
        });

        // Map Aspire Default Health Check Endpoints
        application.MapDefaultEndpoints();

        // Run The Application
        application.Run();
    }
}
