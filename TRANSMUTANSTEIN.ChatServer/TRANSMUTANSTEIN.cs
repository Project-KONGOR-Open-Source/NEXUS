namespace TRANSMUTANSTEIN.ChatServer;

public class TRANSMUTANSTEIN
{
    public static bool RunsInDevelopmentMode { get; set; }

    public static WebApplication? Application { get; set; }

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
        Application = builder.Build();

        // Map Aspire Default Endpoints
        Application.MapDefaultEndpoints();

        // Run The Application
        Application.Run();
    }
}
