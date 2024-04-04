namespace TRANSMUTANSTEIN.ChatServer;

public class TRANSMUTANSTEIN
{
    public static bool RunsInDevelopmentMode { get; set; }

    public static IServiceProvider ServiceProvider { get; set; } = null!;

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
            options.EnableDetailedErrors(builder.Environment.IsDevelopment());

            // Suppress Warning Regarding Enabled Sensitive Data Logging, Since It Is Only Enabled In The Development Environment
            // https://github.com/dotnet/efcore/blob/main/src/EFCore/Properties/CoreStrings.resx (LogSensitiveDataLoggingEnabled)
            options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
                .ConfigureWarnings(warnings => warnings.Log((Id: CoreEventId.SensitiveDataLoggingEnabledWarning, Level: LogLevel.Trace)));

            options.EnableThreadSafetyChecks();
        });

        // Host The Chat Service
        builder.Services.AddHostedService<ChatService>();

        // Register The Database Context Service
        builder.Services.AddTransient<MerrickContext>();

        // Add Database Health Check
        builder.Services.AddHealthChecks().AddCheck<ChatServerHealthCheck>("TRANSMUTANSTEIN Chat Server Health Check");

        // Build The Application
        WebApplication app = builder.Build();

        // Map Aspire Default Endpoints
        app.MapDefaultEndpoints();

        // Set A Global Service Provider
        ServiceProvider = app.Services; // TODO: Find A Smarter Way To Provide Services; Figure Out Dependency Injection In This Project Setup (Reflection-Based Routing)

        // Run The Application
        app.Run();
    }
}
