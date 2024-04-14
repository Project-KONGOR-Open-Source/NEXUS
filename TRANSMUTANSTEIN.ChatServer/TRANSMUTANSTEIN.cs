namespace TRANSMUTANSTEIN.ChatServer;

public class TRANSMUTANSTEIN
{
    // TRUE If The Application Is Running In Development Mode Or False If Not
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
            options.EnableDetailedErrors(builder.Environment.IsDevelopment());

            // Suppress Warning Regarding Enabled Sensitive Data Logging, Since It Is Only Enabled In The Development Environment
            // https://github.com/dotnet/efcore/blob/main/src/EFCore/Properties/CoreStrings.resx (LogSensitiveDataLoggingEnabled)
            options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
                .ConfigureWarnings(warnings => warnings.Log((Id: CoreEventId.SensitiveDataLoggingEnabledWarning, Level: LogLevel.Trace)));

            options.EnableThreadSafetyChecks();
        });

        // Add Garnet (Drop-In Replacement For Redis); The Connection String Maps To The "cache" Resource Defined In ASPIRE.AppHost
        builder.AddRedisClient("cache", settings => settings.ConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__cache"));

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

        // Run The Application
        app.Run();
    }
}
