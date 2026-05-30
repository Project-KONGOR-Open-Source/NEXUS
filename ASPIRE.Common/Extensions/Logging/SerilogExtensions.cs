namespace ASPIRE.Common.Extensions.Logging;

public static class SerilogExtensions
{
    public static TBuilder AddSerilogLogging<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        string applicationName = builder.Environment.ApplicationName;
        string? logServerURL = builder.Configuration.GetConnectionString("log-server");

        builder.Services.AddSerilog(loggerConfiguration =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithThreadId()
                .Enrich.WithExceptionDetails()
                .WriteTo.File($"logs/{applicationName}..log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30, flushToDiskInterval: TimeSpan.FromSeconds(1));

            // TODO: Move Logs To Repository Root Once The Source Code Moves To A "source" Directory

            if (string.IsNullOrWhiteSpace(logServerURL) is false)
                loggerConfiguration.WriteTo.Seq(logServerURL);
        },

        // Forwards Every Serilog Event To The Registered Microsoft.Extensions.Logging Providers (Including OpenTelemetry), So Those Providers (And Therefore The Aspire Dashboard) Also Receive Every Log Event
        // The Console And OpenTelemetry Destinations Are Owned By Those Forwarded Providers, So Serilog Itself Only Adds The File And Seq Sinks; This Keeps One Writer Per Destination And Avoids Duplicate Log Entries
        writeToProviders: true);

        return builder;
    }

    public static WebApplication UseSerilogLogging(this WebApplication application)
    {
        // Emit One Structured Log Event Per HTTP Request, Capturing Its Method, Path, Status Code, And Elapsed Time
        application.UseSerilogRequestLogging();

        return application;
    }
}
