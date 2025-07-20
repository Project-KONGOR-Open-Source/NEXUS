namespace KONGOR.MasterServer;

public class KONGOR
{
    // The Count Of Seconds Since The UNIX Epoch (Epochalypse = 19.01.2038 @ 03:14:07 UTC)
    public static long ServerStartEpochTime { get; private set; } = default;

    // TRUE If The Application Is Running In Development Mode Or FALSE If Not
    public static bool RunsInDevelopmentMode { get; private set; } = true;

    public static void Main(string[] args)
    {
        // Create The Application Builder
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Set Static ServerStartEpochTime Property
        ServerStartEpochTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Set Static RunsInDevelopmentMode Property
        RunsInDevelopmentMode = builder.Environment.IsDevelopment();

        // Map User-Defined Configuration Section
        builder.Services.Configure<OperationalConfiguration>(builder.Configuration.GetRequiredSection(OperationalConfiguration.ConfigurationSection));

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

        // Add Distributed Cache; The Connection String Maps To The "distributed-cache" Resource Defined In ASPIRE.AppHost
        builder.AddRedisClient("DISTRIBUTED-CACHE");

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
                options.RequestBodyLogLimit = 4096; // 4KB Request Body Limit
                options.ResponseBodyLogLimit = 4096; // 4KB Response Body Limit
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

        // Build The Application
        WebApplication app = builder.Build();

        // Configure Development-Specific Middleware
        if (app.Environment.IsDevelopment())
        {
            // Show Detailed Error Pages In Development
            app.UseDeveloperExceptionPage();

            // Enable HTTP Request/Response Logging
            app.UseHttpLogging();

            // Enable Swagger API Documentation
            app.UseSwagger();

            // Configure Swagger UI With Custom Styling
            app.UseSwaggerUI(options =>
            {
                options.InjectStylesheet("swagger.css");
                options.DocumentTitle = "KONGOR Master Server API";
            });

            // Serve Static Files For Swagger CSS
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "Resources", "CSS")),
                RequestPath = "/swagger"
            });
        }

        else
        {
            // Use Global Exception Handler In Production
            app.UseExceptionHandler("/error");
        }

        // Enable Rate Limiting (Before Other Processing)
        app.UseRateLimiter();

        // Automatically Redirect HTTP Requests To HTTPS
        app.UseHttpsRedirection();

        // Enforce HTTPS With Strict Transport Security
        app.UseHsts();

        // Add Security Headers Middleware
        app.Use(async (context, next) =>
        {
            // Prevent MIME Type Sniffing
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            
            // Prevent Page From Being Displayed In Frames
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            
            // Enable XSS Protection
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
            
            // Control Referrer Information
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            
            // Content Security Policy For API
            context.Response.Headers.Append("Content-Security-Policy", "default-src 'none'; frame-ancestors 'none';");
            
            await next();
        });

        // Map Aspire Default Health Check Endpoints
        app.MapDefaultEndpoints();

        // Map MVC Controllers With Rate Limiting
        app.MapControllers().RequireRateLimiting(RateLimiterPolicies.Relaxed);

        // Run The Application
        app.Run();
    }
}
