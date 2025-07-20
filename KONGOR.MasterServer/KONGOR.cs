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
            options.EnableDetailedErrors(builder.Environment.IsDevelopment());

            // Suppress Warning Regarding Enabled Sensitive Data Logging, Since It Is Only Enabled In The Development Environment
            // https://github.com/dotnet/efcore/blob/main/src/EFCore/Properties/CoreStrings.resx (LogSensitiveDataLoggingEnabled)
            options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
                .ConfigureWarnings(warnings => warnings.Log((Id: CoreEventId.SensitiveDataLoggingEnabledWarning, Level: LogLevel.Trace)));

            options.EnableThreadSafetyChecks();
        });

        // The Connection String Maps To The "distributed-cache" Resource Defined In ASPIRE.AppHost
        builder.AddRedisClient("DISTRIBUTED-CACHE");

        // Add Memory Cache
        builder.Services.AddMemoryCache();

        // Add MVC Controllers
        builder.Services.AddControllers();

        // Add Comprehensive Error Responses
        if (builder.Environment.IsDevelopment())
            builder.Services.AddProblemDetails();

        // Add Swagger
        builder.Services.AddSwaggerGen(options =>
        {
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

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();

            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                options.InjectStylesheet("swagger.css");
                options.DocumentTitle = "KONGOR Master Server API";
            });

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "Resources", "CSS")),
                RequestPath = "/swagger"
            });
        }

        else
        {
            app.UseExceptionHandler("/error");
        }

        // Map Aspire Default Endpoints
        app.MapDefaultEndpoints();

        // Map MVC Controllers
        app.MapControllers();

        // Run The Application
        app.Run();
    }
}
