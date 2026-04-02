namespace ASPIRE.ApplicationHost;

public class ASPIRE
{
    public static void Main(string[] args)
    {
        // Create Distributed Application Builder
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

        // Get Configuration From Environment Variables And User Secrets
        IConfiguration configuration = new ConfigurationBuilder().AddEnvironmentVariables().AddUserSecrets<ASPIRE>(optional: true).Build();

        // Get Chat Server Configuration
        IConfigurationSection chatServerConfiguration = builder.Configuration.GetRequiredSection("ChatServer");
        string chatServerHost = chatServerConfiguration.GetValue<string?>("Host") ?? throw new NullReferenceException("Chat Server Host Is NULL");
        int chatServerClientConnectionsPort = chatServerConfiguration.GetValue<int?>("ClientPort") ?? throw new NullReferenceException("Chat Server Client Connections Port Is NULL");
        int chatServerMatchServerConnectionsPort = chatServerConfiguration.GetValue<int?>("MatchServerPort") ?? throw new NullReferenceException("Chat Server Match Server Connections Port Is NULL");
        int chatServerMatchServerManagerConnectionsPort = chatServerConfiguration.GetValue<int?>("MatchServerManagerPort") ?? throw new NullReferenceException("Chat Server Match Server Manager Connections Port Is NULL");

        // Get Infrastructure Configuration
        IConfigurationSection infrastructureConfiguration = builder.Configuration.GetRequiredSection("Infrastructure");
        string gateway = infrastructureConfiguration.GetValue<string?>("Gateway") ?? throw new NullReferenceException("Infrastructure Gateway Is NULL");

        // Set Distributed Cache Password Parameter Name And Environment Variable Name
        const string distributedCachePasswordParameterName = "distributed-cache-password";
        const string distributedCachePasswordEnvironmentVariableName = "DISTRIBUTED_CACHE_PASSWORD";

        // Attempt To Resolve Distributed Cache Password From Configuration In Order Of Priority: 1) User Secrets, 2) Environment Variables
        string? resolvedDistributedCachePassword = configuration[$"Parameters:{distributedCachePasswordParameterName}"] ?? configuration[distributedCachePasswordEnvironmentVariableName];

        // Populate Distributed Cache Password If Available In User Secrets Or Environment Variables
        IResourceBuilder<ParameterResource> distributedCachePassword = resolvedDistributedCachePassword is not null
            ? builder.AddParameter(distributedCachePasswordParameterName, resolvedDistributedCachePassword, secret: true)
            : builder.AddParameter(distributedCachePasswordParameterName, secret: true);

        // Add Distributed Cache Resource
        IResourceBuilder<RedisResource> distributedCache = builder.AddRedis("distributed-cache", password: distributedCachePassword)
            .WithImageTag("latest") // Latest Redis Image: https://github.com/redis/redis/releases/latest
            .WithLifetime(ContainerLifetime.Persistent).WithDataVolume("distributed-cache-data"); // Persist Cached Data Between Distributed Application Restarts But Not Between Resource Container Restarts

        // Create Resource Relationship After Parent Resource Is Defined
        distributedCachePassword
            .WithDescription("Distributed Cache Password") // Add Description To Parameter Resource
            .WithParentRelationship(distributedCache); // Set Distributed Cache As Parent Resource

        // Create Distributed Cache Dashboard Resource
        Action<IResourceBuilder<RedisInsightResource>> distributedCacheDashboard = builder => builder
            .WithImageTag("latest") // Latest Redis Insight Image: https://github.com/RedisInsight/RedisInsight/releases/latest
            .WithLifetime(ContainerLifetime.Persistent).WithDataVolume("distributed-cache-dashboard-data") // Persist Cached Data Between Distributed Application Restarts But Not Between Resource Container Restarts
            .WithEnvironment("RI_ACCEPT_TERMS_AND_CONDITIONS", "true") // Automatically Accept Terms And Conditions: https://redis.io/docs/latest/operate/redisinsight/configuration/
            .WithParentRelationship(distributedCache); // Set Distributed Cache As Parent Resource

        // Add Distributed Cache Dashboard Resource
        distributedCache.WithRedisInsight(distributedCacheDashboard, containerName: "distributed-cache-dashboard");

        // Set Database Password Parameter Name And Environment Variable Name
        const string databasePasswordParameterName = "database-password";
        const string databasePasswordEnvironmentVariableName = "DATABASE_PASSWORD";

        // Attempt To Resolve Database Password From Configuration In Order Of Priority: 1) User Secrets, 2) Environment Variables
        string? resolvedDatabasePassword = configuration[$"Parameters:{databasePasswordParameterName}"] ?? configuration[databasePasswordEnvironmentVariableName];

        // Populate Database Password If Available In User Secrets Or Environment Variables
        IResourceBuilder<ParameterResource> databasePassword = resolvedDatabasePassword is not null
            ? builder.AddParameter(databasePasswordParameterName, resolvedDatabasePassword, secret: true)
            : builder.AddParameter(databasePasswordParameterName, secret: true);

        // Configure Database Name Based On Environment
        string databaseName = builder.Environment.IsProduction() ? "production" : "development";

        // Using The Default SQL Server Port Allows The Database To Be Accessible With Just The Host Name/Address And No Port Number (e.g. 127.0.0.1)
        // If The SQL Server Resource Does Not Define A Port, Then It Will Be Randomly Assigned One, And The Connection Address Format Will Be {HOST},{PORT} (e.g. 127.0.0.1,51433)
        // While Aspire's Service Orchestration Is Not Running, The Port To Connect Directly To The Running SQL Server Container Can Be Found In Docker (e.g. "docker container list")
        const int databasePort = 1433;

        // Configure SQL Server Data Directory
        string userHomeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string databaseDirectory = Path.Combine(userHomeDirectory, "SQL", "MERRICK", databaseName);

        // Add SQL Server Container With Persistent Data In The Current User's Directory (Cross-Platform)
        IResourceBuilder<SqlServerServerResource> databaseServer = builder.AddSqlServer("database-server", password: databasePassword, port: databasePort)
            .WithImageTag("2022-latest") // SQL Server Image Tags: https://mcr.microsoft.com/en-gb/artifact/mar/mssql/server/tags
            .WithLifetime(ContainerLifetime.Persistent).WithDataBindMount(source: databaseDirectory) // Persist SQL Server Data Both Between Distributed Application Restarts And Resource Container Restarts
            .WithEnvironment("ACCEPT_EULA", "Y").WithEnvironment("MSSQL_PID", "Developer"); // SQL Server Image Information: https://mcr.microsoft.com/en-gb/artifact/mar/mssql/server/about

        // Create Resource Relationship After Parent Resource Is Defined
        databasePassword
            .WithDescription("Database Password") // Add Description To Parameter Resource
            .WithParentRelationship(databaseServer); // Set Database Server As Parent Resource

        // Add SQL Server Database Resource
        IResourceBuilder<SqlServerDatabaseResource> database = databaseServer.AddDatabase("database", databaseName)
            .WithParentRelationship(databaseServer); // Set Database Server As Parent Resource

        // Add Database Project
        builder.AddProject<MERRICK>("database-context", builder.Environment.IsProduction() ? "MERRICK.DatabaseContext Production" : "MERRICK.DatabaseContext Development")
            .WithReference(database, connectionName: "MERRICK").WaitFor(database) // Connect To SQL Server Database And Wait For It To Start
            .WithParentRelationship(databaseServer) // Set Database Server As Parent Resource
            .WithEnvironment("INFRASTRUCTURE_GATEWAY", gateway);

        // Add Master Server Project
        builder.AddProject<KONGOR>("master-server", builder.Environment.IsProduction() ? "KONGOR.MasterServer Production" : "KONGOR.MasterServer Development")
            .WithReference(database, connectionName: "MERRICK").WaitFor(database) // Connect To SQL Server Database And Wait For It To Start
            .WithReference(distributedCache, connectionName: "DISTRIBUTED-CACHE").WaitFor(distributedCache) // Connect To Distributed Cache And Wait For It To Start
            .WithEnvironment("CHAT_SERVER_HOST", chatServerHost)
            .WithEnvironment("CHAT_SERVER_PORT_CLIENT", chatServerClientConnectionsPort.ToString())
            .WithEnvironment("CHAT_SERVER_PORT_MATCH_SERVER", chatServerMatchServerConnectionsPort.ToString())
            .WithEnvironment("CHAT_SERVER_PORT_MATCH_SERVER_MANAGER", chatServerMatchServerManagerConnectionsPort.ToString())
            .WithEnvironment("INFRASTRUCTURE_GATEWAY", gateway);

        // Add Chat Server Project
        builder.AddProject<TRANSMUTANSTEIN>("chat-server", builder.Environment.IsProduction() ? "TRANSMUTANSTEIN.ChatServer Production" : "TRANSMUTANSTEIN.ChatServer Development")
            .WithReference(database, connectionName: "MERRICK").WaitFor(database) // Connect To SQL Server Database And Wait For It To Start
            .WithReference(distributedCache, connectionName: "DISTRIBUTED-CACHE").WaitFor(distributedCache) // Connect To Distributed Cache And Wait For It To Start
            .WithEnvironment("CHAT_SERVER_HOST", chatServerHost)
            .WithEnvironment("CHAT_SERVER_PORT_CLIENT", chatServerClientConnectionsPort.ToString())
            .WithEnvironment("CHAT_SERVER_PORT_MATCH_SERVER", chatServerMatchServerConnectionsPort.ToString())
            .WithEnvironment("CHAT_SERVER_PORT_MATCH_SERVER_MANAGER", chatServerMatchServerManagerConnectionsPort.ToString())
            .WithEnvironment("INFRASTRUCTURE_GATEWAY", gateway);

        // Add Web Portal API Project
        IResourceBuilder<ProjectResource> webPortalAPI = builder.AddProject<ZORGATH>("web-portal-api", builder.Environment.IsProduction() ? "ZORGATH.WebPortal.API Production" : "ZORGATH.WebPortal.API Development")
            .WithReference(database, connectionName: "MERRICK").WaitFor(database) // Connect To SQL Server Database And Wait For It To Start
            .WithEnvironment("INFRASTRUCTURE_GATEWAY", gateway);

        // Add Local STMP Server In Development
        if (builder.Environment.IsDevelopment())
        {
            IResourceBuilder<ContainerResource> smtpServer = builder.AddContainer("smtp-server", "axllent/mailpit")
                .WithImageTag("latest") // Latest MailPit Image: https://github.com/axllent/mailpit/releases/latest
                .WithLifetime(ContainerLifetime.Persistent)
                .WithEndpoint(port: 1025, targetPort: 1025, name: "smtp", scheme: "tcp") // Default SMTP Port
                .WithHttpEndpoint(port: 8025, targetPort: 8025, name: "http"); // Default Web UI Port

            webPortalAPI.WaitFor(smtpServer);
        }

        // Populate AWS SES Configuration In Staging/Production/etc.
        else
        {
            // Set SMTP Host Parameter Name And Environment Variable Name
            const string smtpHostParameterName = "smtp-host";
            const string smtpHostEnvironmentVariableName = "SMTP_HOST";

            // Attempt To Resolve SMTP Host From Configuration In Order Of Priority: 1) User Secrets, 2) Environment Variables
            string? resolvedSMTPHost = configuration[$"Parameters:{smtpHostParameterName}"] ?? configuration[smtpHostEnvironmentVariableName];

            // Populate SMTP Host If Available In User Secrets Or Environment Variables
            IResourceBuilder<ParameterResource> smtpHost = resolvedSMTPHost is not null
                ? builder.AddParameter(smtpHostParameterName, resolvedSMTPHost)
                : builder.AddParameter(smtpHostParameterName);

            // Set SMTP Port Parameter Name And Environment Variable Name
            const string smtpPortParameterName = "smtp-port";
            const string smtpPortEnvironmentVariableName = "SMTP_PORT";

            // Attempt To Resolve SMTP Port From Configuration In Order Of Priority: 1) User Secrets, 2) Environment Variables
            string? resolvedSMTPPort = configuration[$"Parameters:{smtpPortParameterName}"] ?? configuration[smtpPortEnvironmentVariableName];

            // Populate SMTP Port If Available In User Secrets Or Environment Variables
            IResourceBuilder<ParameterResource> smtpPort = resolvedSMTPPort is not null
                ? builder.AddParameter(smtpPortParameterName, resolvedSMTPPort)
                : builder.AddParameter(smtpPortParameterName);

            // Set SMTP Username Parameter Name And Environment Variable Name
            const string smtpUsernameParameterName = "smtp-username";
            const string smtpUsernameEnvironmentVariableName = "SMTP_USERNAME";

            // Attempt To Resolve SMTP Username From Configuration In Order Of Priority: 1) User Secrets, 2) Environment Variables
            string? resolvedSMTPUsername = configuration[$"Parameters:{smtpUsernameParameterName}"] ?? configuration[smtpUsernameEnvironmentVariableName];

            // Populate SMTP Username If Available In User Secrets Or Environment Variables
            IResourceBuilder<ParameterResource> smtpUsername = resolvedSMTPUsername is not null
                ? builder.AddParameter(smtpUsernameParameterName, resolvedSMTPUsername, secret: true)
                : builder.AddParameter(smtpUsernameParameterName, secret: true);

            // Set SMTP Password Parameter Name And Environment Variable Name
            const string smtpPasswordParameterName = "smtp-password";
            const string smtpPasswordEnvironmentVariableName = "SMTP_PASSWORD";

            // Attempt To Resolve SMTP Password From Configuration In Order Of Priority: 1) User Secrets, 2) Environment Variables
            string? resolvedSMTPPassword = configuration[$"Parameters:{smtpPasswordParameterName}"] ?? configuration[smtpPasswordEnvironmentVariableName];

            // Populate SMTP Password If Available In User Secrets Or Environment Variables
            IResourceBuilder<ParameterResource> smtpPassword = resolvedSMTPPassword is not null
                ? builder.AddParameter(smtpPasswordParameterName, resolvedSMTPPassword, secret: true)
                : builder.AddParameter(smtpPasswordParameterName, secret: true);

            // Pass SMTP Configuration To Web Portal API As Environment Variables That Override The "Operational:SMTP" Configuration Section
            // The "__" Separator In Environment Variable Names Maps To ":" In ASP.NET Core's Configuration System (e.g. "Operational__SMTP__Host" Resolves To "Operational:SMTP:Host")
            // These Environment Variables Are Set On The Child Process Before It Starts, So They Are Available During Configuration Building And Before IOptions<T> Is Bound
            webPortalAPI
                .WithEnvironment("Operational__SMTP__Host", smtpHost)
                .WithEnvironment("Operational__SMTP__Port", smtpPort)
                .WithEnvironment("Operational__SMTP__Username", smtpUsername)
                .WithEnvironment("Operational__SMTP__Password", smtpPassword);

            // Create Resource Relationships After Parent Resource Is Defined
            smtpHost.WithDescription("SMTP Host").WithParentRelationship(webPortalAPI);
            smtpPort.WithDescription("SMTP Port").WithParentRelationship(webPortalAPI);
            smtpUsername.WithDescription("SMTP Username").WithParentRelationship(webPortalAPI);
            smtpPassword.WithDescription("SMTP Password").WithParentRelationship(webPortalAPI);
        }

        // Add Web Portal UI Project
        builder.AddProject<DAWNBRINGER>("web-portal-ui", builder.Environment.IsProduction() ? "DAWNBRINGER.WebPortal.UI Production" : "DAWNBRINGER.WebPortal.UI Development")
            .WithReference(webPortalAPI).WaitFor(webPortalAPI) // Connect To Web Portal API And Wait For It To Start
            .WithEnvironment("INFRASTRUCTURE_GATEWAY", gateway);

        // Start Orchestrating Distributed Application
        builder.Build().Run();
    }
}
