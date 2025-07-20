namespace TRANSMUTANSTEIN.ChatServer;

public class TRANSMUTANSTEIN
{
    // TRUE If The Application Is Running In Development Mode Or FALSE If Not
    public static bool RunsInDevelopmentMode { get; set; } = true;

    public static void Main(string[] args)
    {
        // Create The Application Builder
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Set Static RunsInDevelopmentMode Property
        RunsInDevelopmentMode = builder.Environment.IsDevelopment();

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

        // Register Chat Service As Background Hosted Service
        builder.Services.AddHostedService<ChatService>();

        // Register Matchmaking Service As Background Hosted Service
        builder.Services.AddHostedService<MatchmakingService>();

        // Register Database Context Service
        builder.Services.AddTransient<MerrickContext>();

        // Add Chat Server Health Check
        builder.Services.AddHealthChecks().AddCheck<ChatServerHealthCheck>("TRANSMUTANSTEIN Chat Server Health Check");

        // Build The Application
        WebApplication app = builder.Build();

        // Configure Development-Specific Middleware
        if (app.Environment.IsDevelopment())
        {
            // Show Detailed Error Pages In Development
            app.UseDeveloperExceptionPage();
        }

        else
        {
            // Use Global Exception Handler In Production
            app.UseExceptionHandler("/error");
        }

        // Automatically Redirect HTTP Requests To HTTPS
        app.UseHttpsRedirection();

        // Enforce HTTPS With Strict Transport Security
        app.UseHsts();

        // Add Basic Security Headers Middleware
        app.Use(async (context, next) =>
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
        app.MapDefaultEndpoints();

        // Run The Application
        app.Run();
    }
}
