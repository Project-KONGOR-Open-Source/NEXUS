namespace KONGOR.MasterServer;

public class KONGOR
{
    // The Count Of Seconds Since The UNIX Epoch (Epochalypse = 19.01.2038 @ 03:14:07 UTC)
    public static long ServerStartEpochTime { get; private set; } = default;

    public static async Task Main(string[] args)
    {
        // Create The Application Builder
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Set Static ServerStartEpochTime Property
        ServerStartEpochTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Map User-Defined Configuration Section
        builder.Services.Configure<OperationalConfiguration>(builder.Configuration.GetRequiredSection(OperationalConfiguration.ConfigurationSection));

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

        // Add Memory Cache Service
        builder.Services.AddMemoryCache();

        // Add Rate Limiting Service To Protect Against Abuse And DoS Attacks
        builder.Services.AddRateLimiter(options =>
        {
            // Relaxed Limits For General API Endpoints
            options.AddSlidingWindowLimiter(policyName: RateLimiterPolicies.Relaxed, policy =>
            {
                policy.PermitLimit = 100;
                policy.Window = TimeSpan.FromMinutes(1);
                policy.SegmentsPerWindow = 6; // 10 Seconds Per Sliding Window Segment
                policy.QueueLimit = 10;
                policy.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });

            // Strict Limits For Authentication And Other Sensitive Endpoints
            options.AddSlidingWindowLimiter(policyName: RateLimiterPolicies.Strict, policy =>
            {
                policy.PermitLimit = 5;
                policy.Window = TimeSpan.FromMinutes(1);
                policy.SegmentsPerWindow = 6; // 10 Seconds Per Sliding Window Segment
                policy.QueueLimit = 0;
                policy.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });
        });

        // Add HTTP Request/Response Logging For Debugging In Development
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddHttpLogging(options =>
            {
                options.LoggingFields = HttpLoggingFields.RequestPropertiesAndHeaders | HttpLoggingFields.ResponsePropertiesAndHeaders;
                options.RequestBodyLogLimit = 4096; /* 4KB Request Body Limit */ options.ResponseBodyLogLimit = 4096; /* 4KB Response Body Limit */
            });
        }

        // Add MVC Controllers Support
        builder.Services.AddControllers();

        // Add Comprehensive Error Response Detail In Development Environment
        if (builder.Environment.IsDevelopment())
            builder.Services.AddProblemDetails();

        // Add Swagger/OpenAPI Documentation Generation
        builder.Services.AddSwaggerGen(options =>
        {
            // Configure API Documentation Metadata
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "KONGOR Master Server API",
                Version = "v1",

                License = new OpenApiLicense
                {
                    Name = "Project KONGOR Open-Source License",
                    Url = new Uri("https://github.com/Project-KONGOR-Open-Source/ASPIRE/blob/main/license")
                },

                Contact = new OpenApiContact
                {
                    Name = "[K]ONGOR",
                    Url = new Uri("https://github.com/K-O-N-G-O-R"),
                    Email = "project.kongor@proton.me"
                }
            });
        });

        // Configure Forwarded Headers For Reverse Proxy Support
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            string proxy = builder.Configuration.GetValue<string>("INFRASTRUCTURE_GATEWAY") ?? "localhost";

            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;

            IPAddress[] proxyResolvedAddresses;

            try { proxyResolvedAddresses = Dns.GetHostAddresses(proxy); }
            catch (Exception exception) { throw new InvalidOperationException($@"Failed To Resolve Proxy Host ""{proxy}""", exception); }

            foreach (IPAddress proxyResolvedAddress in proxyResolvedAddresses)
                if (proxyResolvedAddress.AddressFamily is AddressFamily.InterNetwork or AddressFamily.InterNetworkV6)
                    options.KnownProxies.Add(proxyResolvedAddress);

            // Only Trust The Last Forwarded Header In The Chain
            options.ForwardLimit = 1;
        });

        // Build The Application
        WebApplication application = builder.Build();

        // Enable Forwarded Headers Middleware For Reverse Proxy Support
        application.UseForwardedHeaders();

        if (application.Services.GetService<IConnectionMultiplexer>() is IConnectionMultiplexer connectionMultiplexer)
        {
            // Purge All Session Cookies From Cache At Startup To Prevent Stale Authentication Data
            // Only Run If IConnectionMultiplexer Is Registered (Skipped In Tests That Use In-Process Test Doubles)

            await connectionMultiplexer.PurgeSessionCookies();
        }

        // Configure Development-Specific Middleware
        if (application.Environment.IsDevelopment())
        {
            // Show Detailed Error Pages In Development
            application.UseDeveloperExceptionPage();

            // Enable HTTP Request/Response Logging
            application.UseHttpLogging();
        }

        else
        {
            // Use Global Exception Handler In Production
            application.UseExceptionHandler("/error");
        }

        // Enable Swagger API Documentation
        application.UseSwagger();

        // Configure Swagger UI With Custom Styling
        application.UseSwaggerUI(options =>
        {
            options.InjectStylesheet("swagger.css");
            options.DocumentTitle = "KONGOR Master Server API";
        });

        // Serve Static Files For Swagger CSS
        application.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "Resources", "CSS")),
            RequestPath = "/swagger"
        });

        // Enable Rate Limiting (Before Other Processing)
        application.UseRateLimiter();

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

        // Map MVC Controllers With Rate Limiting
        application.MapControllers().RequireRateLimiting(RateLimiterPolicies.Relaxed);

        // Run The Application
        application.Run();
    }
}
