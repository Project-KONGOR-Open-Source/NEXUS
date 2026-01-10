namespace MERRICK.DatabaseContext;

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
        builder.AddSqlServerDbContext<MerrickContext>("MERRICK", null, options =>
        {
            // Specify Migrations History Table And Schema
            options.UseSqlServer(sqlServerOptions =>
                sqlServerOptions.MigrationsHistoryTable("MigrationsHistory", MerrickContext.MetadataSchema));

            // Enable Detailed Error Messages In Development Environment
            options.EnableDetailedErrors(builder.Environment.IsDevelopment());

            // Suppress Warning Regarding Enabled Sensitive Data Logging, Since It Is Only Enabled In The Development Environment
            // https://github.com/dotnet/efcore/blob/main/src/EFCore/Properties/CoreStrings.resx (LogSensitiveDataLoggingEnabled)
            options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
                .ConfigureWarnings(warnings =>
                    warnings.Log((Id: CoreEventId.SensitiveDataLoggingEnabledWarning, Level: LogLevel.Trace)));

            // Enable Thread Safety Checks For Entity Framework
            options.EnableThreadSafetyChecks();
        });

        // Add Database Initializer Telemetry
        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing => tracing.AddSource(DatabaseInitializer.ActivitySourceName));

        // Register Database Initializer As Singleton Service For Dependency Injection
        builder.Services.AddSingleton<DatabaseInitializer>();

        // Register Database Initializer As Hosted Service For Background Execution At Application Startup
        builder.Services.AddHostedService(provider => provider.GetRequiredService<DatabaseInitializer>());

        // Add Database Health Check
        builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("MERRICK Database Health Check");

        // Configure Forwarded Headers For Reverse Proxy Support
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            string proxy = Environment.GetEnvironmentVariable("INFRASTRUCTURE_GATEWAY") ??
                           throw new NullReferenceException("Infrastructure Gateway Is NULL");

            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto |
                                       ForwardedHeaders.XForwardedHost;

            IPAddress[] proxyResolvedAddresses;

            try { proxyResolvedAddresses = Dns.GetHostAddresses(proxy); }
            catch (Exception exception)
            {
                throw new InvalidOperationException($@"Failed To Resolve Proxy Host ""{proxy}""", exception);
            }

            foreach (IPAddress proxyResolvedAddress in proxyResolvedAddresses)
            {
                if (proxyResolvedAddress.AddressFamily is AddressFamily.InterNetwork or AddressFamily.InterNetworkV6)
                {
                    options.KnownProxies.Add(proxyResolvedAddress);
                }
            }

            // Only Trust The Last Forwarded Header In The Chain
            options.ForwardLimit = 1;
        });

        // Build The Application
        WebApplication application = builder.Build();

        // Enable Forwarded Headers Middleware For Reverse Proxy Support
        application.UseForwardedHeaders();

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
        await application.RunAsync();
    }
}