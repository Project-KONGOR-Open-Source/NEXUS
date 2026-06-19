namespace TRANSMUTANSTEIN.ChatServer;

public class TRANSMUTANSTEIN
{
    public static void Main(string[] arguments)
    {
        CreateApplication(arguments).Run();
    }

    /// <summary>
    ///     Builds and fully configures the chat server's web application, leaving it ready to run.
    ///     Extracted from <see cref="Main"/> so that integration tests can start the production wiring on their own ports without invoking the blocking run loop.
    /// </summary>
    public static WebApplication CreateApplication(string[] arguments)
    {
        // Create The Application Builder
        WebApplicationBuilder builder = WebApplication.CreateBuilder(arguments);

        // Add Aspire Service Defaults
        builder.AddServiceDefaults();

        // Add Serilog Logging
        builder.AddSerilogLogging();

        // Configure The Chat Server's Endpoints On Kestrel; The Three Chat Protocol Ports Are Each Served By Their Own Connection Handler
        // Because Configuring Any Endpoint In Code Makes Kestrel Ignore The Host's URL-Based Address Configuration ("ASPNETCORE_URLS"), The HTTP Surface (Health Checks) Is Re-Applied Here From That Same Configuration So That Both It And The Chat Protocol Endpoints Continue To Bind
        builder.WebHost.ConfigureKestrel(kestrelOptions =>
        {
            foreach (string applicationUrl in (Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                BindingAddress bindingAddress = BindingAddress.Parse(applicationUrl);

                Action<ListenOptions> configureHTTPEndpoint = listenOptions =>
                {
                    if (bindingAddress.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
                        listenOptions.UseHttps();
                };

                if (bindingAddress.Host is "*" or "+" or "0.0.0.0" or "::" or "[::]")
                    kestrelOptions.ListenAnyIP(bindingAddress.Port, configureHTTPEndpoint);

                else if (bindingAddress.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                    kestrelOptions.ListenLocalhost(bindingAddress.Port, configureHTTPEndpoint);

                else
                    kestrelOptions.Listen(IPAddress.Parse(bindingAddress.Host), bindingAddress.Port, configureHTTPEndpoint);
            }

            int clientConnectionsPort = int.Parse(Environment.GetEnvironmentVariable("CHAT_SERVER_PORT_CLIENT")
                ?? throw new NullReferenceException("Chat Server Port For Client Connections Is NULL"));

            int matchServerConnectionsPort = int.Parse(Environment.GetEnvironmentVariable("CHAT_SERVER_PORT_MATCH_SERVER")
                ?? throw new NullReferenceException("Chat Server Port For Match Server Connections Is NULL"));

            int matchServerManagerConnectionsPort = int.Parse(Environment.GetEnvironmentVariable("CHAT_SERVER_PORT_MATCH_SERVER_MANAGER")
                ?? throw new NullReferenceException("Chat Server Port For Match Server Manager Connections Is NULL"));

            bool isDevelopmentEnvironment = builder.Environment.IsDevelopment();

            // In Development, Log Each Connection's Raw Traffic To Aid Protocol Debugging; This Is Too Verbose For Other Environments
            void ConfigureChatEndpoint<THandler>(ListenOptions listenOptions) where THandler : ConnectionHandler
            {
                if (isDevelopmentEnvironment)
                    listenOptions.UseConnectionLogging();

                listenOptions.UseConnectionHandler<THandler>();
            }

            kestrelOptions.ListenAnyIP(clientConnectionsPort, ConfigureChatEndpoint<ClientConnectionHandler>);
            kestrelOptions.ListenAnyIP(matchServerConnectionsPort, ConfigureChatEndpoint<MatchServerConnectionHandler>);
            kestrelOptions.ListenAnyIP(matchServerManagerConnectionsPort, ConfigureChatEndpoint<MatchServerManagerConnectionHandler>);
        });

        // Configure Matchmaking Settings
        builder.Services.Configure<MatchmakingSettings>(builder.Configuration.GetSection(MatchmakingSettings.SectionName));

        // Configure Match Server Settings
        builder.Services.Configure<MatchServerSettings>(builder.Configuration.GetSection(MatchServerSettings.SectionName));

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

            // Add Interceptors
            options.AddMerrickInterceptors();
        });

        // Add Distributed Cache; The Connection String Maps To The "distributed-cache" Resource Defined In ASPIRE.ApplicationHost
        builder.AddRedisClient("DISTRIBUTED-CACHE");

        // Register IDatabase From IConnectionMultiplexer
        builder.Services.AddSingleton<IDatabase>(serviceProvider => serviceProvider.GetRequiredService<IConnectionMultiplexer>().GetDatabase());

        // Register Matchmaking Service As Background Hosted Service
        builder.Services.AddHostedService<MatchmakingService>();

        // Register Flood Prevention Service
        builder.Services.AddSingleton<FloodPreventionService>();

        // Register Account Logout Subscriber Service That Consumes Force-Logout Signals Published By The Master Server
        builder.Services.AddHostedService<LogoutMonitor>();

        // Register The Stale Host Reaper That Removes Cached Match Servers And Managers With No Live Chat Session
        builder.Services.AddHostedService<StaleHostReaper>();

        // Register Database Context Service
        builder.Services.AddTransient<MerrickContext>();

        // Add Chat Server Health Check
        builder.Services.AddHealthChecks().AddCheck<ChatServerHealthCheck>("TRANSMUTANSTEIN Chat Server Health Check");

        // Configure Forwarded Headers For Reverse Proxy Support
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            string proxy = Environment.GetEnvironmentVariable("INFRASTRUCTURE_GATEWAY") ?? throw new NullReferenceException("Infrastructure Gateway Is NULL");

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

        // Initialise The Chat Server's Static Logger Facade From The Host's Configured Logger, Before Any Connection Handler Or Hosted Service Runs
        Log.Initialise(application.Services.GetRequiredService<ILogger>());

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

        return application;
    }
}
